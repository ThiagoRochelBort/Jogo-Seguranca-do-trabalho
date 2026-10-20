using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Desafios práticos (mini-jogos) da seção "Desafios práticos" do GDD: wire e lockout.
/// Diferente dos outros desafios, a tela é montada uma vez e atualizada no lugar,
/// porque os fios dependem da posição real dos bornes.
/// </summary>
public partial class GameUI
{
    // Estado comum dos desafios práticos
    CardData practical;
    int mistakes;
    bool practicalDone;
    string practicalHint;
    Text practicalStatus;

    // wire
    readonly List<Button> wireEpiBtns = new List<Button>();
    readonly HashSet<int> wireEpiSelected = new HashSet<int>();
    readonly HashSet<int> wireEpiDisabled = new HashSet<int>();
    readonly List<Button> wireLeft = new List<Button>();
    readonly List<Button> wireRight = new List<Button>();
    readonly HashSet<int> wireLinked = new HashSet<int>(); // índices de left já ligados
    bool wireEpiDone;
    int wireSelectedLeft = -1;
    RectTransform wireLines;

    // lockout
    readonly List<Button> lockBtns = new List<Button>();
    readonly List<int> lockOrder = new List<int>(); // botão k -> opção (passos primeiro, depois armadilhas)
    int lockNext;      // próximo passo certo (os anteriores já foram feitos)
    int lockHinted = -1;
    Text lockMachine;

    // ------------------------------------------------------------ Comum

    void StartPractical(CardData c, string typeLabel, string hint)
    {
        Clear();
        RefreshHud();
        practical = c;
        mistakes = 0;
        practicalDone = false;
        practicalHint = hint;
        shakeHome.Clear();

        CardHeader(c, typeLabel);
        practicalStatus = L("", Theme.FontSmall, FontStyle.Bold, Theme.TextMuted);
        UIFactory.ShrinkToFit(practicalStatus);
        practicalStatus.GetComponent<LayoutElement>().preferredHeight = Theme.HintLineHeight;
        UpdateStatus(null);
    }

    /// <summary>Faixa de baixo: habilidade + dica do passo atual (+ botão do passo do EPI no wire).</summary>
    void PracticalBar()
    {
        var c = practical;
        ChallengeBar(c, PracticalBar);
        ShowPracticalHint();
        if (practicalDone) HideAbility();
        if (c.Type == CardType.Wire && !wireEpiDone && !practicalDone)
            SetAction("Pronto para mexer", OnWireReady);
    }

    void ShowPracticalHint()
    {
        if (gm.ShieldActive) SetHint("Escudo da CIPA ativo. " + practicalHint, Theme.Green);
        else SetHint(practicalHint, Theme.Text);
    }

    IEnumerator AfterDelay(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }

    /// <summary>Linha "Erros: n de max" + a frase do último clique (vermelha se foi erro, verde se foi acerto).</summary>
    void UpdateStatus(string why, bool isError = true)
    {
        string text = "Erros: " + mistakes + " de " + gm.MaxMistakes(practical);
        if (!string.IsNullOrEmpty(why)) text += ".  " + why;
        practicalStatus.text = text;
        if (string.IsNullOrEmpty(why)) practicalStatus.color = Theme.TextMuted;
        else practicalStatus.color = isError ? Theme.Red : Theme.Green;
    }

    /// <summary>Soma um erro, mostra o porquê e encerra o desafio se chegar no limite.</summary>
    void Mistake(string why)
    {
        if (practicalDone) return;
        mistakes++;
        SoundManager.Play(Sfx.SmallBad);
        UpdateStatus(why);
        if (mistakes >= gm.MaxMistakes(practical)) Finish(false);
    }

    void Finish(bool success)
    {
        practicalDone = true;
        HideAbility();
        HideAction();
        if (practical.Type == CardType.Wire && wireFancy) WireFinishFx(success);
        if (practical.Type == CardType.Match && matchFancy && success) StartCoroutine(MatchBlinkFx());
        if (practical.Type == CardType.Hunt && huntFancy) HuntFinishFx(success);
        if (practical.Type == CardType.Lockout && lockFancy) LockFinishFx(success);
        StartCoroutine(FinishRoutine(success));
    }

    IEnumerator FinishRoutine(bool success)
    {
        yield return new WaitForSeconds(Theme.ResultPause);
        Resolve(practical, success);
    }

    /// <summary>O botão fica vermelho por um instante e depois volta ao estado certo.</summary>
    IEnumerator Flash(Button b, System.Action repaint)
    {
        UIFactory.SetStyle(b, Theme.Wrong);
        yield return new WaitForSeconds(Theme.FlashTime);
        repaint();
    }

    Transform MakeGrid(Vector2 cell, Vector2 spacing, int cols, int count)
    {
        var gridGo = new GameObject("Grade", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridGo.transform.SetParent(content, false);
        var grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = cell;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.childAlignment = TextAnchor.UpperCenter;
        int rows = Mathf.CeilToInt(count / (float)cols);
        gridGo.GetComponent<LayoutElement>().preferredHeight = rows * cell.y + Mathf.Max(0, rows - 1) * spacing.y;
        return gridGo.transform;
    }

    // ------------------------------------------------------------ wire: Ligar e proteger

    void ShowWire(CardData c)
    {
        StartPractical(c, "Desafio prático: Ligar e proteger",
            "Marque os EPIs certos e aperte Pronto para mexer.");
        wireEpiBtns.Clear();
        wireEpiSelected.Clear();
        wireEpiDisabled.Clear();
        wireLeft.Clear();
        wireRight.Clear();
        wireLinked.Clear();
        wireEpiDone = false;
        wireSelectedLeft = -1;

        // Com os campos visuais (epiIcons, wireColors, device) a bancada é desenhada; sem eles, botões simples
        wireFancy = WireIsFancy(c);
        if (wireFancy)
        {
            BuildWireScene(c);
            RefreshWire();
            PracticalBar();
            return;
        }

        var grid = MakeGrid(Theme.EpiCellSize, Theme.EpiSpacing, 3, c.epiItems.Length);
        for (int i = 0; i < c.epiItems.Length; i++)
        {
            int idx = i;
            wireEpiBtns.Add(UIFactory.MakeButton(grid, c.epiItems[i], () => OnWireEpi(idx), Theme.Secondary,
                Theme.OptionHeight, -1, Theme.FontBody));
        }

        // Bancada: bornes da fonte à esquerda, do equipamento à direita, fios no meio
        var bench = new GameObject("Bancada", typeof(RectTransform), typeof(LayoutElement));
        bench.transform.SetParent(content, false);
        bench.GetComponent<LayoutElement>().preferredHeight = Theme.WireAreaHeight;
        wireLines = UIFactory.Rect(bench.transform, "Fios", Vector2.zero, Vector2.one);

        int rows = Mathf.Max(c.left.Length, c.right.Length);
        for (int i = 0; i < c.left.Length; i++)
        {
            int idx = i;
            wireLeft.Add(ColumnButton(bench.transform, c.left[i], () => OnWireLeft(idx), true, i, c.left.Length, rows));
        }
        for (int j = 0; j < c.right.Length; j++)
        {
            int idx = j;
            wireRight.Add(ColumnButton(bench.transform, c.right[j], () => OnWireRight(idx), false, j, c.right.Length, rows));
        }

        RefreshWire();
        PracticalBar();
    }

    /// <summary>
    /// Botão numa das duas colunas (esquerda/direita) de uma área. rows = linhas da coluna mais alta;
    /// count = itens desta coluna, que fica centralizada na vertical.
    /// </summary>
    Button ColumnButton(Transform parent, string label, UnityEngine.Events.UnityAction action, bool leftSide, int row, int count, int rows)
    {
        var btn = UIFactory.MakeButton(parent, label, action, Theme.Secondary, Theme.ButtonHeight, -1, Theme.FontBody);
        var rt = btn.GetComponent<RectTransform>();
        float x0 = leftSide ? 0 : 1 - Theme.WireColumnWidth;
        float x1 = leftSide ? Theme.WireColumnWidth : 1;
        float r = row + (rows - count) / 2f;
        rt.anchorMin = new Vector2(x0, 1 - (r + 1) / rows);
        rt.anchorMax = new Vector2(x1, 1 - r / rows);
        rt.offsetMin = new Vector2(0, Theme.WireRowGap / 2);
        rt.offsetMax = new Vector2(0, -Theme.WireRowGap / 2);
        return btn;
    }

    void RefreshWire()
    {
        if (wireFancy)
        {
            RefreshWireScene();
            return;
        }
        var c = practical;
        for (int i = 0; i < wireEpiBtns.Count; i++)
        {
            var b = wireEpiBtns[i];
            if (wireEpiDone)
            {
                UIFactory.SetStyle(b, System.Array.IndexOf(c.epiCorrect, i) >= 0 ? Theme.Correct : Theme.Secondary);
                UIFactory.SetInteractable(b, false);
            }
            else
            {
                UIFactory.SetStyle(b, wireEpiSelected.Contains(i) ? Theme.Ability : Theme.Secondary);
                UIFactory.SetInteractable(b, !practicalDone && !wireEpiDisabled.Contains(i));
            }
        }

        bool active = wireEpiDone && !practicalDone;
        for (int i = 0; i < wireLeft.Count; i++)
        {
            bool linked = wireLinked.Contains(i);
            UIFactory.SetStyle(wireLeft[i], linked ? Theme.Correct : i == wireSelectedLeft ? Theme.Primary : Theme.Secondary);
            UIFactory.SetInteractable(wireLeft[i], active && !linked);
        }
        for (int j = 0; j < wireRight.Count; j++)
        {
            bool linked = IsRightLinked(j);
            UIFactory.SetStyle(wireRight[j], linked ? Theme.Correct : Theme.Secondary);
            UIFactory.SetInteractable(wireRight[j], active && !linked);
        }
    }

    bool IsRightLinked(int j)
    {
        foreach (int i in wireLinked)
            if (practical.links[i] == j) return true;
        return false;
    }

    void OnWireEpi(int i)
    {
        if (practicalDone || wireEpiDone || wireEpiDisabled.Contains(i)) return;
        if (!wireEpiSelected.Remove(i)) wireEpiSelected.Add(i);
        RefreshWire();
    }

    void OnWireReady()
    {
        if (practicalDone || wireEpiDone) return;
        if (new HashSet<int>(practical.epiCorrect).SetEquals(wireEpiSelected))
        {
            wireEpiDone = true;
            SoundManager.Play(Sfx.SmallOk);
            practicalHint = "EPI certo! Clique num fio da esquerda e depois no borne certo da direita.";
            HideAction();
            RefreshWire();
            PracticalBar();
            if (wireFancy) StartCoroutine(DressRoutine());
        }
        else
        {
            Mistake(practical.epiWhy);
            RefreshWire();
            if (wireFancy) WireEpiWrongFx();
        }
    }

    void OnWireLeft(int i)
    {
        if (practicalDone || !wireEpiDone || wireLinked.Contains(i)) return;
        wireSelectedLeft = wireSelectedLeft == i ? -1 : i;
        RefreshWire();
    }

    void OnWireRight(int j)
    {
        if (practicalDone || !wireEpiDone || IsRightLinked(j)) return;
        if (wireSelectedLeft < 0)
        {
            SetHint("Escolha primeiro um fio da esquerda.", Theme.Text);
            return;
        }

        int left = wireSelectedLeft;
        wireSelectedLeft = -1;
        if (practical.links[left] == j)
        {
            ConnectWire(left);
        }
        else
        {
            StartCoroutine(wireFancy ? WireCableFx(left, j, false) : TempLine(left, j));
            Mistake(practical.linkWhy);
            RefreshWire();
        }
    }

    void ConnectWire(int left)
    {
        wireLinked.Add(left);
        SoundManager.Play(Sfx.SmallOk);
        if (wireFancy) StartCoroutine(WireCableFx(left, practical.links[left], true));
        else DrawLine(left, practical.links[left], Theme.Green);
        RefreshWire();
        if (wireLinked.Count >= practical.left.Length) Finish(true);
    }

    IEnumerator TempLine(int left, int right)
    {
        var line = DrawLine(left, right, Theme.Red);
        yield return new WaitForSeconds(Theme.FlashTime);
        if (line != null) Destroy(line.gameObject);
    }

    /// <summary>Linha reta da borda direita do borne da esquerda até a borda esquerda do borne da direita.</summary>
    Image DrawLine(int left, int right, Color color)
    {
        Vector2 a = WirePoint(wireLeft[left].GetComponent<RectTransform>(), true);
        Vector2 b = WirePoint(wireRight[right].GetComponent<RectTransform>(), false);
        Vector2 d = b - a;

        var rt = UIFactory.Rect(wireLines, "Fio", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        rt.sizeDelta = new Vector2(d.magnitude, Theme.WireThickness);
        rt.anchoredPosition = (a + b) / 2;
        rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Vector2 WirePoint(RectTransform rt, bool rightEdge)
    {
        var r = rt.rect;
        var world = rt.TransformPoint(new Vector3(rightEdge ? r.xMax : r.xMin, r.center.y, 0));
        return wireLines.InverseTransformPoint(world);
    }

    /// <summary>Dica Técnica no wire: no passo do EPI desativa 2 EPIs errados; depois liga 1 fio certo.</summary>
    void WireHint(CardData c)
    {
        if (practicalDone) return;
        if (!wireEpiDone)
        {
            var wrong = new List<int>();
            for (int i = 0; i < c.epiItems.Length; i++)
                if (System.Array.IndexOf(c.epiCorrect, i) < 0 && !wireEpiDisabled.Contains(i)) wrong.Add(i);
            CardDeck.Shuffle(wrong);
            for (int k = 0; k < Mathf.Min(2, wrong.Count); k++)
            {
                wireEpiDisabled.Add(wrong[k]);
                wireEpiSelected.Remove(wrong[k]);
            }
            RefreshWire();
            return;
        }

        for (int i = 0; i < c.left.Length; i++)
        {
            if (wireLinked.Contains(i)) continue;
            if (wireSelectedLeft == i) wireSelectedLeft = -1;
            ConnectWire(i);
            return;
        }
    }

    static string WireAnswerText(CardData c)
    {
        var epis = new List<string>();
        foreach (int i in c.epiCorrect) epis.Add(c.epiItems[i]);
        var wires = new List<string>();
        for (int i = 0; i < c.left.Length; i++) wires.Add(c.left[i] + " no " + c.right[c.links[i]]);
        return "EPIs corretos: " + string.Join(", ", epis.ToArray()) + "\nFios: " + string.Join("; ", wires.ToArray());
    }

    // ------------------------------------------------------------ lockout: Bloqueio e etiquetagem

    int LockSteps { get { return practical.steps.Length; } }

    string LockLabel(int opt)
    {
        return opt < LockSteps ? practical.steps[opt] : practical.traps[opt - LockSteps].label;
    }

    void ShowLockout(CardData c)
    {
        StartPractical(c, "Desafio prático: Bloqueio e etiquetagem",
            "Clique os passos na ordem certa. Cuidado com as armadilhas.");
        lockBtns.Clear();
        lockOrder.Clear();
        lockNext = 0;
        lockHinted = -1;

        int traps = c.traps == null ? 0 : c.traps.Length;
        for (int i = 0; i < c.steps.Length + traps; i++) lockOrder.Add(i);
        CardDeck.Shuffle(lockOrder);

        // Com os campos visuais (scene, stepIcons, stepFx, icon nas traps) a cena é desenhada; sem eles, botões simples
        lockFancy = LockIsFancy(c);
        if (lockFancy)
        {
            BuildLockScene(c);
            BuildLockCards();
            RefreshLockout();
            PracticalBar();
            return;
        }

        // A máquina: um bloco grande que mostra quantos passos já foram feitos
        var machine = new GameObject("Maquina", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        machine.transform.SetParent(content, false);
        UIFactory.Round(machine.GetComponent<Image>()).color = Theme.SurfaceHigh;
        machine.GetComponent<LayoutElement>().preferredHeight = Theme.MachineHeight;
        lockMachine = UIFactory.Label(machine.transform, "", Theme.FontSubheading, FontStyle.Bold);
        var mrt = lockMachine.rectTransform;
        mrt.anchorMin = Vector2.zero;
        mrt.anchorMax = Vector2.one;
        mrt.offsetMin = Vector2.zero;
        mrt.offsetMax = Vector2.zero;

        var grid = MakeGrid(Theme.EpiCellSize, Theme.EpiSpacing, 3, lockOrder.Count);
        for (int k = 0; k < lockOrder.Count; k++)
        {
            int pos = k;
            lockBtns.Add(UIFactory.MakeButton(grid, LockLabel(lockOrder[k]), () => OnLock(pos), Theme.Secondary,
                Theme.OptionHeight, -1, Theme.FontBody));
        }

        RefreshLockout();
        PracticalBar();
    }

    void RefreshLockout()
    {
        for (int k = 0; k < lockBtns.Count; k++)
        {
            int opt = lockOrder[k];
            bool done = opt < LockSteps && opt < lockNext;
            var b = lockBtns[k];
            b.GetComponentInChildren<Text>().text = done ? (opt + 1) + ".  " + LockLabel(opt) : LockLabel(opt);
            UIFactory.SetStyle(b, done ? Theme.Correct : opt == lockHinted ? Theme.Primary : Theme.Secondary);
            UIFactory.SetInteractable(b, !done && !practicalDone);
        }
        lockMachine.text = lockNext >= LockSteps
            ? "MÁQUINA BLOQUEADA E ETIQUETADA"
            : "MÁQUINA  ·  passos feitos: " + lockNext + " de " + LockSteps;
        lockMachine.color = lockNext >= LockSteps ? Theme.Green : Theme.Text;
    }

    void OnLock(int pos)
    {
        if (practicalDone) return;
        int opt = lockOrder[pos];
        if (opt == lockNext)
        {
            lockNext++;
            SoundManager.Play(Sfx.SmallOk);
            if (lockHinted == opt) lockHinted = -1;
            RefreshLockout();
            if (lockFancy)
            {
                LockStepFx(opt);
                StartCoroutine(GlowFx(lockBtns[pos].GetComponent<RectTransform>(), false, Theme.Green));
            }
            if (lockNext >= LockSteps) Finish(true);
            return;
        }

        string why = opt < LockSteps ? "Fora de ordem: ainda falta um passo antes deste." : practical.traps[opt - LockSteps].why;
        Mistake(why);
        StartCoroutine(Flash(lockBtns[pos], RefreshLockout));
        if (lockFancy) LockWrongFx(pos);
    }

    /// <summary>Dica Técnica no lockout: destaca em amarelo o próximo passo certo.</summary>
    void LockoutHint()
    {
        if (practicalDone || lockNext >= LockSteps) return;
        lockHinted = lockNext;
        RefreshLockout();
        int k = lockOrder.IndexOf(lockNext);
        if (lockFancy && k >= 0) StartCoroutine(PulseFx(lockBtns[k].GetComponent<RectTransform>(), 2, Theme.HintBlinkTime));
    }

    // ------------------------------------------------------------ hunt: Caça aos riscos

    readonly List<Button> huntBtns = new List<Button>();
    readonly HashSet<int> huntFound = new HashSet<int>();   // riscos achados
    readonly HashSet<int> huntChecked = new HashSet<int>(); // itens seguros já clicados (não contam erro de novo)
    int huntRisks;
    int huntLit = -1;          // risco aceso pela Dica Técnica neste instante
    float huntTimeLeft;

    void ShowHunt(CardData c)
    {
        huntBtns.Clear();
        huntFound.Clear();
        huntChecked.Clear();
        huntLit = -1;
        huntRisks = 0;
        foreach (var item in c.huntItems) if (item.risk) huntRisks++;
        huntTimeLeft = c.seconds;

        StartPractical(c, "Desafio prático: Caça aos riscos", "");
        practicalHint = HuntHintText();

        // Cena centralizada; posições e tamanhos dos objetos vêm em % da cena
        var holder = new GameObject("Cena", typeof(RectTransform), typeof(LayoutElement));
        holder.transform.SetParent(content, false);
        holder.GetComponent<LayoutElement>().preferredHeight = Theme.HuntAreaHeight;
        var scene = UIFactory.Rect(holder.transform, "Area", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        scene.sizeDelta = new Vector2(Theme.HuntAreaWidth, Theme.HuntAreaHeight);
        UIFactory.Round(scene.gameObject.AddComponent<Image>()).color = Theme.Surface;

        // Com os campos visuais (scene e icon nos itens) o fundo é desenhado e os objetos viram cartões com ícone
        huntScene = scene;
        huntFancy = HuntIsFancy(c);
        huntFades.Clear();
        huntPos.Clear();
        huntSize.Clear();
        huntRings.Clear();
        if (huntFancy) DrawHuntBackground(scene, c.scene);

        for (int i = 0; i < c.huntItems.Length; i++)
        {
            var item = c.huntItems[i];
            int idx = i;
            var btn = UIFactory.MakeButton(scene, item.label, () => OnHunt(idx), Theme.HuntItem,
                Theme.ButtonHeight, -1, Theme.FontSmall);
            var text = btn.GetComponentInChildren<Text>().rectTransform;
            text.offsetMin = new Vector2(Theme.HuntItemPadding, Theme.ButtonTextPaddingY);
            text.offsetMax = new Vector2(-Theme.HuntItemPadding, -Theme.ButtonTextPaddingY);

            float w = Mathf.Max(Theme.HuntMinItemWidth, item.w / 100f * Theme.HuntAreaWidth);
            float h = Mathf.Max(Theme.HuntMinItemHeight, item.h / 100f * Theme.HuntAreaHeight);
            if (HuntHasIcon(i))
            {
                var size = HuntIconCard(btn, item, new Vector2(w, h));
                w = size.x;
                h = size.y;
            }
            else if ((item.shape ?? "").ToLowerInvariant() == "circle")
            {
                w = h = Mathf.Max(Mathf.Min(w, h), Theme.HuntMinItemWidth);
                UIFactory.MakeCircle(btn);
            }

            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(item.x / 100f, 1 - item.y / 100f);
            rt.sizeDelta = new Vector2(w, h);
            huntBtns.Add(btn);
            if (huntFancy) HuntRegister(btn, item, new Vector2(w, h));
        }

        RefreshHunt();
        PracticalBar();
        if (c.seconds > 0) StartCoroutine(HuntTimer());
    }

    string HuntHintText()
    {
        string text = "Ache " + huntRisks + (huntRisks == 1 ? " risco: " : " riscos: ") + huntFound.Count + "/" + huntRisks;
        if (practical != null && practical.seconds > 0)
            text += "  ·  " + Mathf.CeilToInt(Mathf.Max(0, huntTimeLeft)) + " s";
        return text;
    }

    void UpdateHuntHint()
    {
        practicalHint = HuntHintText();
        ShowPracticalHint();
    }

    void RefreshHunt()
    {
        for (int i = 0; i < huntBtns.Count; i++)
        {
            bool found = huntFound.Contains(i);
            var style = found ? Theme.HuntFound : i == huntLit ? Theme.Primary : HuntBaseStyle(i);
            UIFactory.SetStyle(huntBtns[i], style);
            UIFactory.SetInteractable(huntBtns[i], !practicalDone && !found && !huntChecked.Contains(i));
        }
    }

    void OnHunt(int i)
    {
        if (practicalDone || huntFound.Contains(i) || huntChecked.Contains(i)) return;
        var item = practical.huntItems[i];
        if (item.risk)
        {
            huntFound.Add(i);
            SoundManager.Play(Sfx.SmallOk);
            if (huntLit == i) huntLit = -1;
            UpdateStatus(item.why, false);
            UpdateHuntHint();
            RefreshHunt();
            if (huntFancy) HuntFoundFx(i);
            if (huntFound.Count >= huntRisks) Finish(true);
            return;
        }

        // Item seguro: borda amarela por um instante e depois fica apagado (já conferido)
        huntChecked.Add(i);
        RefreshHunt();
        UIFactory.SetStyle(huntBtns[i], Theme.HuntSafe);
        StartCoroutine(AfterDelay(Theme.FlashTime, RefreshHunt));
        if (huntFancy) HuntSafeFx(i);
        Mistake(item.why);
    }

    IEnumerator HuntTimer()
    {
        while (huntTimeLeft > 0 && !practicalDone)
        {
            huntTimeLeft -= Time.deltaTime;
            UpdateHuntHint();
            yield return null;
        }
        if (!practicalDone)
        {
            UpdateStatus("O tempo acabou.");
            Finish(false);
        }
    }

    /// <summary>Dica Técnica na caça: um risco ainda não achado pisca em amarelo.</summary>
    void HuntHint()
    {
        if (practicalDone) return;
        var pending = new List<int>();
        for (int i = 0; i < practical.huntItems.Length; i++)
            if (practical.huntItems[i].risk && !huntFound.Contains(i)) pending.Add(i);
        if (pending.Count == 0) return;
        int pick = pending[Random.Range(0, pending.Count)];
        StartCoroutine(Blink(pick));
        if (huntFancy) StartCoroutine(PulseFx(huntBtns[pick].GetComponent<RectTransform>(), 2, Theme.HintBlinkTime));
    }

    IEnumerator Blink(int i)
    {
        for (float t = 0; t < Theme.HintBlinkTime; t += Theme.BlinkInterval)
        {
            if (practicalDone || huntFound.Contains(i)) break;
            huntLit = huntLit == i ? -1 : i;
            RefreshHunt();
            yield return new WaitForSeconds(Theme.BlinkInterval);
        }
        huntLit = -1;
        RefreshHunt();
    }

    static string HuntAnswerText(CardData c)
    {
        var risks = new List<string>();
        foreach (var item in c.huntItems) if (item.risk) risks.Add(item.label);
        return "Riscos: " + string.Join(", ", risks.ToArray());
    }

    // ------------------------------------------------------------ match: Situação e ação certa

    readonly List<Button> matchLeft = new List<Button>();
    readonly List<Button> matchRight = new List<Button>();
    readonly List<int> matchSitOrder = new List<int>(); // botão da esquerda k -> índice em matchPairs
    readonly List<int> matchActOrder = new List<int>(); // botão da direita k -> índice em matchPairs, ou -1-e para extras[e]
    readonly HashSet<int> matchDone = new HashSet<int>();
    int matchSelected = -1; // botão da esquerda escolhido

    void ShowMatch(CardData c)
    {
        StartPractical(c, "Desafio prático: Situação e ação certa", "Clique numa situação e depois na ação certa.");
        matchLeft.Clear();
        matchRight.Clear();
        matchSitOrder.Clear();
        matchActOrder.Clear();
        matchDone.Clear();
        matchSelected = -1;

        int n = c.matchPairs.Length;
        int extras = c.extras == null ? 0 : c.extras.Length;
        for (int i = 0; i < n; i++)
        {
            matchSitOrder.Add(i);
            matchActOrder.Add(i);
        }
        for (int e = 0; e < extras; e++) matchActOrder.Add(-1 - e);
        CardDeck.Shuffle(matchSitOrder);
        CardDeck.Shuffle(matchActOrder);

        // Com os campos visuais (sIcon, aIcon, extraIcons) os cartões ganham ícone e linha de ligação
        matchFancy = MatchIsFancy(c);
        if (matchFancy)
        {
            BuildMatchBoard(c);
            RefreshMatch();
            PracticalBar();
            return;
        }

        var area = new GameObject("Colunas", typeof(RectTransform), typeof(LayoutElement));
        area.transform.SetParent(content, false);
        area.GetComponent<LayoutElement>().preferredHeight = Theme.MatchAreaHeight;

        int rows = Mathf.Max(n, matchActOrder.Count);
        for (int k = 0; k < n; k++)
        {
            int pos = k;
            matchLeft.Add(ColumnButton(area.transform, c.matchPairs[matchSitOrder[k]].s, () => OnMatchLeft(pos), true, k, n, rows));
        }
        for (int k = 0; k < matchActOrder.Count; k++)
        {
            int pos = k;
            matchRight.Add(ColumnButton(area.transform, MatchActionText(matchActOrder[k]), () => OnMatchRight(pos),
                false, k, matchActOrder.Count, rows));
        }

        RefreshMatch();
        PracticalBar();
    }

    string MatchActionText(int act)
    {
        return act >= 0 ? practical.matchPairs[act].a : practical.extras[-1 - act];
    }

    void RefreshMatch()
    {
        for (int k = 0; k < matchLeft.Count; k++)
        {
            int p = matchSitOrder[k];
            bool done = matchDone.Contains(p);
            matchLeft[k].GetComponentInChildren<Text>().text = done ? "Certo: " + practical.matchPairs[p].s : practical.matchPairs[p].s;
            UIFactory.SetStyle(matchLeft[k], done ? Theme.Correct : k == matchSelected ? Theme.Primary : Theme.Secondary);
            UIFactory.SetInteractable(matchLeft[k], !done && !practicalDone);
        }
        for (int k = 0; k < matchRight.Count; k++)
        {
            int act = matchActOrder[k];
            bool done = act >= 0 && matchDone.Contains(act);
            UIFactory.SetStyle(matchRight[k], done ? Theme.Correct : Theme.Secondary);
            UIFactory.SetInteractable(matchRight[k], !done && !practicalDone);
        }
        if (matchFancy)
            for (int k = 0; k < matchPins.Count; k++) matchPins[k].enabled = k == matchSelected && !practicalDone;
    }

    void OnMatchLeft(int k)
    {
        if (practicalDone || matchDone.Contains(matchSitOrder[k])) return;
        matchSelected = matchSelected == k ? -1 : k;
        RefreshMatch();
        if (matchFancy && matchSelected == k) StartCoroutine(PinPulse(k));
    }

    void OnMatchRight(int k)
    {
        int act = matchActOrder[k];
        if (practicalDone || (act >= 0 && matchDone.Contains(act))) return;
        if (matchSelected < 0)
        {
            SetHint("Escolha primeiro uma situação.", Theme.Text);
            return;
        }

        int leftK = matchSelected;
        int p = matchSitOrder[leftK];
        matchSelected = -1;
        if (act == p)
        {
            MatchConnect(p, false);
            return;
        }

        RefreshMatch();
        UIFactory.SetStyle(matchLeft[leftK], Theme.Wrong);
        UIFactory.SetStyle(matchRight[k], Theme.Wrong);
        StartCoroutine(AfterDelay(Theme.FlashTime, RefreshMatch));
        if (matchFancy) StartCoroutine(MatchWrongFx(leftK, k));
        Mistake(practical.matchPairs[p].why);
    }

    void MatchConnect(int p, bool byHint)
    {
        matchDone.Add(p);
        SoundManager.Play(Sfx.SmallOk);
        RefreshMatch();
        if (matchFancy) StartCoroutine(MatchLineFx(matchSitOrder.IndexOf(p), matchActOrder.IndexOf(p), byHint));
        if (matchDone.Count >= practical.matchPairs.Length) Finish(true);
    }

    /// <summary>Dica Técnica no match: liga sozinha 1 par certo.</summary>
    void MatchHint()
    {
        if (practicalDone) return;
        foreach (int p in matchSitOrder)
        {
            if (matchDone.Contains(p)) continue;
            if (matchSelected >= 0 && matchSitOrder[matchSelected] == p) matchSelected = -1;
            MatchConnect(p, true);
            return;
        }
    }

    static string MatchAnswerText(CardData c)
    {
        var lines = new List<string>();
        foreach (var m in c.matchPairs) lines.Add(m.s + ": " + m.a);
        return "Respostas certas:\n" + string.Join("\n", lines.ToArray());
    }
}

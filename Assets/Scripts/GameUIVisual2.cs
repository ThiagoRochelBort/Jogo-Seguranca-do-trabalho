using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cenas desenhadas dos desafios práticos hunt e lockout ("Desafios práticos — visual" do GDD).
/// Só aparecem quando a carta tem os campos visuais (scene, icon, stepIcons, stepFx); sem eles fica o
/// visual simples de GameUIPractical. Regras, dano, timer e habilidades são os mesmos.
/// </summary>
public partial class GameUI
{
    // ------------------------------------------------------------ Ajudantes de animação

    /// <summary>"Pop": escala 0 -> 1,2 -> 1.</summary>
    IEnumerator PopFx(RectTransform rt)
    {
        for (float t = 0; t < Theme.GlowTime; t += Time.deltaTime)
        {
            if (rt == null) yield break;
            float k = t / Theme.GlowTime;
            rt.localScale = Vector3.one * (k < 0.7f ? Mathf.Lerp(0, 1.2f, k / 0.7f) : Mathf.Lerp(1.2f, 1, (k - 0.7f) / 0.3f));
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
    }

    IEnumerator MoveFx(RectTransform rt, Vector2 from, Vector2 to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (rt == null) yield break;
            rt.anchoredPosition = Vector2.Lerp(from, to, t / time);
            yield return null;
        }
        if (rt != null) rt.anchoredPosition = to;
    }

    // ------------------------------------------------------------ hunt desenhado

    bool huntFancy;
    RectTransform huntScene;
    readonly List<CanvasGroup> huntFades = new List<CanvasGroup>();
    readonly List<Vector2> huntPos = new List<Vector2>();
    readonly List<Vector2> huntSize = new List<Vector2>();
    readonly Dictionary<int, Image> huntRings = new Dictionary<int, Image>();

    static bool HuntIsFancy(CardData c)
    {
        if (!string.IsNullOrEmpty(c.scene)) return true;
        foreach (var item in c.huntItems) if (!string.IsNullOrEmpty(item.icon)) return true;
        return false;
    }

    bool HuntHasIcon(int i)
    {
        return huntFancy && Icons.Has(practical.huntItems[i].icon);
    }

    ButtonStyle HuntBaseStyle(int i)
    {
        return HuntHasIcon(i) ? Theme.HuntCard : Theme.HuntItem;
    }

    /// <summary>Fundo da cena montado de formas (oficina, telhado ou obra). Nome desconhecido = fundo liso.</summary>
    void DrawHuntBackground(RectTransform scene, string kind)
    {
        float w = Theme.HuntAreaWidth;
        float h = Theme.HuntAreaHeight;
        var bg = Piece(scene, "Fundo", Vector2.zero, new Vector2(w, h));
        bg.gameObject.AddComponent<RectMask2D>();
        var p = new Icons.Pen(bg, false);
        float floorH = h * 0.22f;
        float floorY = -h / 2 + floorH / 2;

        switch ((kind ?? "").Trim().ToLowerInvariant())
        {
            case "oficina":
                foreach (float x in new[] { -w * 0.29f, w * 0.29f })
                {
                    AddOutline(p.Rect(x, h * 0.22f, 240, 130, Theme.Background), Theme.Text);
                    p.Rect(x, h * 0.22f, 4, 130, Theme.Text);
                    p.Rect(x, h * 0.22f, 240, 4, Theme.Text);
                }
                p.Rect(0, floorY, w, floorH, Theme.SurfaceHigh);
                StripeBand(bg, floorY + floorH / 2, w);
                break;

            case "telhado":
                p.Rect(0, 0, w, h, Theme.Background);
                p.Rect(0, -h * 0.04f, w, 3, Theme.TextMuted);
                p.Rect(w * 0.32f, h * 0.08f, 70, 140, Theme.Surface);
                p.Rect(0, -h * 0.34f, w * 1.2f, h * 0.52f, Theme.SurfaceHigh, 12);
                break;

            case "obra":
                p.Rect(0, 0, w, h, Theme.Background);
                p.Rect(0, h / 2 - 12, w, 24, Theme.Surface);
                foreach (float x in new[] { -w * 0.32f, 0f, w * 0.32f }) p.Rect(x, h * 0.05f, 40, h * 0.7f, Theme.Surface);
                for (int k = 0; k < 4; k++)
                {
                    float x = w * 0.38f + k * 40;
                    p.Line(x, floorY + floorH / 2, x, h * 0.35f, 4, Theme.TextMuted);
                    p.Line(w * 0.38f, -h * 0.1f + k * 60, w * 0.38f + 120, -h * 0.1f + k * 60, 4, Theme.TextMuted);
                }
                p.Rect(0, floorY, w, floorH, Theme.SurfaceHigh);
                StripeBand(bg, floorY + floorH / 2, w);
                break;
        }
    }

    /// <summary>Faixa zebrada amarela e preta (borda do piso).</summary>
    static void StripeBand(Transform parent, float y, float width)
    {
        var band = Piece(parent, "Zebrado", new Vector2(0, y), new Vector2(width, Theme.HuntStripeHeight));
        band.gameObject.AddComponent<RectMask2D>();
        PieceImage(band, "Amarelo", Vector2.zero, new Vector2(width, Theme.HuntStripeHeight), Theme.Yellow, null, false);
        for (float x = -width / 2; x < width / 2; x += Theme.HuntStripeEvery)
        {
            var s = PieceImage(band, "Preto", new Vector2(x, 0), new Vector2(Theme.HuntStripeEvery / 2, Theme.HuntStripeHeight * 3),
                Theme.Background, null, false);
            s.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        }
    }

    /// <summary>Transforma o botão do objeto em cartão com ícone em cima e nome embaixo. Devolve o tamanho final.</summary>
    Vector2 HuntIconCard(Button btn, HuntItem item, Vector2 size)
    {
        size = Vector2.Max(size, Theme.HuntIconCard);
        var icon = Icons.Make(btn.transform, item.icon, Theme.HuntIconSize);
        if (icon != null) icon.anchoredPosition = new Vector2(0, size.y / 2 - Theme.HuntItemPadding - Theme.HuntIconSize / 2);
        var text = btn.GetComponentInChildren<Text>();
        text.fontSize = Theme.FontTag;
        text.resizeTextMaxSize = Theme.FontTag;
        text.rectTransform.offsetMin = new Vector2(Theme.HuntItemPadding, Theme.ButtonTextPaddingY);
        text.rectTransform.offsetMax = new Vector2(-Theme.HuntItemPadding, -(Theme.HuntItemPadding + Theme.HuntIconSize));
        return size;
    }

    /// <summary>Guarda posição e tamanho do objeto (centro da cena = 0,0) e um CanvasGroup para esmaecer.</summary>
    void HuntRegister(Button btn, HuntItem item, Vector2 size)
    {
        huntPos.Add(new Vector2((item.x / 100f - 0.5f) * Theme.HuntAreaWidth, (0.5f - item.y / 100f) * Theme.HuntAreaHeight));
        huntSize.Add(size);
        huntFades.Add(btn.gameObject.AddComponent<CanvasGroup>());
    }

    /// <summary>Risco achado: anel vermelho cresce em volta e um alerta surge acima.</summary>
    void HuntFoundFx(int i)
    {
        var size = huntSize[i];
        float d = Mathf.Max(size.x, size.y) + Theme.HuntRingPad;
        var ring = PieceImage(huntScene, "Anel", huntPos[i], new Vector2(d, d), Theme.Red, Theme.IconRing, false);
        huntRings[i] = ring;
        StartCoroutine(PopFx(ring.rectTransform));

        var alert = Icons.Make(huntScene, "alerta", Theme.HuntAlertSize);
        if (alert == null) return;
        alert.anchoredPosition = huntPos[i] + new Vector2(0, size.y / 2 + Theme.HuntAlertSize / 2);
        alert.localScale = Vector3.zero;
        StartCoroutine(PopIcon(alert, Theme.HuntAlertSize));
    }

    /// <summary>Pop de um ícone que já tem escala própria (tamanho / 96).</summary>
    IEnumerator PopIcon(RectTransform icon, float size)
    {
        float baseScale = size / Theme.IconBase;
        for (float t = 0; t < Theme.GlowTime; t += Time.deltaTime)
        {
            if (icon == null) yield break;
            float k = t / Theme.GlowTime;
            float s = k < 0.7f ? Mathf.Lerp(0, 1.2f, k / 0.7f) : Mathf.Lerp(1.2f, 1, (k - 0.7f) / 0.3f);
            icon.localScale = Vector3.one * baseScale * s;
            yield return null;
        }
        if (icon != null) icon.localScale = Vector3.one * baseScale;
    }

    /// <summary>Item seguro: treme e, depois do aviso amarelo, fica esmaecido.</summary>
    void HuntSafeFx(int i)
    {
        StartCoroutine(ShakeFx(huntBtns[i].GetComponent<RectTransform>()));
        var fade = huntFades[i];
        StartCoroutine(AfterDelay(Theme.FlashTime, () => { if (fade != null) fade.alpha = Theme.HuntFadedAlpha; }));
    }

    /// <summary>Fim: ganhou = aros verdes; perdeu = os riscos que faltaram ganham anel vermelho pulsando.</summary>
    void HuntFinishFx(bool success)
    {
        if (success)
        {
            foreach (var ring in huntRings.Values) if (ring != null) ring.color = Theme.Green;
            return;
        }
        for (int i = 0; i < practical.huntItems.Length; i++)
        {
            if (!practical.huntItems[i].risk || huntFound.Contains(i)) continue;
            var size = huntSize[i];
            float d = Mathf.Max(size.x, size.y) + Theme.HuntRingPad;
            var ring = PieceImage(huntScene, "Faltou", huntPos[i], new Vector2(d, d), Theme.Red, Theme.IconRing, false);
            StartCoroutine(PulseFx(ring.rectTransform, 2, Theme.ResultPause));
        }
    }

    // ------------------------------------------------------------ lockout desenhado

    bool lockFancy;
    RectTransform lockPanel, lockFxRoot, lockLever, lockPadlock, lockPerson;
    Image lockMachineLight, lockBreakerLight;
    Vector2 lockMainPos, lockBreakerPos, lockAnchorPos;
    bool lockHasBreaker, lockHasAnchor;
    int lockSlot;

    // Lugares livres na ilustração (560 x 260, centro 0,0) para os objetos que os passos fazem aparecer
    static readonly Vector2[] LockSlots =
    {
        new Vector2(210, 70), new Vector2(210, -60), new Vector2(-30, -85), new Vector2(-240, -85), new Vector2(-30, 85),
    };

    static bool LockIsFancy(CardData c)
    {
        if (!string.IsNullOrEmpty(c.scene) || HasAny(c.stepIcons) || HasAny(c.stepFx)) return true;
        if (c.traps != null) foreach (var t in c.traps) if (!string.IsNullOrEmpty(t.icon)) return true;
        return false;
    }

    string LockIcon(int opt)
    {
        return opt < LockSteps ? Icons.At(practical.stepIcons, opt) : practical.traps[opt - LockSteps].icon;
    }

    /// <summary>Ilustração 560 x 260 com o objeto principal da cena (máquina se a cena não for conhecida).</summary>
    void BuildLockScene(CardData c)
    {
        lockLever = lockPadlock = lockPerson = null;
        lockHasBreaker = lockHasAnchor = false;
        lockSlot = 0;

        var holder = new GameObject("Ilustracao", typeof(RectTransform), typeof(LayoutElement));
        holder.transform.SetParent(content, false);
        holder.GetComponent<LayoutElement>().preferredHeight = Theme.LockSceneSize.y;
        var panelImg = PieceImage(holder.transform, "Cena", Vector2.zero, Theme.LockSceneSize, Theme.Surface, Theme.Rounded, true);
        lockPanel = panelImg.rectTransform;

        float half = Theme.LockMainSize / 2;
        string kind = (c.scene ?? "").Trim().ToLowerInvariant();
        switch (kind)
        {
            case "painel":
                lockMainPos = new Vector2(-130, 10);
                Icons.Make(lockPanel, "painel", Theme.LockMainSize).anchoredPosition = lockMainPos;
                BuildBreaker(new Vector2(110, 10));
                break;
            case "altura":
                lockMainPos = new Vector2(-150, 0);
                Icons.Make(lockPanel, "escada", Theme.LockMainSize).anchoredPosition = lockMainPos;
                lockPerson = Icons.Make(lockPanel, "pessoa", Theme.LockPersonSize * 0.8f);
                lockPerson.anchoredPosition = lockMainPos + new Vector2(0, half * 0.6f);
                lockAnchorPos = new Vector2(40, 95);
                lockHasAnchor = true;
                PieceImage(lockPanel, "Ancoragem", lockAnchorPos, Vector2.one * Theme.LockLight, Theme.Yellow, Theme.Circle, false);
                break;
            case "tanque":
                lockMainPos = new Vector2(-110, 0);
                Icons.Make(lockPanel, "tanque", Theme.LockMainSize).anchoredPosition = lockMainPos;
                lockPerson = Icons.Make(lockPanel, "pessoa", Theme.LockPersonSize);
                lockPerson.anchoredPosition = new Vector2(80, -40);
                break;
            default: // maquina
                lockMainPos = new Vector2(-130, 10);
                Icons.Make(lockPanel, "maquina", Theme.LockMainSize).anchoredPosition = lockMainPos;
                BuildBreaker(new Vector2(110, 10));
                break;
        }

        // Luz de estado do equipamento (verde = ligado; vermelho = parado)
        lockMachineLight = PieceImage(lockPanel, "Luz", lockMainPos + new Vector2(half * 0.75f, half * 0.75f),
            Vector2.one * Theme.LockLight, Theme.Green, Theme.Circle, false);
        AddOutline(lockMachineLight, Theme.Text);

        lockFxRoot = UIFactory.Rect(lockPanel, "Efeitos", Vector2.zero, Vector2.one);

        lockMachine = SceneLabel(lockPanel, "", new Vector2(0, -Theme.LockSceneSize.y / 2 + 18),
            new Vector2(Theme.LockSceneSize.x - 2 * Theme.WireCardGap, 30), Theme.FontTag, Theme.Text, TextAnchor.MiddleCenter);
    }

    /// <summary>Disjuntor desenhado em peças (a alavanca e a luz mudam com os passos).</summary>
    void BuildBreaker(Vector2 pos)
    {
        lockHasBreaker = true;
        lockBreakerPos = pos;
        var box = PieceImage(lockPanel, "Disjuntor", pos, Theme.LockBreakerSize, Theme.SurfaceHigh, Theme.Rounded, true);
        AddOutline(box, Theme.Text);
        lockLever = PieceImage(lockPanel, "Alavanca", pos + new Vector2(0, Theme.LockLeverTravel), Theme.LockLeverSize,
            Theme.Text, null, false).rectTransform;
        lockBreakerLight = PieceImage(lockPanel, "LuzDisjuntor", pos + new Vector2(0, -Theme.LockBreakerSize.y / 2 + 16),
            Vector2.one * (Theme.LockLight * 0.6f), Theme.Green, Theme.Circle, false);
    }

    /// <summary>Cartões de passo com ícone em cima e nome embaixo, em linha (até 7 por linha).</summary>
    void BuildLockCards()
    {
        var size = Theme.LockCardSize;
        float gap = Theme.WireCardGap;
        int count = lockOrder.Count;
        int perRow = Mathf.Min(count, Theme.LockCardsPerRow);
        int rows = Mathf.CeilToInt(count / (float)perRow);

        var holder = new GameObject("Passos", typeof(RectTransform), typeof(LayoutElement));
        holder.transform.SetParent(content, false);
        float height = rows * size.y + (rows - 1) * gap;
        holder.GetComponent<LayoutElement>().preferredHeight = height;
        var area = Piece(holder.transform, "Area", Vector2.zero, new Vector2(perRow * size.x + (perRow - 1) * gap, height));

        for (int k = 0; k < count; k++)
        {
            int pos = k;
            int row = k / perRow;
            int col = k % perRow;
            int inRow = Mathf.Min(perRow, count - row * perRow);
            float rowWidth = inRow * size.x + (inRow - 1) * gap;
            var center = new Vector2(-rowWidth / 2 + size.x / 2 + col * (size.x + gap), height / 2 - size.y / 2 - row * (size.y + gap));
            int opt = lockOrder[k];
            var btn = SceneButton(area, LockLabel(opt), () => OnLock(pos), Theme.Secondary, center, size, Theme.FontTag);

            var text = btn.GetComponentInChildren<Text>().rectTransform;
            var icon = Icons.Make(btn.transform, LockIcon(opt), Theme.LockCardIcon);
            text.offsetMin = new Vector2(gap, Theme.ButtonTextPaddingY);
            if (icon != null)
            {
                icon.anchoredPosition = new Vector2(0, size.y / 2 - gap - Theme.LockCardIcon / 2);
                text.offsetMax = new Vector2(-gap, -(gap + Theme.LockCardIcon));
            }
            else
            {
                text.offsetMax = new Vector2(-gap, -Theme.ButtonTextPaddingY);
            }
            lockBtns.Add(btn);
        }
    }

    Vector2 NextLockSlot()
    {
        var p = LockSlots[lockSlot % LockSlots.Length];
        lockSlot++;
        return p;
    }

    RectTransform PopLockIcon(string name, Vector2 pos, float size)
    {
        var icon = Icons.Make(lockFxRoot, name, size);
        if (icon == null) return null;
        icon.anchoredPosition = pos;
        icon.localScale = Vector3.zero;
        StartCoroutine(PopIcon(icon, size));
        return icon;
    }

    /// <summary>Selo verde de "feito" (círculo com texto).</summary>
    RectTransform Check(Vector2 pos, float size, string text)
    {
        var disc = PieceImage(lockFxRoot, "Check", pos, Vector2.one * size, Theme.Green, Theme.Circle, false);
        AddOutline(disc, Theme.Background);
        SceneLabel(disc.transform, text, Vector2.zero, Vector2.one * size, Theme.FontTag, Theme.OnYellow, TextAnchor.MiddleCenter);
        disc.rectTransform.localScale = Vector3.zero;
        StartCoroutine(PopFx(disc.rectTransform));
        return disc.rectTransform;
    }

    Vector2 LeverPos
    {
        get { return lockHasBreaker ? lockBreakerPos : lockMainPos; }
    }

    /// <summary>Reação da cena a um passo certo (stepFx). Valor desconhecido = só o cartão muda.</summary>
    void LockStepFx(int step)
    {
        string fx = (Icons.At(practical.stepFx, step) ?? "").Trim().ToLowerInvariant();
        switch (fx)
        {
            case "avisar":
            {
                Vector2 from = (lockPerson != null ? lockPerson.anchoredPosition : lockMainPos) + new Vector2(0, 40);
                var balloon = Piece(lockFxRoot, "Balao", from, new Vector2(90, 60));
                UIFactory.Round(balloon.gameObject.AddComponent<Image>()).color = Theme.CardFace;
                Icons.Make(balloon, "alerta", 40);
                StartCoroutine(MoveFx(balloon, from, from + new Vector2(0, 40), Theme.StepFxTime));
                break;
            }
            case "desligar":
                if (lockLever != null)
                    StartCoroutine(MoveFx(lockLever, lockLever.anchoredPosition, lockBreakerPos - new Vector2(0, Theme.LockLeverTravel), Theme.StepFxTime));
                if (lockBreakerLight != null) lockBreakerLight.color = Theme.Red;
                lockMachineLight.color = Theme.Red;
                break;
            case "bloquear":
            {
                var target = LeverPos + new Vector2(0, -8);
                lockPadlock = Icons.Make(lockFxRoot, "cadeado", 56);
                if (lockPadlock != null) StartCoroutine(MoveFx(lockPadlock, target + new Vector2(0, Theme.LockDropHeight), target, Theme.StepFxTime));
                break;
            }
            case "etiquetar":
            {
                var at = (lockPadlock != null ? (Vector2)lockPadlock.anchoredPosition : LeverPos) + new Vector2(30, -50);
                var tag = Icons.Make(lockFxRoot, "etiqueta", 44);
                if (tag != null)
                {
                    tag.anchoredPosition = at;
                    StartCoroutine(SwingFx(tag));
                }
                break;
            }
            case "medir":
                PopLockIcon("multimetro", NextLockSlot(), Theme.LockFxIcon);
                break;
            case "aterrar":
            {
                Vector2 a = LeverPos + new Vector2(0, -Theme.LockBreakerSize.y / 2);
                Vector2 b = new Vector2(a.x, -Theme.LockSceneSize.y / 2 + 40);
                var path = NewPath(lockFxRoot, new[] { a, b, b + new Vector2(60, 0) }, Theme.CableThickness, Theme.Green, false);
                SetPath(path, 1);
                StripePath(path, Theme.CableThickness, Theme.Yellow);
                StartCoroutine(SparksFx(lockFxRoot, b + new Vector2(60, 0), Theme.Yellow, 4, Theme.SparkSize, Theme.SparkDistance));
                break;
            }
            case "testar":
            {
                var pos = NextLockSlot();
                var button = PieceImage(lockFxRoot, "Partida", pos, Vector2.one * Theme.LockCheck, Theme.Green, Theme.Circle, false);
                AddOutline(button, Theme.Text);
                StartCoroutine(PulseFx(button.rectTransform, 1, Theme.StepFxTime));
                lockMachineLight.color = Theme.Red; // continua parada
                Check(pos + new Vector2(Theme.LockCheck, 0), Theme.LockCheck, "OK");
                break;
            }
            case "documento":
            {
                var pos = NextLockSlot();
                PopLockIcon("documento", pos, Theme.LockFxIcon);
                Check(pos + new Vector2(Theme.LockFxIcon / 2, -Theme.LockFxIcon / 2), Theme.LockCheck, "OK");
                break;
            }
            case "inspecionar":
            {
                var lupa = Icons.Make(lockFxRoot, "lupa", Theme.LockFxIcon);
                if (lupa != null)
                    StartCoroutine(MoveFx(lupa, lockMainPos + new Vector2(-80, 0), lockMainPos + new Vector2(80, 0), Theme.SlideTime));
                Check(lockMainPos + new Vector2(0, -Theme.LockMainSize / 2 + 10), Theme.LockCheck, "OK");
                break;
            }
            case "ancorar":
            {
                Vector2 from = lockPerson != null ? lockPerson.anchoredPosition : lockMainPos;
                Vector2 to = lockHasAnchor ? lockAnchorPos : from + new Vector2(120, 60);
                var path = NewPath(lockFxRoot, new[] { from, to }, Theme.MatchLineThickness, Theme.Yellow, false);
                StartCoroutine(GrowPath(path, 0, 1, Theme.CableTime));
                StartCoroutine(SparksFx(lockFxRoot, to, Theme.Yellow, 4, Theme.SparkSize, Theme.SparkDistance));
                break;
            }
            case "isolar":
            {
                float x = Theme.LockSceneSize.x / 2 - 50;
                float y = -Theme.LockSceneSize.y / 2 + 50;
                var tape = NewPath(lockFxRoot, new[] { new Vector2(-x, y), new Vector2(x, y) }, 6, Theme.Yellow, false);
                StartCoroutine(GrowPath(tape, 0, 1, Theme.StepFxTime));
                PopLockIcon("cone", new Vector2(-x, y), 60);
                PopLockIcon("cone", new Vector2(x, y), 60);
                break;
            }
            case "entrar":
            {
                Vector2 from = new Vector2(Theme.LockSceneSize.x / 2 - 60, -30);
                var worker = Icons.Make(lockFxRoot, "pessoa", Theme.LockPersonSize * 0.8f);
                if (worker != null) StartCoroutine(EnterFx(worker, from, lockMainPos + new Vector2(0, Theme.LockMainSize / 4)));
                break;
            }
        }
    }

    IEnumerator SwingFx(RectTransform rt)
    {
        for (float t = 0; t < Theme.SwingTime; t += Time.deltaTime)
        {
            if (rt == null) yield break;
            float a = Mathf.Sin(t / Theme.SwingTime * 2 * 2 * Mathf.PI) * Theme.LockSwingAngle * (1 - t / Theme.SwingTime);
            rt.localRotation = Quaternion.Euler(0, 0, a);
            yield return null;
        }
        if (rt != null) rt.localRotation = Quaternion.identity;
    }

    /// <summary>A pessoa de trabalho vai até a boca do tanque e some (o vigia fica).</summary>
    IEnumerator EnterFx(RectTransform rt, Vector2 from, Vector2 to)
    {
        var fade = rt.gameObject.AddComponent<CanvasGroup>();
        for (float t = 0; t < Theme.SlideTime; t += Time.deltaTime)
        {
            if (rt == null) yield break;
            float k = t / Theme.SlideTime;
            rt.anchoredPosition = Vector2.Lerp(from, to, k);
            fade.alpha = k < 0.6f ? 1 : 1 - (k - 0.6f) / 0.4f;
            yield return null;
        }
        if (rt != null) fade.alpha = 0;
    }

    /// <summary>Passo errado ou armadilha: a cena treme com brilho vermelho, e o cartão também treme.</summary>
    void LockWrongFx(int pos)
    {
        StartCoroutine(ShakeFx(lockPanel));
        StartCoroutine(GlowFx(lockPanel, false, Theme.Red));
        StartCoroutine(ShakeFx(lockBtns[pos].GetComponent<RectTransform>()));
    }

    /// <summary>Fim certo: luz vermelha (parada segura) e selo verde "Seguro".</summary>
    void LockFinishFx(bool success)
    {
        if (!success) return;
        lockMachineLight.color = Theme.Red;
        Check(new Vector2(Theme.LockSceneSize.x / 2 - Theme.LockBadge / 2 - Theme.WireCardGap, 0), Theme.LockBadge, "Seguro");
    }
}

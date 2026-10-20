using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controla as telas: configuração, mesa da memória, desafios, eventos e fim de jogo.
/// Coloque este componente num GameObject vazio da cena e dê Play.
/// Cores, tamanhos e estilos de botão vêm da classe Theme.
/// </summary>
[RequireComponent(typeof(GameManager))]
public partial class GameUI : MonoBehaviour // desafios práticos ficam em GameUIPractical.cs
{
    GameManager gm;
    RectTransform content;
    ScrollRect scroll;

    // HUD (topo)
    GameObject hud;
    readonly List<Image> healthSegments = new List<Image>();
    RectTransform auditFill;
    Text auditText, hudPlayer, hudRoleRound;

    // Faixa de baixo: habilidade, mensagem e ação principal
    GameObject bottomBar;
    Button abilityBtn, actionBtn;
    Text abilityText, actionText, messageText;

    // Estado temporário do desafio atual
    List<int> seqOrder = new List<int>();
    int lockedPrefix;
    HashSet<int> hiddenOptions = new HashSet<int>();
    HashSet<int> epiSelected = new HashSet<int>();
    HashSet<int> epiLocked = new HashSet<int>();
    List<int> epiOrder = new List<int>(); // ordem de exibição embaralhada

    // Estado da mesa da memória no turno atual
    readonly List<Button> tileBtns = new List<Button>();
    readonly List<Text> tileTexts = new List<Text>();
    readonly List<Text> tileTags = new List<Text>();
    int firstPick = -1;
    bool busy;

    // Prévia de memorização no início da partida
    Text previewLabel;

    // Controles de som (HUD e tela inicial)
    Text hudMuteText, setupMuteText, setupVolumeText, setupMusicText;

    void Start()
    {
        gm = GetComponent<GameManager>();
        if (UIFactory.Font == null)
            Debug.LogWarning("Fonte padrão não encontrada; os textos podem não aparecer.");
        if (SoundManager.Instance == null) gameObject.AddComponent<SoundManager>();
        SoundManager.Changed += RefreshSoundLabels;
        BuildLayout();
        ShowSetup();
    }

    void OnDestroy()
    {
        SoundManager.Changed -= RefreshSoundLabels;
    }

    // ------------------------------------------------------------ Layout base

    void BuildLayout()
    {
        var canvas = UIFactory.CreateCanvas();
        var bg = UIFactory.Panel(canvas.transform, "Fundo", Theme.Background);

        // Mesa centralizada entre o HUD e a faixa de baixo
        var area = UIFactory.Rect(bg, "AreaCentral", new Vector2(Theme.SideMargin, 0), new Vector2(1 - Theme.SideMargin, 1));
        area.offsetMin = new Vector2(0, Theme.BottomHeight + Theme.ContentMarginY);
        area.offsetMax = new Vector2(0, -(Theme.HudHeight + Theme.ContentMarginY));
        content = UIFactory.ScrollArea(area, Vector2.zero, Vector2.one, out scroll);

        BuildHud(bg);
        BuildBottomBar(bg);
    }

    /// <summary>HUD: Índice de Segurança (esquerda), Auditoria (centro) e jogador da vez (direita).</summary>
    void BuildHud(Transform parent)
    {
        var rt = Bar(parent, "HUD", true, Theme.HudHeight);
        hud = rt.gameObject;

        // Índice de Segurança em segmentos
        var left = Zone(rt, TextAnchor.MiddleLeft, Theme.HudSideWidth, 1);
        UIFactory.Label(left, "ÍNDICE DE SEGURANÇA", Theme.FontTag, FontStyle.Bold, TextAnchor.MiddleLeft, Theme.TextMuted);
        var segRow = HRow(left, Theme.HealthSegmentSpacing);
        healthSegments.Clear();
        for (int i = 0; i < gm.maxHealth; i++)
            healthSegments.Add(Block(segRow, "Ponto", Theme.HealthSegmentWidth, Theme.HealthSegmentHeight));

        // Auditoria: barra + número
        var center = Zone(rt, TextAnchor.MiddleCenter, Theme.AuditBlockWidth, 0);
        UIFactory.Label(center, "AUDITORIA", Theme.FontTag, FontStyle.Bold, TextAnchor.MiddleCenter, Theme.TextMuted);
        var auditRow = HRow(center, Theme.ContentSpacing);
        var track = Block(auditRow, "Barra", Theme.AuditBarWidth, Theme.AuditBarHeight);
        track.color = Theme.SurfaceHigh;
        auditFill = UIFactory.Rect(track.transform, "Preenchimento", Vector2.zero, Vector2.one);
        UIFactory.Round(auditFill.gameObject.AddComponent<Image>()).color = Theme.Green;
        auditText = UIFactory.Label(auditRow, "", Theme.FontSubheading, FontStyle.Bold, TextAnchor.MiddleLeft);

        // Jogador da vez + botão de som (mesma largura mínima da esquerda: Auditoria fica no centro)
        var right = HBlock(rt, Theme.HudSideWidth);
        var player = Zone(right, TextAnchor.MiddleRight, 0, 0);
        hudPlayer = UIFactory.Label(player, "", Theme.FontSubheading, FontStyle.Bold, TextAnchor.MiddleRight, Theme.Yellow);
        hudRoleRound = UIFactory.Label(player, "", Theme.FontTag, FontStyle.Normal, TextAnchor.MiddleRight, Theme.TextMuted);
        var mute = UIFactory.MakeButton(right, "", ToggleMute, Theme.Secondary,
            Theme.SoundButtonHeight, Theme.SoundButtonWidth, Theme.FontTag);
        hudMuteText = mute.GetComponentInChildren<Text>();
        RefreshSoundLabels();
    }

    /// <summary>Bloco horizontal do HUD, alinhado à direita.</summary>
    static RectTransform HBlock(Transform parent, float minWidth)
    {
        var go = new GameObject("Bloco", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = Theme.ContentSpacing;
        h.childAlignment = TextAnchor.MiddleRight;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        var le = go.GetComponent<LayoutElement>();
        le.minWidth = minWidth;
        le.flexibleWidth = 1;
        return go.GetComponent<RectTransform>();
    }

    // ------------------------------------------------------------ Som

    void ToggleMute()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.ToggleMute();
    }

    void RefreshSoundLabels()
    {
        var s = SoundManager.Instance;
        if (s == null) return;
        string mute = s.Muted ? "Som: mudo" : "Som: ligado";
        if (hudMuteText != null) hudMuteText.text = mute;
        if (setupMuteText != null) setupMuteText.text = mute;
        if (setupVolumeText != null) setupVolumeText.text = "Volume " + Mathf.RoundToInt(s.Volume * 100) + "%";
        if (setupMusicText != null) setupMusicText.text = s.MusicOn ? "Música: ligada" : "Música: desligada";
    }

    /// <summary>Linha da tela inicial: som, volume - / +, música.</summary>
    void SoundControls()
    {
        var row = UIFactory.Row(content, Theme.ButtonHeight);
        row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

        var mute = UIFactory.MakeButton(row, "", ToggleMute, Theme.Secondary, Theme.ButtonHeight, Theme.SoundToggleWidth);
        setupMuteText = mute.GetComponentInChildren<Text>();

        UIFactory.MakeButton(row, "-", () => { if (SoundManager.Instance != null) SoundManager.Instance.StepVolume(-1); },
            Theme.Secondary, Theme.ButtonHeight, Theme.SoundStepWidth);
        setupVolumeText = UIFactory.Label(row, "", Theme.FontBody, FontStyle.Bold);
        var le = setupVolumeText.GetComponent<LayoutElement>();
        le.preferredWidth = Theme.SoundLabelWidth;
        le.minWidth = Theme.SoundLabelWidth;
        UIFactory.MakeButton(row, "+", () => { if (SoundManager.Instance != null) SoundManager.Instance.StepVolume(1); },
            Theme.Secondary, Theme.ButtonHeight, Theme.SoundStepWidth);

        var music = UIFactory.MakeButton(row, "", () => { if (SoundManager.Instance != null) SoundManager.Instance.ToggleMusic(); },
            Theme.Secondary, Theme.ButtonHeight, Theme.SoundToggleWidth);
        setupMusicText = music.GetComponentInChildren<Text>();
        RefreshSoundLabels();
    }

    /// <summary>Usa a habilidade do jogador da vez (com o som de habilidade).</summary>
    void UseAbility()
    {
        gm.UseAbility();
        SoundManager.Play(Sfx.Ability);
    }

    /// <summary>Faixa de baixo: botão da habilidade, mensagem/dica e botão de ação.</summary>
    void BuildBottomBar(Transform parent)
    {
        var rt = Bar(parent, "FaixaInferior", false, Theme.BottomHeight);
        bottomBar = rt.gameObject;
        var h = rt.GetComponent<HorizontalLayoutGroup>();
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childForceExpandHeight = false;

        abilityBtn = UIFactory.MakeButton(rt, "", null, Theme.Ability, Theme.ButtonHeight, Theme.AbilityButtonWidth);
        abilityText = abilityBtn.GetComponentInChildren<Text>();

        messageText = UIFactory.Label(rt, "", Theme.FontSmall, FontStyle.Normal, TextAnchor.MiddleLeft);
        UIFactory.ShrinkToFit(messageText);
        var le = messageText.GetComponent<LayoutElement>();
        le.preferredWidth = 1;
        le.flexibleWidth = 1;
        le.preferredHeight = Theme.MessageHeight;

        actionBtn = UIFactory.MakeButton(rt, "", null, Theme.Primary, Theme.ButtonHeight, Theme.FooterButtonWidth);
        actionText = actionBtn.GetComponentInChildren<Text>();
    }

    /// <summary>Faixa de largura total colada no topo ou embaixo, com linha amarela do lado da mesa.</summary>
    static RectTransform Bar(Transform parent, string name, bool top, float height)
    {
        float y = top ? 1 : 0;
        var rt = UIFactory.Rect(parent, name, new Vector2(0, y), new Vector2(1, y));
        rt.pivot = new Vector2(0.5f, y);
        rt.sizeDelta = new Vector2(0, height);
        rt.gameObject.AddComponent<Image>().color = Theme.Surface;

        var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
        int pad = (int)Theme.BarPadding;
        h.padding = new RectOffset(pad, pad, Theme.BarPaddingY, Theme.BarPaddingY);
        h.spacing = Theme.ContentSpacing;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;

        UIFactory.Stripe(rt, !top, Theme.StripeHeight, Theme.Yellow);
        return rt;
    }

    /// <summary>Bloco do HUD que empilha textos. flexible 1 nas pontas mantém o bloco do meio centralizado.</summary>
    static RectTransform Zone(Transform parent, TextAnchor align, float minWidth, float flexible)
    {
        var go = new GameObject("Bloco", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var v = go.GetComponent<VerticalLayoutGroup>();
        v.spacing = Theme.HudLineSpacing;
        v.childAlignment = align;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = false;
        v.childForceExpandHeight = false;
        var le = go.GetComponent<LayoutElement>();
        le.minWidth = minWidth;
        le.flexibleWidth = flexible;
        return go.GetComponent<RectTransform>();
    }

    static Transform HRow(Transform parent, float spacing)
    {
        var go = new GameObject("Linha", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        go.transform.SetParent(parent, false);
        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        return go.transform;
    }

    static Image Block(Transform parent, string name, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = height;
        return UIFactory.Round(go.GetComponent<Image>());
    }

    void Clear()
    {
        StopAllCoroutines();
        UIFactory.Clear(content);
        content.anchoredPosition = Vector2.zero; // volta a rolagem para o topo
    }

    void RefreshHud()
    {
        hud.SetActive(true);
        bottomBar.SetActive(true);

        bool low = gm.Health <= Theme.LowHealth;
        for (int i = 0; i < healthSegments.Count; i++)
            healthSegments[i].color = i < gm.Health ? (low ? Theme.Red : Theme.Green) : Theme.SurfaceHigh;

        float frac = gm.AuditGoal > 0 ? Mathf.Clamp01(gm.Audit / (float)gm.AuditGoal) : 0;
        auditFill.anchorMax = new Vector2(frac, 1);
        auditFill.gameObject.SetActive(frac > 0);
        auditText.text = gm.Audit + "/" + gm.AuditGoal;

        var p = gm.Current;
        hudPlayer.text = "Vez de " + p.name;
        hudRoleRound.text = RoleInfo.Name(p.role) + " · Rodada " + gm.Round;
    }

    // ------------------------------------------------------------ Faixa de baixo

    void SetHint(string message, Color color)
    {
        messageText.text = message;
        messageText.color = color;
    }

    void SetAbility(string label, UnityAction action, bool enabled)
    {
        abilityBtn.gameObject.SetActive(true);
        abilityText.text = label;
        abilityBtn.onClick.RemoveAllListeners();
        if (action != null) abilityBtn.onClick.AddListener(action);
        UIFactory.SetInteractable(abilityBtn, enabled);
    }

    void HideAbility()
    {
        abilityBtn.gameObject.SetActive(false);
    }

    void SetAction(string label, UnityAction action)
    {
        actionBtn.gameObject.SetActive(true);
        actionText.text = label;
        actionBtn.onClick.RemoveAllListeners();
        actionBtn.onClick.AddListener(action);
    }

    void HideAction()
    {
        actionBtn.gameObject.SetActive(false);
    }

    /// <summary>Texto do botão da habilidade quando ela não está pronta.</summary>
    string CooldownLabel(Player p)
    {
        string name = RoleInfo.AbilityName(p.role);
        if (p.cooldown > gm.abilityRecharge) return name + " · já usada";
        return name + " · volta em " + p.cooldown + (p.cooldown == 1 ? " turno" : " turnos");
    }

    Text L(string text, int size, FontStyle style = FontStyle.Normal, Color? color = null,
        TextAnchor align = TextAnchor.MiddleCenter)
    {
        return UIFactory.Label(content, text, size, style, align, color);
    }

    Button B(string label, UnityAction action, ButtonStyle style, float height = Theme.ButtonHeight)
    {
        return UIFactory.MakeButton(content, label, action, style, height);
    }

    // ------------------------------------------------------------ Telas

    void ShowSetup()
    {
        Clear();
        hud.SetActive(false);
        bottomBar.SetActive(false);
        SoundManager.Duck(false);
        SoundManager.StartMusic();

        UIFactory.Spacer(content, Theme.SpaceM);
        L("TURNO SEGURO", Theme.FontTitle, FontStyle.Bold, Theme.Yellow);
        L("Um jogo da memória cooperativo sobre as Normas Regulamentadoras", Theme.FontSubheading);
        UIFactory.Spacer(content, Theme.SpaceM);
        L("Encontrem os pares de norma e tema, e de risco e EPI. Cada par encontrado soma na Auditoria. " +
          "Errou o par? A equipe precisa acertar um desafio, senão perde Índice de Segurança. " +
          "Todos ganham ou todos perdem.", Theme.FontBody, FontStyle.Normal, Theme.TextMuted);
        UIFactory.Spacer(content, Theme.SpaceL);
        L("Quantos jogadores?", Theme.FontHeading, FontStyle.Bold);

        // Os 3 botões numa linha só, para sobrar espaço para os controles de som
        var players = UIFactory.Row(content, Theme.ButtonHeight);
        players.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        for (int n = 2; n <= 4; n++)
        {
            int count = n;
            UIFactory.MakeButton(players, n + " jogadores", () => StartGame(count), Theme.Primary,
                Theme.ButtonHeight, Theme.SoundToggleWidth);
        }

        UIFactory.Spacer(content, Theme.SpaceM);
        SoundControls();
    }

    void StartGame(int players)
    {
        if (!gm.Setup(players))
        {
            Clear();
            L("Faltam pares ou desafios em Assets/Resources/" + gm.cardsResource + ".json", Theme.FontSubheading, FontStyle.Bold, Theme.Red);
            B("Voltar", ShowSetup, Theme.Secondary);
            return;
        }
        if (gm.PreviewSeconds > 0) ShowPreview();
        else ShowTurn();
    }

    // ------------------------------------------------------------ Prévia de memorização

    /// <summary>Todas as cartas viradas para cima por alguns segundos, para a equipe decorar.</summary>
    void ShowPreview()
    {
        Clear();
        RefreshHud();
        busy = true; // ninguém clica nas cartas durante a prévia
        firstPick = -1;

        previewLabel = L("", Theme.FontSubheading, FontStyle.Bold, Theme.Yellow);
        BuildBoard(true);

        SetAbility(RoleInfo.AbilityName(gm.Current.role) + " · depois da prévia", null, false);
        SetHint("Memorizem onde está cada carta. Depois todas viram.", Theme.Text);
        HideAction();
        StartCoroutine(PreviewRoutine());
    }

    IEnumerator PreviewRoutine()
    {
        float remaining = gm.PreviewSeconds;
        int lastShown = -1;
        while (remaining > 0)
        {
            int shown = Mathf.CeilToInt(remaining);
            if (shown != lastShown && shown <= 3) SoundManager.Play(Sfx.Countdown); // bipe nos últimos 3 s
            lastShown = shown;
            previewLabel.text = "Memorizem as cartas! Começa em " + shown + " s";
            remaining -= Time.deltaTime;
            yield return null;
        }

        previewLabel.text = "Valendo! Boa sorte, equipe.";
        yield return FlipAllDown();
        ShowTurn();
    }

    /// <summary>Todas as cartas viram juntas (escala X), do mesmo jeito do flip de uma carta.</summary>
    IEnumerator FlipAllDown()
    {
        SoundManager.Play(Sfx.Flip);
        float half = Theme.FlipHalfTime;
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            SetTilesScaleX(1 - t / half);
            yield return null;
        }
        for (int i = 0; i < tileBtns.Count; i++) PaintTile(i, gm.Board[i].matched);
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            SetTilesScaleX(t / half);
            yield return null;
        }
        SetTilesScaleX(1);
    }

    void SetTilesScaleX(float x)
    {
        foreach (var b in tileBtns) b.transform.localScale = new Vector3(x, 1, 1);
    }

    // ------------------------------------------------------------ Mesa da memória

    void ShowTurn()
    {
        Clear();
        RefreshHud();
        SoundManager.Duck(false);
        busy = false;
        firstPick = -1;
        var p = gm.Current;

        // Linha única de altura fixa (encolhe até FontMin se for longa), para a mesa nunca passar da área útil
        var desc = L(RoleInfo.AbilityName(p.role) + ": " + RoleInfo.AbilityDesc(p.role), Theme.FontSmall, FontStyle.Normal, Theme.TextMuted);
        UIFactory.ShrinkToFit(desc);
        desc.GetComponent<LayoutElement>().preferredHeight = Theme.HintLineHeight;
        BuildBoard();

        HideAction();
        if (gm.Health <= Theme.LowHealth)
            SetHint("Cuidado: o Índice está baixo! Vire duas cartas e ache o par.", Theme.Red);
        else
            SetHint("Vire duas cartas e ache o par.", Theme.Text);
        ShowBoardAbility();
    }

    void ShowBoardAbility()
    {
        var p = gm.Current;
        string name = RoleInfo.AbilityName(p.role);
        if (!p.AbilityReady)
        {
            SetAbility(CooldownLabel(p), null, false);
            return;
        }

        if (p.role == Role.Brigadista)
        {
            bool canHeal = gm.Health < gm.maxHealth;
            SetAbility(canHeal ? "Usar: " + name : name + " · Índice cheio", () =>
            {
                if (busy || firstPick >= 0) return; // só antes de virar a primeira carta
                UseAbility();
                gm.Heal(1);
                ShowTurn();
            }, canHeal);
        }
        else
        {
            // As outras habilidades são usadas no desafio; aqui o botão só explica.
            SetAbility(name + " · no desafio", () =>
            {
                if (!busy && firstPick < 0) SetHint("Use " + name + " quando aparecer um desafio.", Theme.Text);
            }, true);
        }
    }

    /// <summary>allFaceUp = true: todas as cartas abertas e sem clique (prévia de memorização).</summary>
    void BuildBoard(bool allFaceUp = false)
    {
        tileBtns.Clear();
        tileTexts.Clear();
        tileTags.Clear();

        int n = gm.Board.Count;
        int cols = n > 20 ? 6 : n > 16 ? 5 : 4;
        int rows = Mathf.CeilToInt(n / (float)cols);
        var spacing = Theme.BoardSpacing;
        float cellW = Mathf.Min(Theme.CardMaxWidth, (Theme.BoardMaxWidth - (cols - 1) * spacing.x) / cols);
        float cellH = Mathf.Min(Theme.CardMaxHeight, (Theme.BoardMaxHeight - (rows - 1) * spacing.y) / rows);

        var gridGo = new GameObject("Mesa", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridGo.transform.SetParent(content, false);
        var grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(cellW, cellH);
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.childAlignment = TextAnchor.UpperCenter;
        gridGo.GetComponent<LayoutElement>().preferredHeight = rows * cellH + (rows - 1) * spacing.y;

        for (int i = 0; i < n; i++)
        {
            int idx = i;
            var btn = UIFactory.MakeButton(gridGo.transform, "", () => OnTile(idx), Theme.TileBack, cellH, -1, Theme.FontSmall);
            var main = btn.GetComponentInChildren<Text>();
            main.rectTransform.offsetMin = new Vector2(Theme.CardPadding, Theme.CardPadding);
            main.rectTransform.offsetMax = new Vector2(-Theme.CardPadding, -(Theme.SpaceS + Theme.CardTagHeight));

            var tag = UIFactory.Label(btn.transform, "", Theme.FontTag, FontStyle.Bold);
            var trt = tag.rectTransform;
            trt.anchorMin = new Vector2(0, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1);
            trt.sizeDelta = new Vector2(-2 * Theme.CardPadding, Theme.CardTagHeight);
            trt.anchoredPosition = new Vector2(0, -Theme.SpaceS);

            tileBtns.Add(btn);
            tileTexts.Add(main);
            tileTags.Add(tag);
            if (allFaceUp) UIFactory.SetInteractable(btn, false);
            PaintTile(i, allFaceUp || gm.Board[i].matched);
        }
    }

    void PaintTile(int i, bool faceUp)
    {
        var tile = gm.Board[i];
        var main = tileTexts[i];
        var tag = tileTags[i];

        if (!faceUp)
        {
            UIFactory.SetStyle(tileBtns[i], Theme.TileBack);
            tag.text = "TURNO SEGURO";
            tag.color = Theme.TextMuted;
            main.text = "?";
            main.fontSize = main.resizeTextMaxSize = Theme.FontTitle;
            return;
        }

        UIFactory.SetStyle(tileBtns[i], tile.matched ? Theme.TileMatched : Theme.TileFace);
        tag.text = tile.Tag;
        tag.color = Theme.OnYellow;
        main.text = tile.Text;
        main.fontSize = main.resizeTextMaxSize = Theme.FontSmall;
    }

    void OnTile(int i)
    {
        if (busy || gm.IsOver || gm.Board[i].matched || i == firstPick) return;
        busy = true;
        StartCoroutine(PickRoutine(i));
    }

    IEnumerator PickRoutine(int i)
    {
        yield return Flip(i);

        if (firstPick < 0)
        {
            firstPick = i;
            SetHint("Agora vire a segunda carta.", Theme.Text);
            var p = gm.Current;
            if (p.role == Role.Brigadista && p.AbilityReady)
                SetAbility(RoleInfo.AbilityName(p.role) + " · só antes de virar", null, false);
            busy = false;
            yield break;
        }

        int first = firstPick;
        firstPick = -1;
        yield return new WaitForSeconds(Theme.RevealPause);

        if (gm.TryMatch(first, i))
        {
            SoundManager.Play(Sfx.PairOk);
            PaintTile(first, true);
            PaintTile(i, true);
            RefreshHud();
            SetHint("Par certo! Auditoria +1. " + gm.Board[i].pair.explanation, Theme.Green);
            SetAction(gm.IsOver ? "Ver resultado" : "Próximo turno", NextTurn);
        }
        else
        {
            SoundManager.Play(Sfx.PairBad);
            SetHint("Não formou par. Prepare-se para o desafio!", Theme.Text);
            SetAction("Encarar desafio", () => ShowCard(gm.DrawChallenge()));
        }
    }

    IEnumerator Flip(int i)
    {
        SoundManager.Play(Sfx.Flip);
        float half = Theme.FlipHalfTime;
        var tr = tileBtns[i].transform;
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            tr.localScale = new Vector3(1 - t / half, 1, 1);
            yield return null;
        }
        PaintTile(i, true);
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            tr.localScale = new Vector3(t / half, 1, 1);
            yield return null;
        }
        tr.localScale = Vector3.one;
    }

    // ------------------------------------------------------------ Desafio

    void ShowCard(CardData c)
    {
        if (c == null)
        {
            NextTurn();
            return;
        }

        hiddenOptions.Clear();
        epiSelected.Clear();
        epiLocked.Clear();
        lockedPrefix = 0;
        SoundManager.Play(Sfx.ChallengeOpen);
        SoundManager.Duck(true);

        switch (c.Type)
        {
            case CardType.Question: ShowQuestion(c); break;
            case CardType.Sequence: PrepareSequence(c); ShowSequence(c); break;
            case CardType.Epi: PrepareEpi(c); ShowEpi(c); break;
            case CardType.Wire: ShowWire(c); break;
            case CardType.Lockout: ShowLockout(c); break;
            case CardType.Hunt: ShowHunt(c); break;
            case CardType.Match: ShowMatch(c); break;
            default: NextTurn(); break; // tipo ainda sem tela: segue o jogo sem penalidade
        }
    }

    void CardHeader(CardData c, string typeLabel)
    {
        L(typeLabel + (string.IsNullOrEmpty(c.nr) ? "" : " • " + c.nr), Theme.FontTag, FontStyle.Bold, Theme.Yellow);
        if (!string.IsNullOrEmpty(c.title)) L(c.title, Theme.FontHeading, FontStyle.Bold);
        if (!string.IsNullOrEmpty(c.prompt)) L(c.prompt, Theme.FontBody);
        UIFactory.Spacer(content, Theme.SpaceS);
    }

    /// <summary>Faixa de baixo durante um desafio: botão da habilidade e dica.</summary>
    void ChallengeBar(CardData c, System.Action rerender)
    {
        HideAction();
        if (gm.ShieldActive)
            SetHint("Escudo da CIPA ativo: um erro neste desafio não tira Índice.", Theme.Green);
        else
            SetHint("Acerte o desafio para proteger o Índice.", Theme.Text);

        var p = gm.Current;
        string name = RoleInfo.AbilityName(p.role);
        if (!p.AbilityReady)
        {
            SetAbility(CooldownLabel(p), null, false);
            return;
        }

        switch (p.role)
        {
            case Role.TecnicoSeguranca:
                SetAbility("Usar: " + name, () => { UseAbility(); ApplyHint(c); rerender(); }, true);
                break;
            case Role.MembroCIPA:
                SetAbility("Usar: " + name, () => { UseAbility(); gm.ShieldActive = true; rerender(); }, true);
                break;
            case Role.Engenheiro:
                SetAbility("Usar: " + name, () =>
                {
                    UseAbility();
                    ShowFeedback(null, "O(a) engenheiro(a) redesenhou o processo e o desafio foi descartado sem penalidade.", c, false, NextTurn);
                }, true);
                break;
            default:
                SetAbility(name + " · só antes de virar", null, false);
                break;
        }
    }

    void ApplyHint(CardData c)
    {
        switch (c.Type)
        {
            case CardType.Question:
                var wrong = new List<int>();
                for (int i = 0; i < c.options.Length; i++)
                    if (i != c.correctIndex) wrong.Add(i);
                int toHide = Mathf.Min(2, wrong.Count - 1); // sempre sobra ao menos 1 errada
                for (int k = 0; k < toHide; k++)
                {
                    int pick = Random.Range(0, wrong.Count);
                    hiddenOptions.Add(wrong[pick]);
                    wrong.RemoveAt(pick);
                }
                break;

            case CardType.Sequence:
                seqOrder.Remove(0);
                seqOrder.Insert(0, 0);
                lockedPrefix = 1;
                break;

            case CardType.Epi:
                foreach (int idx in c.correctItems)
                {
                    if (!epiLocked.Contains(idx))
                    {
                        epiSelected.Add(idx);
                        epiLocked.Add(idx);
                        break;
                    }
                }
                break;

            case CardType.Wire: WireHint(c); break;
            case CardType.Lockout: LockoutHint(); break;
            case CardType.Hunt: HuntHint(); break;
            case CardType.Match: MatchHint(); break;
        }
    }

    // ------------------------------------------------------------ Pergunta

    void ShowQuestion(CardData c)
    {
        Clear();
        RefreshHud();
        CardHeader(c, "Desafio: Pergunta");

        for (int i = 0; i < c.options.Length; i++)
        {
            int idx = i;
            var btn = UIFactory.MakeButton(content, (char)('A' + i) + ")  " + c.options[i],
                () => Resolve(c, idx == c.correctIndex), Theme.Secondary, Theme.OptionHeight, -1, Theme.FontBody);
            if (hiddenOptions.Contains(i))
            {
                btn.GetComponentInChildren<Text>().text = "(eliminada pela Dica Técnica)";
                UIFactory.SetInteractable(btn, false);
            }
        }
        ChallengeBar(c, () => ShowQuestion(c));
    }

    // ------------------------------------------------------------ Sequência

    void PrepareSequence(CardData c)
    {
        seqOrder.Clear();
        for (int i = 0; i < c.steps.Length; i++) seqOrder.Add(i);
        for (int i = seqOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int t = seqOrder[i]; seqOrder[i] = seqOrder[j]; seqOrder[j] = t;
        }
        if (seqOrder.Count > 1 && IsSequenceCorrect())
        {
            int t = seqOrder[0]; seqOrder[0] = seqOrder[1]; seqOrder[1] = t;
        }
    }

    void ShowSequence(CardData c)
    {
        Clear();
        RefreshHud();
        CardHeader(c, "Desafio: Sequência");
        L("Use Subir/Descer para colocar os passos na ordem correta.", Theme.FontSmall, FontStyle.Italic, Theme.TextMuted);

        for (int pos = 0; pos < seqOrder.Count; pos++)
        {
            int p = pos;
            bool locked = p < lockedPrefix;
            var row = UIFactory.Row(content, Theme.CompactRowHeight);

            var lbl = UIFactory.Label(row, (p + 1) + ".  " + c.steps[seqOrder[p]] + (locked ? "  (fixado)" : ""),
                Theme.FontBody, FontStyle.Normal, TextAnchor.MiddleLeft, locked ? Theme.Green : Theme.Text);
            UIFactory.ShrinkToFit(lbl);
            var le = lbl.GetComponent<LayoutElement>();
            le.preferredWidth = 1;
            le.flexibleWidth = 1;

            var up = UIFactory.MakeButton(row, "Subir", () => { MoveStep(p, -1); ShowSequence(c); }, Theme.Secondary,
                Theme.CompactRowHeight, Theme.SmallButtonWidth);
            UIFactory.SetInteractable(up, !locked && p - 1 >= lockedPrefix);

            var down = UIFactory.MakeButton(row, "Descer", () => { MoveStep(p, 1); ShowSequence(c); }, Theme.Secondary,
                Theme.CompactRowHeight, Theme.SmallButtonWidth);
            UIFactory.SetInteractable(down, !locked && p + 1 < seqOrder.Count);
        }

        ChallengeBar(c, () => ShowSequence(c));
        SetAction("Confirmar ordem", () => Resolve(c, IsSequenceCorrect())); // na faixa de baixo: sempre visível
    }

    void MoveStep(int pos, int dir)
    {
        int target = pos + dir;
        if (pos < lockedPrefix || target < lockedPrefix || target >= seqOrder.Count) return;
        int t = seqOrder[pos]; seqOrder[pos] = seqOrder[target]; seqOrder[target] = t;
    }

    bool IsSequenceCorrect()
    {
        for (int i = 0; i < seqOrder.Count; i++)
            if (seqOrder[i] != i) return false;
        return true;
    }

    // ------------------------------------------------------------ EPI

    void PrepareEpi(CardData c)
    {
        epiOrder.Clear();
        for (int i = 0; i < c.items.Length; i++) epiOrder.Add(i);
        for (int i = epiOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int t = epiOrder[i]; epiOrder[i] = epiOrder[j]; epiOrder[j] = t;
        }
    }

    void ShowEpi(CardData c)
    {
        Clear();
        RefreshHud();
        CardHeader(c, "Desafio: EPI");
        L("Selecione TODOS os itens necessários e nenhum a mais.", Theme.FontSmall, FontStyle.Italic, Theme.TextMuted);

        var gridGo = new GameObject("Grade", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridGo.transform.SetParent(content, false);
        var grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = Theme.EpiCellSize;
        grid.spacing = Theme.EpiSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperCenter;
        int rows = Mathf.CeilToInt(c.items.Length / 3f);
        gridGo.GetComponent<LayoutElement>().preferredHeight = rows * (Theme.EpiCellSize.y + Theme.EpiSpacing.y);

        foreach (int i in epiOrder)
        {
            int idx = i;
            bool selected = epiSelected.Contains(i);
            bool locked = epiLocked.Contains(i);
            UIFactory.MakeButton(gridGo.transform, c.items[i] + (locked ? " (dica)" : ""), () =>
            {
                if (epiLocked.Contains(idx)) return; // revelado pela Dica Técnica: fica marcado
                if (epiSelected.Contains(idx)) epiSelected.Remove(idx);
                else epiSelected.Add(idx);
                ShowEpi(c);
            }, selected ? Theme.Ability : Theme.Secondary, Theme.OptionHeight, -1, Theme.FontBody);
        }

        ChallengeBar(c, () => ShowEpi(c));
        SetAction("Confirmar EPIs", () => Resolve(c, IsEpiCorrect(c)));
    }

    bool IsEpiCorrect(CardData c)
    {
        var correct = new HashSet<int>(c.correctItems);
        return correct.SetEquals(epiSelected);
    }

    // ------------------------------------------------------------ Evento

    void ShowEvent(CardData c)
    {
        Clear();
        RefreshHud();
        HideAbility();
        HideAction();
        SetHint("Algo aconteceu na empresa. Leia e aplique o evento.", Theme.Text);

        L("Início da rodada " + gm.Round + ": algo aconteceu na empresa", Theme.FontSmall, FontStyle.Italic, Theme.TextMuted);
        CardHeader(c, "Evento");

        string effect = DescribeEvent(c);
        L(effect, Theme.FontSubheading, FontStyle.Bold, c.healthDelta < 0 || c.auditDelta < 0 ? Theme.Red : Theme.Green);
        UIFactory.Spacer(content, Theme.SpaceM);
        B("Aplicar evento", () =>
        {
            var revealed = gm.ApplyEvent(c);
            SoundManager.Play(c.healthDelta < 0 ? Sfx.EventBad : Sfx.EventGood);
            string msg = effect;
            foreach (var p in revealed) msg += "\nPar aberto: " + p.a + " ↔ " + p.b;
            ShowFeedback(null, msg, c, false, () => { if (gm.IsOver) ShowEnd(); else ShowTurn(); });
        }, Theme.Primary);
    }

    static string DescribeEvent(CardData c)
    {
        var parts = new List<string>();
        if (c.healthDelta != 0) parts.Add((c.healthDelta > 0 ? "+" : "") + c.healthDelta + " no Índice de Segurança");
        if (c.auditDelta > 0) parts.Add("+" + c.auditDelta + " na Auditoria (um par é aberto)");
        return parts.Count == 0 ? "Sem efeito nos marcadores." : string.Join(" e ", parts.ToArray());
    }

    // ------------------------------------------------------------ Resultado

    void Resolve(CardData c, bool correct)
    {
        int lost = gm.ResolveChallenge(c, correct);
        SoundManager.Play(correct ? Sfx.Success : Sfx.Fail);
        string msg;
        if (correct) msg = "A equipe venceu o desafio e não perdeu Índice.";
        else if (lost == 0) msg = "O escudo da CIPA evitou a perda de Índice.";
        else msg = "-" + lost + " no Índice de Segurança.";
        ShowFeedback(correct, msg, c, true, NextTurn);

        if (correct) SetHint("Acertou! O Índice está protegido.", Theme.Text);
        else if (lost == 0) SetHint("Errou, mas o escudo da CIPA protegeu o Índice.", Theme.Text);
        else SetHint("Errou. O Índice caiu " + lost + ". Leia a explicação.", Theme.Text);
    }

    void ShowFeedback(bool? correct, string message, CardData c, bool showAnswer, UnityAction next)
    {
        Clear();
        RefreshHud();
        HideAbility();
        HideAction();
        SetHint("", Theme.Text);
        UIFactory.Spacer(content, Theme.SpaceM);

        if (correct == null) L("Carta resolvida", Theme.FontHeading, FontStyle.Bold, Theme.Yellow);
        else if (correct.Value) L("Correto!", Theme.FontTitle, FontStyle.Bold, Theme.Green);
        else L("Incorreto", Theme.FontTitle, FontStyle.Bold, Theme.Red);

        L(message, Theme.FontSubheading);

        if (showAnswer && correct == false && c != null)
        {
            string answer = CorrectAnswerText(c);
            if (!string.IsNullOrEmpty(answer)) L(answer, Theme.FontBody, FontStyle.Normal, Theme.Yellow);
        }
        if (showAnswer && c != null && !string.IsNullOrEmpty(c.explanation))
            L("Por quê: " + c.explanation, Theme.FontBody, FontStyle.Normal, Theme.TextMuted);

        UIFactory.Spacer(content, Theme.SpaceM);
        B(gm.IsOver ? "Ver resultado" : "Continuar", next, Theme.Primary, Theme.ButtonBigHeight);
    }

    static string CorrectAnswerText(CardData c)
    {
        switch (c.Type)
        {
            case CardType.Question:
                return "Resposta correta: " + c.options[c.correctIndex];
            case CardType.Wire:
                return WireAnswerText(c);
            case CardType.Hunt:
                return HuntAnswerText(c);
            case CardType.Match:
                return MatchAnswerText(c);
            case CardType.Sequence:
            case CardType.Lockout:
                var sb = new StringBuilder("Ordem correta:\n");
                for (int i = 0; i < c.steps.Length; i++) sb.Append(i + 1).Append(". ").Append(c.steps[i]).Append('\n');
                return sb.ToString().TrimEnd();
            case CardType.Epi:
                var names = new List<string>();
                foreach (int i in c.correctItems) names.Add(c.items[i]);
                return "EPIs corretos: " + string.Join(", ", names.ToArray());
            default:
                return null;
        }
    }

    void NextTurn()
    {
        if (gm.IsOver)
        {
            ShowEnd();
            return;
        }
        gm.EndTurn();
        if (gm.EventPending)
        {
            gm.EventPending = false;
            var ev = gm.Events.Draw();
            if (ev != null)
            {
                ShowEvent(ev);
                return;
            }
        }
        ShowTurn();
    }

    void ShowEnd()
    {
        Clear();
        RefreshHud();
        HideAbility();
        HideAction();
        SetHint("", Theme.Text);
        bool won = gm.Won;
        SoundManager.StopMusic();
        SoundManager.Play(won ? Sfx.Victory : Sfx.Defeat);

        UIFactory.Spacer(content, Theme.SpaceL);
        L(won ? "AUDITORIA APROVADA!" : "ACIDENTE GRAVE NA EMPRESA", Theme.FontTitle, FontStyle.Bold, won ? Theme.Green : Theme.Red);
        L(won
            ? "A equipe encontrou todos os pares em " + gm.Round + " rodada(s). A empresa segue sem acidentes!"
            : "O Índice de Segurança chegou a zero. Revisem as NRs e tentem de novo.", Theme.FontSubheading);
        L("Pares: " + gm.Audit + "/" + gm.AuditGoal + "   |   Erros de memória: " + gm.MemoryMisses +
          "   |   Desafios vencidos: " + gm.ChallengesWon + "   |   perdidos: " + gm.ChallengesLost,
          Theme.FontBody, FontStyle.Normal, Theme.TextMuted);
        UIFactory.Spacer(content, Theme.SpaceL);
        B("Jogar novamente", ShowSetup, Theme.Primary, Theme.ButtonBigHeight);
    }
}

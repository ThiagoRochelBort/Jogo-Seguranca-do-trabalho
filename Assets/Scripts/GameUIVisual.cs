using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cenas desenhadas dos desafios práticos wire e match ("Desafios práticos — visual" do GDD).
/// Só aparecem quando a carta tem os campos visuais (epiIcons, wireColors, device; sIcon, aIcon, extraIcons);
/// sem eles fica o visual simples de GameUIPractical. Regras e estado são os mesmos dos dois visuais.
/// </summary>
public partial class GameUI
{
    // ------------------------------------------------------------ wire desenhado

    bool wireFancy;
    RectTransform wireScene, wireBody, wireCables, wireDeviceIcon;
    Image wireLight;
    Vector2 wireDevicePos;
    readonly List<Image> wireEpiDots = new List<Image>();
    readonly List<CanvasGroup> wireEpiFades = new List<CanvasGroup>();
    readonly List<Image> wireTermRings = new List<Image>();
    readonly List<Image> wireBorneRings = new List<Image>();
    readonly List<Vector2> wireTermPos = new List<Vector2>();
    readonly List<Vector2> wireBornePos = new List<Vector2>();

    static bool HasAny(string[] list)
    {
        if (list == null) return false;
        foreach (var s in list) if (!string.IsNullOrEmpty(s)) return true;
        return false;
    }

    static bool WireIsFancy(CardData c)
    {
        if (c.epiItems.Length > Theme.WireMaxEpiCards) return false; // não cabem na bancada desenhada: usa o visual simples
        return HasAny(c.epiIcons) || HasAny(c.wireColors) || !string.IsNullOrEmpty(c.device);
    }

    float WireRowY(int k)
    {
        return Theme.WireRowTop - k * Theme.WireRowStep;
    }

    /// <summary>Texto solto numa área de centro 0,0.</summary>
    static Text SceneLabel(Transform parent, string text, Vector2 pos, Vector2 size, int fontSize, Color color, TextAnchor align)
    {
        var t = UIFactory.Label(parent, text, fontSize, FontStyle.Bold, align, color);
        var rt = t.rectTransform;
        var center = new Vector2(0.5f, 0.5f);
        rt.anchorMin = center;
        rt.anchorMax = center;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return t;
    }

    /// <summary>Botão posicionado pelo centro numa área de centro 0,0.</summary>
    static Button SceneButton(Transform parent, string label, UnityEngine.Events.UnityAction action, ButtonStyle style,
        Vector2 pos, Vector2 size, int fontSize)
    {
        var btn = UIFactory.MakeButton(parent, label, action, style, size.y, size.x, fontSize);
        var rt = btn.GetComponent<RectTransform>();
        var center = new Vector2(0.5f, 0.5f);
        rt.anchorMin = center;
        rt.anchorMax = center;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return btn;
    }

    /// <summary>Cor do fio i (e se leva contorno claro ou listras). Nome desconhecido = cinza.</summary>
    static Color WireColor(CardData c, int i, out bool outline, out bool stripes)
    {
        outline = false;
        stripes = false;
        switch ((Icons.At(c.wireColors, i) ?? "").Trim().ToLowerInvariant())
        {
            case "preto": outline = true; return Theme.Background;
            case "azul": return Theme.WireBlue;
            case "marrom": outline = true; return Theme.WireBrown;
            case "verde_amarelo": stripes = true; return Theme.Green;
            default: return Theme.TextMuted;
        }
    }

    static string DeviceName(string device)
    {
        switch ((device ?? "").Trim().ToLowerInvariant())
        {
            case "tomada": return "TOMADA";
            case "bomba": return "BOMBA D'ÁGUA";
            case "luminaria": return "LUMINÁRIA";
            case "painel": return "PAINEL";
            default: return "EQUIPAMENTO";
        }
    }

    static RectTransform DeviceIcon(Transform parent, string device, float size)
    {
        switch ((device ?? "").Trim().ToLowerInvariant())
        {
            case "tomada": return Icons.Make(parent, "tomada", size);
            case "bomba": return Icons.Make(parent, "maquina", size);
            case "painel": return Icons.Make(parent, "painel", size);
            case "luminaria":
                var root = Piece(parent, "Luminaria", Vector2.zero, new Vector2(Theme.IconBase, Theme.IconBase));
                root.localScale = Vector3.one * (size / Theme.IconBase);
                var p = new Icons.Pen(root);
                p.Rect(0, -26, 12, 26, Theme.TextMuted);
                p.Box(0, -40, 52, 10, Theme.SurfaceHigh);
                p.Disc(0, 12, 60, Theme.Yellow);
                return root;
            default: return null;
        }
    }

    void BuildWireScene(CardData c)
    {
        wireEpiDots.Clear();
        wireEpiFades.Clear();
        wireTermRings.Clear();
        wireBorneRings.Clear();
        wireTermPos.Clear();
        wireBornePos.Clear();

        var holder = new GameObject("Cena", typeof(RectTransform), typeof(LayoutElement));
        holder.transform.SetParent(content, false);
        holder.GetComponent<LayoutElement>().preferredHeight = Theme.WireSceneSize.y;
        wireScene = Piece(holder.transform, "Bancada", Vector2.zero, Theme.WireSceneSize);

        float w = Theme.WireSceneSize.x;
        float h = Theme.WireSceneSize.y;
        float gap = Theme.WireCardGap;
        var card = Theme.WireCardSize;
        float gridLeft = -w / 2 + gap;
        float gridWidth = 3 * card.x + 2 * gap;
        float top = h / 2 - gap;

        // Passo 1: cartões de EPI com ícone (3 por linha)
        int epiRows = 0;
        for (int i = 0; i < c.epiItems.Length; i++)
        {
            int idx = i;
            int col = i % 3;
            int row = i / 3;
            epiRows = row + 1;
            var pos = new Vector2(gridLeft + card.x / 2 + col * (card.x + gap), top - card.y / 2 - row * (card.y + gap));
            var btn = SceneButton(wireScene, c.epiItems[i], () => OnWireEpi(idx), Theme.Card, pos, card, Theme.FontTag);

            var text = btn.GetComponentInChildren<Text>().rectTransform;
            var icon = Icons.Make(btn.transform, Icons.At(c.epiIcons, i), Theme.WireCardIcon);
            if (icon != null)
            {
                icon.anchoredPosition = new Vector2(0, card.y / 2 - gap - Theme.WireCardIcon / 2);
                text.offsetMin = new Vector2(gap, Theme.ButtonTextPaddingY);
                text.offsetMax = new Vector2(-gap, -(gap + Theme.WireCardIcon));
            }
            else
            {
                text.offsetMin = new Vector2(gap, Theme.ButtonTextPaddingY);
                text.offsetMax = new Vector2(-gap, -Theme.ButtonTextPaddingY);
            }

            var dot = PieceImage(btn.transform, "Marcado", new Vector2(card.x / 2 - 16, card.y / 2 - 16),
                new Vector2(Theme.MatchPin, Theme.MatchPin), Theme.Green, Theme.Circle, false);
            dot.enabled = false;
            wireEpiDots.Add(dot);
            wireEpiFades.Add(btn.gameObject.AddComponent<CanvasGroup>());
            wireEpiBtns.Add(btn);
        }

        // Boneco embaixo da grade
        float gridBottom = top - epiRows * (card.y + gap);
        var bodyPos = new Vector2(gridLeft + gridWidth / 2, (gridBottom + (-h / 2)) / 2);
        wireBody = Icons.Make(wireScene, "pessoa", Theme.WireBodySize);
        wireBody.anchoredPosition = bodyPos;

        // Fonte (caixa de luz) com os terminais nas cores dos fios
        float boxLeft = gridLeft + gridWidth + 2 * gap;
        float boxCenter = boxLeft + Theme.WireBoxWidth / 2;
        float termX = boxLeft + Theme.WireBoxWidth - 30;
        PieceImage(wireScene, "Fonte", new Vector2(boxCenter, 0), new Vector2(Theme.WireBoxWidth, Theme.WirePanelHeight),
            Theme.Surface, Theme.Rounded, true);
        SceneLabel(wireScene, "FONTE", new Vector2(boxCenter, Theme.WirePanelHeight / 2 - 22),
            new Vector2(Theme.WireBoxWidth - 2 * gap, 30), Theme.FontTag, Theme.TextMuted, TextAnchor.MiddleCenter);

        for (int i = 0; i < c.left.Length; i++)
        {
            int idx = i;
            float y = WireRowY(i);
            var btn = SceneButton(wireScene, c.left[i], () => OnWireLeft(idx), Theme.Ghost,
                new Vector2(boxCenter, y), new Vector2(Theme.WireBoxWidth - 2 * gap, Theme.WireRowHeight), Theme.FontTag);
            var t = btn.GetComponentInChildren<Text>();
            t.alignment = TextAnchor.MiddleLeft;
            t.rectTransform.offsetMin = new Vector2(gap, Theme.ButtonTextPaddingY);
            t.rectTransform.offsetMax = new Vector2(-(Theme.WireTerminal + 3 * gap), -Theme.ButtonTextPaddingY);
            wireLeft.Add(btn);

            bool outline, stripes;
            var disc = PieceImage(wireScene, "Terminal", new Vector2(termX, y), Vector2.one * Theme.WireTerminal,
                WireColor(c, i, out outline, out stripes), Theme.Circle, false);
            AddOutline(disc, Theme.Text);
            if (stripes) PieceImage(disc.transform, "Listra", Vector2.zero, new Vector2(Theme.WireTerminal, 8), Theme.Yellow, null, false);
            var ring = PieceImage(wireScene, "Aro", new Vector2(termX, y), Vector2.one * Theme.WireRing, Theme.Yellow, Theme.IconRing, false);
            ring.enabled = false;
            wireTermRings.Add(ring);
            wireTermPos.Add(new Vector2(termX + Theme.WireTerminal / 2, y));
        }

        // Equipamento à direita, com os bornes na lateral esquerda
        float devRight = w / 2 - gap;
        float devLeft = devRight - Theme.WireDeviceWidth;
        float devCenter = (devLeft + devRight) / 2;
        float borneX = devLeft + 30;
        PieceImage(wireScene, "Equipamento", new Vector2(devCenter, 0), new Vector2(Theme.WireDeviceWidth, Theme.WirePanelHeight),
            Theme.Surface, Theme.Rounded, true);
        SceneLabel(wireScene, DeviceName(c.device), new Vector2(devCenter - Theme.WireLight, Theme.WirePanelHeight / 2 - 22),
            new Vector2(Theme.WireDeviceWidth - 3 * Theme.WireLight, 30), Theme.FontTag, Theme.TextMuted, TextAnchor.MiddleLeft);
        wireLight = PieceImage(wireScene, "Luz", new Vector2(devRight - Theme.WireLight, Theme.WirePanelHeight / 2 - Theme.WireLight),
            Vector2.one * Theme.WireLight, Theme.SurfaceHigh, Theme.Circle, false);
        AddOutline(wireLight, Theme.Text);

        wireDevicePos = new Vector2(devCenter + Theme.WireLight, -Theme.WirePanelHeight / 2 + Theme.WireDeviceIcon / 2 + 2 * gap);
        wireDeviceIcon = DeviceIcon(wireScene, c.device, Theme.WireDeviceIcon);
        if (wireDeviceIcon != null) wireDeviceIcon.anchoredPosition = wireDevicePos;

        for (int j = 0; j < c.right.Length; j++)
        {
            int idx = j;
            float y = WireRowY(j);
            var btn = SceneButton(wireScene, c.right[j], () => OnWireRight(idx), Theme.Ghost,
                new Vector2(devCenter, y), new Vector2(Theme.WireDeviceWidth - 2 * gap, Theme.WireRowHeight), Theme.FontTag);
            var t = btn.GetComponentInChildren<Text>();
            t.alignment = TextAnchor.MiddleLeft;
            t.rectTransform.offsetMin = new Vector2(Theme.WireTerminal + 4 * gap, Theme.ButtonTextPaddingY);
            t.rectTransform.offsetMax = new Vector2(-gap, -Theme.ButtonTextPaddingY);
            wireRight.Add(btn);

            var disc = PieceImage(wireScene, "Borne", new Vector2(borneX, y), Vector2.one * Theme.WireTerminal,
                Theme.CardFace, Theme.Circle, false);
            AddOutline(disc, Theme.Background);
            var ring = PieceImage(wireScene, "Aro", new Vector2(borneX, y), Vector2.one * Theme.WireRing, Theme.Green, Theme.IconRing, false);
            ring.enabled = false;
            wireBorneRings.Add(ring);
            wireBornePos.Add(new Vector2(borneX - Theme.WireTerminal / 2, y));
        }

        // Cabos por cima das caixas
        wireCables = UIFactory.Rect(wireScene, "Cabos", Vector2.zero, Vector2.one);
    }

    static void AddOutline(Graphic g, Color color)
    {
        var o = g.gameObject.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = new Vector2(Theme.IconOutline, -Theme.IconOutline);
    }

    void RefreshWireScene()
    {
        var c = practical;
        for (int i = 0; i < wireEpiBtns.Count; i++)
        {
            bool correct = System.Array.IndexOf(c.epiCorrect, i) >= 0;
            bool selected = wireEpiSelected.Contains(i);
            bool faded = wireEpiDisabled.Contains(i);
            var style = wireEpiDone ? (correct ? Theme.Correct : Theme.Card) : (selected ? Theme.CardSelected : Theme.Card);
            UIFactory.SetStyle(wireEpiBtns[i], style);
            UIFactory.SetInteractable(wireEpiBtns[i], !practicalDone && !wireEpiDone && !faded);
            wireEpiDots[i].enabled = selected && !wireEpiDone;
            wireEpiFades[i].alpha = faded ? Theme.FadedAlpha : 1;
        }

        bool active = wireEpiDone && !practicalDone;
        for (int i = 0; i < wireLeft.Count; i++)
        {
            bool linked = wireLinked.Contains(i);
            wireTermRings[i].enabled = linked || i == wireSelectedLeft;
            wireTermRings[i].color = linked ? Theme.Green : Theme.Yellow;
            UIFactory.SetInteractable(wireLeft[i], active && !linked);
        }
        for (int j = 0; j < wireRight.Count; j++)
        {
            bool linked = IsRightLinked(j);
            wireBorneRings[j].enabled = linked;
            wireBorneRings[j].color = Theme.Green;
            UIFactory.SetInteractable(wireRight[j], active && !linked);
        }
    }

    /// <summary>EPI certo: cada EPI certo desce e encaixa no boneco; no fim, brilho verde.</summary>
    IEnumerator DressRoutine()
    {
        wireDressExtra = 0;
        foreach (int i in practical.epiCorrect) StartCoroutine(DressFx(Icons.At(practical.epiIcons, i)));
        yield return new WaitForSeconds(Theme.DressTime);
        StartCoroutine(GlowFx(wireBody, false, Theme.Green));
    }

    int wireDressExtra; // EPIs sem lugar no corpo ficam empilhados ao lado do boneco

    /// <summary>Monta a peça do EPI (posição relativa ao boneco) e anima a descida. Nome desconhecido = nada.</summary>
    IEnumerator DressFx(string icon)
    {
        string key = (icon ?? "").Trim().ToLowerInvariant();
        Vector2 offset;
        float size;
        switch (key)
        {
            case "capacete": offset = new Vector2(0, 60); size = 56; break;
            case "oculos": offset = new Vector2(0, 40); size = 40; break;
            case "respirador":
            case "mascara_solda": offset = new Vector2(0, 36); size = 44; break;
            case "protetor_auricular": offset = new Vector2(0, 42); size = 64; break;
            case "colete":
            case "cinturao": offset = new Vector2(0, -26); size = 64; break;
            case "luva": offset = new Vector2(0, -40); size = 0; break;
            case "calcado": offset = new Vector2(0, -70); size = 0; break;
            default:
                if (!Icons.Has(key)) return Nothing();
                offset = new Vector2(Theme.WireBodySize / 2 + Theme.WireCardGap, Theme.WireBodySize / 4 - wireDressExtra * Theme.MatchIconSize);
                size = Theme.WireCardIcon;
                wireDressExtra++;
                break;
        }
        return DressMove(key, offset, size);
    }

    static IEnumerator Nothing()
    {
        yield break;
    }

    IEnumerator DressMove(string key, Vector2 offset, float size)
    {
        if (wireBody == null) yield break;
        Vector2 target = wireBody.anchoredPosition + offset;
        var piece = Piece(wireScene, "Vestido_" + key, target + Vector2.up * Theme.DressRise, Vector2.zero);
        var fade = piece.gameObject.AddComponent<CanvasGroup>();
        fade.alpha = 0;

        if (key == "luva")
        {
            PieceImage(piece, "Mao", new Vector2(-42, 0), new Vector2(20, 20), Theme.Yellow, Theme.Circle, false);
            PieceImage(piece, "Mao", new Vector2(42, 0), new Vector2(20, 20), Theme.Yellow, Theme.Circle, false);
        }
        else if (key == "calcado")
        {
            PieceImage(piece, "Pe", new Vector2(-16, 0), new Vector2(28, 16), Theme.CardFace, Theme.Rounded, true);
            PieceImage(piece, "Pe", new Vector2(16, 0), new Vector2(28, 16), Theme.CardFace, Theme.Rounded, true);
        }
        else
        {
            Icons.Make(piece, key, size);
        }

        for (float t = 0; t < Theme.DressTime; t += Time.deltaTime)
        {
            if (piece == null) yield break;
            float k = t / Theme.DressTime;
            piece.anchoredPosition = Vector2.Lerp(target + Vector2.up * Theme.DressRise, target, k);
            fade.alpha = k;
            yield return null;
        }
        if (piece == null) yield break;
        piece.anchoredPosition = target;
        fade.alpha = 1;
    }

    /// <summary>EPI errado: o boneco e os cartões marcados que não servem tremem em vermelho.</summary>
    void WireEpiWrongFx()
    {
        StartCoroutine(ShakeFx(wireBody));
        for (int i = 0; i < wireEpiBtns.Count; i++)
        {
            if (!wireEpiSelected.Contains(i) || System.Array.IndexOf(practical.epiCorrect, i) >= 0) continue;
            UIFactory.SetStyle(wireEpiBtns[i], Theme.Wrong);
            StartCoroutine(ShakeFx(wireEpiBtns[i].GetComponent<RectTransform>()));
        }
        StartCoroutine(AfterDelay(Theme.FlashTime, RefreshWire));
    }

    /// <summary>Cabo esticando do terminal até o borne. Certo: fica, com faísca amarela. Errado: vermelho, treme e recolhe.</summary>
    IEnumerator WireCableFx(int left, int right, bool correct)
    {
        bool outline, stripes;
        Color color = WireColor(practical, left, out outline, out stripes);
        if (!correct)
        {
            color = Theme.Red;
            outline = false;
            stripes = false;
        }

        Vector2 a = wireTermPos[left];
        Vector2 b = wireBornePos[right];
        var points = new[] { a, a + Vector2.right * Theme.CableBend, b - Vector2.right * Theme.CableBend, b };
        var path = NewPath(wireCables, points, Theme.CableThickness, color, outline);
        yield return GrowPath(path, 0, 1, Theme.CableTime);
        if (path.root == null) yield break;

        if (correct)
        {
            if (stripes) StripePath(path, Theme.CableThickness, Theme.Yellow);
            StartCoroutine(SparksFx(wireCables, b, Theme.Yellow, 4, Theme.SparkSize, Theme.SparkDistance));
            yield break;
        }

        StartCoroutine(SparksFx(wireCables, b, Theme.Red, 4, Theme.SparkSize, Theme.SparkDistance));
        wireBorneRings[right].enabled = true;
        wireBorneRings[right].color = Theme.Red;
        StartCoroutine(AfterDelay(Theme.FlashTime, RefreshWire));
        StartCoroutine(ShakeFx(path.root));
        yield return GrowPath(path, 1, 0, Theme.ShakeTime);
        if (path.root != null) Destroy(path.root.gameObject);
    }

    /// <summary>Fim do wire: certo = luz verde e brilho no equipamento; falha = faísca vermelha grande.</summary>
    void WireFinishFx(bool success)
    {
        if (success)
        {
            wireLight.color = Theme.Green;
            StartCoroutine(GlowFx(wireLight.rectTransform, true, Theme.Green));
            if (wireDeviceIcon != null) StartCoroutine(GlowFx(wireDeviceIcon, false, Theme.Green));
        }
        else
        {
            StartCoroutine(SparksFx(wireScene, wireDevicePos, Theme.Red, 8, Theme.BigSparkSize, Theme.BigSparkDistance));
        }
    }

    // ------------------------------------------------------------ match desenhado

    bool matchFancy;
    RectTransform matchLinesRoot;
    readonly List<Image> matchPins = new List<Image>();
    readonly List<Vector2> matchLeftPin = new List<Vector2>();
    readonly List<Vector2> matchRightPin = new List<Vector2>();
    readonly List<Path> matchPaths = new List<Path>();

    static bool MatchIsFancy(CardData c)
    {
        if (HasAny(c.extraIcons)) return true;
        foreach (var m in c.matchPairs)
            if (!string.IsNullOrEmpty(m.sIcon) || !string.IsNullOrEmpty(m.aIcon)) return true;
        return false;
    }

    /// <summary>Duas colunas de cartões com ícone e um espaço no meio para as linhas.</summary>
    void BuildMatchBoard(CardData c)
    {
        matchPins.Clear();
        matchLeftPin.Clear();
        matchRightPin.Clear();
        matchPaths.Clear();

        var holder = new GameObject("Colunas", typeof(RectTransform), typeof(LayoutElement));
        holder.transform.SetParent(content, false);
        holder.GetComponent<LayoutElement>().preferredHeight = Theme.MatchAreaHeight;
        var board = Piece(holder.transform, "Quadro", Vector2.zero,
            new Vector2(2 * Theme.MatchCardWidth + Theme.MatchGap, Theme.MatchAreaHeight));
        matchLinesRoot = UIFactory.Rect(board, "Linhas", Vector2.zero, Vector2.one);

        int n = c.matchPairs.Length;
        int rows = Mathf.Max(n, matchActOrder.Count);
        float rowH = Theme.MatchAreaHeight / rows;
        float cardH = Mathf.Min(Theme.MatchCardMaxHeight, rowH - Theme.MatchRowGap);
        float xLeft = -(Theme.MatchGap / 2 + Theme.MatchCardWidth / 2);

        for (int k = 0; k < n; k++)
        {
            int pos = k;
            var m = c.matchPairs[matchSitOrder[k]];
            float y = MatchRowY(k, n, rows, rowH);
            matchLeft.Add(MatchCard(board, m.s, m.sIcon, () => OnMatchLeft(pos), new Vector2(xLeft, y), cardH));
            var pin = PieceImage(board, "Pino", new Vector2(-Theme.MatchGap / 2, y), Vector2.one * Theme.MatchPin,
                Theme.Yellow, Theme.Circle, false);
            pin.enabled = false;
            matchPins.Add(pin);
            matchLeftPin.Add(new Vector2(-Theme.MatchGap / 2, y));
        }
        for (int k = 0; k < matchActOrder.Count; k++)
        {
            int pos = k;
            int act = matchActOrder[k];
            string icon = act >= 0 ? c.matchPairs[act].aIcon : Icons.At(c.extraIcons, -1 - act);
            float y = MatchRowY(k, matchActOrder.Count, rows, rowH);
            matchRight.Add(MatchCard(board, MatchActionText(act), icon, () => OnMatchRight(pos), new Vector2(-xLeft, y), cardH));
            matchRightPin.Add(new Vector2(Theme.MatchGap / 2, y));
        }
    }

    static float MatchRowY(int k, int count, int rows, float rowH)
    {
        return Theme.MatchAreaHeight / 2 - (k + (rows - count) / 2f + 0.5f) * rowH;
    }

    static Button MatchCard(Transform parent, string text, string icon, UnityEngine.Events.UnityAction action, Vector2 pos, float h)
    {
        var btn = SceneButton(parent, text, action, Theme.Secondary, pos, new Vector2(Theme.MatchCardWidth, h), Theme.FontBody);
        float iconSize = Mathf.Min(Theme.MatchIconSize, h - 2 * Theme.ButtonTextPaddingY);
        var rt = Icons.Make(btn.transform, icon, iconSize);
        if (rt != null)
        {
            float pad = Theme.ButtonTextPadding;
            rt.anchoredPosition = new Vector2(-Theme.MatchCardWidth / 2 + pad + iconSize / 2, 0);
            var t = btn.GetComponentInChildren<Text>();
            t.alignment = TextAnchor.MiddleLeft;
            t.rectTransform.offsetMin = new Vector2(2 * pad + iconSize, Theme.ButtonTextPaddingY);
        }
        return btn;
    }

    /// <summary>Pino da situação escolhida pulsa enquanto ela estiver escolhida.</summary>
    IEnumerator PinPulse(int k)
    {
        var rt = matchPins[k].rectTransform;
        float t = 0;
        while (matchSelected == k && rt != null)
        {
            rt.localScale = Vector3.one * Mathf.Lerp(1, Theme.PinPulseScale, Mathf.Abs(Mathf.Sin(t / Theme.PinPulseTime * Mathf.PI)));
            t += Time.deltaTime;
            yield return null;
        }
        if (rt != null) rt.localScale = Vector3.one;
    }

    /// <summary>Acerto: linha cresce do pino até a ação, com brilho correndo; Dica = amarela por 1 s.</summary>
    IEnumerator MatchLineFx(int leftK, int rightK, bool byHint)
    {
        var path = NewPath(matchLinesRoot, new[] { matchLeftPin[leftK], matchRightPin[rightK] },
            Theme.MatchLineThickness, byHint ? Theme.Yellow : Theme.Green, false);
        matchPaths.Add(path);
        StartCoroutine(RunnerFx(path, Theme.Text, 2 * Theme.MatchLineThickness));
        yield return GrowPath(path, 0, 1, Theme.CableTime);
        StartCoroutine(GlowFx(matchLeft[leftK].GetComponent<RectTransform>(), false, Theme.Green));
        StartCoroutine(GlowFx(matchRight[rightK].GetComponent<RectTransform>(), false, Theme.Green));
        if (!byHint) yield break;

        yield return new WaitForSeconds(Theme.HintLineTime);
        foreach (var seg in path.segments) if (seg != null) seg.color = Theme.Green;
    }

    /// <summary>Erro: linha vermelha até a metade, treme e some; os dois cartões tremem.</summary>
    IEnumerator MatchWrongFx(int leftK, int rightK)
    {
        StartCoroutine(ShakeFx(matchLeft[leftK].GetComponent<RectTransform>()));
        StartCoroutine(ShakeFx(matchRight[rightK].GetComponent<RectTransform>()));
        var path = NewPath(matchLinesRoot, new[] { matchLeftPin[leftK], matchRightPin[rightK] },
            Theme.MatchLineThickness, Theme.Red, false);
        yield return GrowPath(path, 0, 0.5f, Theme.CableTime / 2);
        StartCoroutine(ShakeFx(path.root));
        yield return GrowPath(path, 0.5f, 0, Theme.ShakeTime);
        if (path.root != null) Destroy(path.root.gameObject);
    }

    /// <summary>Fim do match com acerto: todas as linhas piscam juntas uma vez.</summary>
    IEnumerator MatchBlinkFx()
    {
        for (float t = 0; t < Theme.BlinkAllTime; t += Time.deltaTime)
        {
            float a = 1 - Mathf.Sin(t / Theme.BlinkAllTime * Mathf.PI) * 0.8f;
            foreach (var path in matchPaths)
                foreach (var seg in path.segments)
                    if (seg != null) seg.color = Theme.Alpha(Theme.Green, a);
            yield return null;
        }
        foreach (var path in matchPaths)
            foreach (var seg in path.segments)
                if (seg != null) seg.color = Theme.Green;
    }
}

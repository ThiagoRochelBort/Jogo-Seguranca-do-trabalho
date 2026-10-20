using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Cria a interface inteira por código, para você não precisar montar a cena na mão.
/// Cores, tamanhos e medidas vêm da classe Theme.
/// </summary>
public static class UIFactory
{
    static Font font;
    public static Font Font
    {
        get
        {
            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                if (font == null)
                {
                    try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
                }
            }
            return font;
        }
    }

    public static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = Theme.ReferenceResolution;
        scaler.matchWidthOrHeight = Theme.CanvasMatch;

        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
        return canvas;
    }

    /// <summary>Painel que ocupa todo o pai.</summary>
    public static RectTransform Panel(Transform parent, string name, Color color)
    {
        var rt = Rect(parent, name, Vector2.zero, Vector2.one);
        rt.gameObject.AddComponent<Image>().color = color;
        return rt;
    }

    /// <summary>Deixa uma Image com cantos arredondados (usa o sprite do Theme).</summary>
    public static Image Round(Image img)
    {
        img.sprite = Theme.Rounded;
        img.type = Image.Type.Sliced;
        return img;
    }

    /// <summary>Retângulo posicionado por âncoras (0 a 1).</summary>
    public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    /// <summary>Faixa fina colada na borda de cima (top = true) ou de baixo do pai. Fica fora do layout.</summary>
    public static Image Stripe(Transform parent, bool top, float height, Color color)
    {
        float y = top ? 1 : 0;
        var rt = Rect(parent, "Faixa", new Vector2(0, y), new Vector2(1, y));
        rt.pivot = new Vector2(0.5f, y);
        rt.sizeDelta = new Vector2(0, height);
        rt.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    /// <summary>Área rolável; retorna o "content" onde os elementos devem ser adicionados.</summary>
    public static RectTransform ScrollArea(Transform parent, Vector2 anchorMin, Vector2 anchorMax, out ScrollRect scroll)
    {
        var viewport = Rect(parent, "Area", anchorMin, anchorMax);
        viewport.gameObject.AddComponent<Image>().color = Theme.Clear;
        viewport.gameObject.AddComponent<RectMask2D>();
        scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = Theme.ScrollSensitivity;

        var content = new GameObject("Conteudo", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = Theme.ContentSpacing;
        int pad = Theme.ContentPadding;
        layout.padding = new RectOffset(pad, pad, pad, pad);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        return content;
    }

    public static Text Label(Transform parent, string text, int size,
        FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.MiddleCenter, Color? color = null)
    {
        var go = new GameObject("Texto", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = Font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color ?? Theme.Text;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    /// <summary>Faz o texto encolher até Theme.FontMin quando não couber na área dele.</summary>
    public static void ShrinkToFit(Text t)
    {
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = Theme.FontMin;
        t.resizeTextMaxSize = t.fontSize;
        t.verticalOverflow = VerticalWrapMode.Truncate;
    }

    public static void Spacer(Transform parent, float height)
    {
        var go = new GameObject("Espaco", typeof(RectTransform), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
    }

    public static RectTransform Row(Transform parent, float height)
    {
        var go = new GameObject("Linha", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = Theme.RowSpacing;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        var le = go.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
        return go.GetComponent<RectTransform>();
    }

    public static Button MakeButton(Transform parent, string label, UnityAction onClick, ButtonStyle style,
        float height = Theme.ButtonHeight, float width = -1, int fontSize = Theme.FontButton)
    {
        var go = new GameObject("Botao", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);

        var img = Round(go.GetComponent<Image>());
        img.color = Theme.TintBase;

        var le = go.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
        if (width > 0)
        {
            le.preferredWidth = width;
            le.minWidth = width;
        }

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        if (onClick != null) btn.onClick.AddListener(onClick);

        // Borda (some quando o estilo não tem borda)
        var borderRt = Rect(go.transform, "Borda", Vector2.zero, Vector2.one);
        var border = borderRt.gameObject.AddComponent<Image>();
        border.sprite = Theme.RoundedFrame;
        border.type = Image.Type.Sliced;
        border.raycastTarget = false;

        var txtGo = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(go.transform, false);
        var rt = txtGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(Theme.ButtonTextPadding, Theme.ButtonTextPaddingY);
        rt.offsetMax = new Vector2(-Theme.ButtonTextPadding, -Theme.ButtonTextPaddingY);

        var t = txtGo.GetComponent<Text>();
        t.font = Font;
        t.text = label;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.raycastTarget = false;
        ShrinkToFit(t);

        var look = go.AddComponent<ButtonLook>();
        look.label = t;
        look.border = border;
        look.style = style;
        look.Apply();
        return btn;
    }

    /// <summary>Deixa um botão do MakeButton redondo (fundo e borda em círculo).</summary>
    public static void MakeCircle(Button btn)
    {
        var img = btn.GetComponent<Image>();
        img.sprite = Theme.Circle;
        img.type = Image.Type.Simple;
        var look = btn.GetComponent<ButtonLook>();
        if (look != null && look.border != null)
        {
            look.border.sprite = Theme.CircleFrame;
            look.border.type = Image.Type.Simple;
        }
    }

    /// <summary>Troca o estilo de um botão criado pelo MakeButton.</summary>
    public static void SetStyle(Button btn, ButtonStyle style)
    {
        var look = btn.GetComponent<ButtonLook>();
        if (look == null) return;
        look.style = style;
        look.Apply();
    }

    /// <summary>Liga ou desliga o botão e acerta a cor do texto e da borda.</summary>
    public static void SetInteractable(Button btn, bool on)
    {
        btn.interactable = on;
        var look = btn.GetComponent<ButtonLook>();
        if (look != null) look.Apply();
    }

    public static void Clear(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var child = t.GetChild(i).gameObject;
            child.SetActive(false);
            Object.Destroy(child);
        }
    }
}

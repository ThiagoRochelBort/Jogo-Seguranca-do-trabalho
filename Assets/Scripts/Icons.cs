using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Os 34 ícones da "Lista fixa de ícones" do GDD, desenhados só com formas de Unity UI
/// (retângulo, retângulo arredondado, círculo, anel, triângulo, linha e texto) e cores do Theme.
/// Cada ícone é desenhado num quadrado de 96 (centro = 0,0; y para cima) e reduzido por escala.
/// Icons.Make devolve null se o nome não existir: quem chama mostra só o texto.
/// </summary>
public static class Icons
{
    delegate void Painter(Pen p);

    static readonly Dictionary<string, Painter> painters = new Dictionary<string, Painter>
    {
        { "capacete", Capacete }, { "oculos", Oculos }, { "luva", Luva }, { "calcado", Calcado },
        { "protetor_auricular", ProtetorAuricular }, { "respirador", Respirador }, { "cinturao", Cinturao },
        { "mascara_solda", MascaraSolda }, { "colete", Colete }, { "extintor", Extintor }, { "cadeado", Cadeado },
        { "etiqueta", Etiqueta }, { "disjuntor", Disjuntor }, { "plugue", Plugue }, { "tomada", Tomada },
        { "fio", Fio }, { "alerta", Alerta }, { "chama", Chama }, { "gota", Gota }, { "escada", Escada },
        { "maquina", Maquina }, { "painel", Painel }, { "placa_saida", PlacaSaida }, { "cruz", Cruz },
        { "documento", Documento }, { "som", Som }, { "pessoa", Pessoa }, { "tanque", Tanque }, { "gas", Gas },
        { "multimetro", Multimetro }, { "cone", Cone }, { "botao_emergencia", BotaoEmergencia }, { "lupa", Lupa },
        { "ferramenta", Ferramenta },
    };

    static string Key(string name)
    {
        return (name ?? "").Trim().ToLowerInvariant();
    }

    public static bool Has(string name)
    {
        return painters.ContainsKey(Key(name));
    }

    /// <summary>Desenha o ícone centralizado no pai, com o tamanho pedido. Nome desconhecido ou vazio = null.</summary>
    public static RectTransform Make(Transform parent, string name, float size)
    {
        Painter painter;
        if (!painters.TryGetValue(Key(name), out painter)) return null;

        var center = new Vector2(0.5f, 0.5f);
        var root = UIFactory.Rect(parent, "Icone_" + Key(name), center, center);
        root.sizeDelta = new Vector2(Theme.IconBase, Theme.IconBase);
        root.localScale = Vector3.one * (size / Theme.IconBase);
        painter(new Pen(root));
        return root;
    }

    /// <summary>Lista de nomes com segurança: índice fora da lista ou lista nula = null.</summary>
    public static string At(string[] names, int i)
    {
        return names != null && i >= 0 && i < names.Length ? names[i] : null;
    }

    // ------------------------------------------------------------ Formas

    /// <summary>Desenha formas dentro do quadrado de 96 do ícone (ou de qualquer área com centro 0,0).</summary>
    public class Pen
    {
        readonly RectTransform root;
        readonly bool outline;

        public Pen(RectTransform root, bool outline = true)
        {
            this.root = root;
            this.outline = outline;
        }

        Image Shape(float x, float y, float w, float h, Color color, float rot, Sprite sprite, Image.Type type)
        {
            var center = new Vector2(0.5f, 0.5f);
            var rt = UIFactory.Rect(root, "Forma", center, center);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            rt.localRotation = Quaternion.Euler(0, 0, rot);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = type;
            img.color = color;
            img.raycastTarget = false;
            if (outline)
            {
                var o = rt.gameObject.AddComponent<Outline>();
                o.effectColor = Theme.Background;
                o.effectDistance = new Vector2(Theme.IconOutline, -Theme.IconOutline);
            }
            return img;
        }

        /// <summary>Retângulo de cantos retos.</summary>
        public Image Rect(float x, float y, float w, float h, Color color, float rot = 0)
        {
            return Shape(x, y, w, h, color, rot, null, Image.Type.Simple);
        }

        /// <summary>Retângulo de cantos arredondados.</summary>
        public Image Box(float x, float y, float w, float h, Color color, float rot = 0)
        {
            return Shape(x, y, w, h, color, rot, Theme.Rounded, Image.Type.Sliced);
        }

        public Image Disc(float x, float y, float d, Color color)
        {
            return Shape(x, y, d, d, color, 0, Theme.Circle, Image.Type.Simple);
        }

        public Image Ring(float x, float y, float d, Color color)
        {
            return Shape(x, y, d, d, color, 0, Theme.IconRing, Image.Type.Simple);
        }

        /// <summary>Metade de cima de um círculo (rot gira a metade: -90 = metade da direita).</summary>
        public Image Semi(float x, float y, float d, Color color, float rot = 0, bool ring = false)
        {
            var img = Shape(x, y, d, d, color, rot, ring ? Theme.IconRing : Theme.Circle, Image.Type.Filled);
            img.fillMethod = Image.FillMethod.Vertical;
            img.fillOrigin = (int)Image.OriginVertical.Top;
            img.fillAmount = 0.5f;
            return img;
        }

        /// <summary>Triângulo apontando para cima (rot gira).</summary>
        public Image Tri(float x, float y, float w, float h, Color color, float rot = 0)
        {
            return Shape(x, y, w, h, color, rot, Theme.Triangle, Image.Type.Simple);
        }

        /// <summary>Linha reta de (x1,y1) até (x2,y2).</summary>
        public Image Line(float x1, float y1, float x2, float y2, float thick, Color color)
        {
            var d = new Vector2(x2 - x1, y2 - y1);
            return Shape((x1 + x2) / 2, (y1 + y2) / 2, d.magnitude, thick, color,
                Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, null, Image.Type.Simple);
        }

        public Text Label(float x, float y, string text, int size, Color color)
        {
            var t = UIFactory.Label(root, text, size, FontStyle.Bold, TextAnchor.MiddleCenter, color);
            var rt = t.rectTransform;
            var center = new Vector2(0.5f, 0.5f);
            rt.anchorMin = center;
            rt.anchorMax = center;
            rt.sizeDelta = new Vector2(Theme.IconBase, Theme.IconBase);
            rt.anchoredPosition = new Vector2(x, y);
            return t;
        }
    }

    // ------------------------------------------------------------ Ícones (quadrado de 96, centro 0,0)

    static void Capacete(Pen p)
    {
        p.Semi(0, -6, 76, Theme.Yellow);
        p.Rect(0, -8, 90, 8, Theme.Yellow);
        p.Rect(0, 12, 10, 30, Theme.Alpha(Theme.Background, 0.35f));
    }

    static void Oculos(Pen p)
    {
        p.Disc(-21, 0, 32, Theme.Alpha(Theme.Text, 0.25f));
        p.Disc(21, 0, 32, Theme.Alpha(Theme.Text, 0.25f));
        p.Ring(-21, 0, 36, Theme.Text);
        p.Ring(21, 0, 36, Theme.Text);
        p.Line(-4, 2, 4, 2, 4, Theme.Text);
        p.Line(-39, 4, -46, 12, 4, Theme.Text);
        p.Line(39, 4, 46, 12, 4, Theme.Text);
    }

    static void Luva(Pen p)
    {
        for (int k = 0; k < 4; k++) p.Box(-15 + k * 10, 22, 9, 26, Theme.Yellow);
        p.Box(-27, -2, 11, 26, Theme.Yellow, 30);
        p.Box(0, -6, 44, 42, Theme.Yellow);
        p.Rect(0, -34, 48, 14, Theme.SurfaceHigh);
    }

    static void Calcado(Pen p)
    {
        p.Box(-10, 8, 40, 56, Theme.CardFace);
        p.Box(6, -18, 66, 26, Theme.CardFace);
        p.Rect(6, -34, 70, 8, Theme.Background);
    }

    static void ProtetorAuricular(Pen p)
    {
        p.Semi(0, -4, 66, Theme.Text, 0, true);
        p.Disc(-31, -8, 26, Theme.Yellow);
        p.Disc(31, -8, 26, Theme.Yellow);
    }

    static void Respirador(Pen p)
    {
        p.Box(0, 4, 62, 46, Theme.CardFace);
        p.Disc(-30, -16, 28, Theme.Yellow);
        p.Disc(30, -16, 28, Theme.Yellow);
    }

    static void Cinturao(Pen p)
    {
        p.Rect(0, 0, 14, 92, Theme.Yellow, 35);
        p.Rect(0, 0, 14, 92, Theme.Yellow, -35);
        p.Ring(0, 0, 28, Theme.Text);
    }

    static void MascaraSolda(Pen p)
    {
        p.Box(0, 0, 70, 86, Theme.SurfaceHigh);
        p.Rect(0, 12, 50, 16, Theme.Yellow);
    }

    static void Colete(Pen p)
    {
        p.Box(-17, 0, 30, 74, Theme.Yellow);
        p.Box(17, 0, 30, 74, Theme.Yellow);
        p.Rect(0, 8, 70, 6, Theme.Text);
        p.Rect(0, -14, 70, 6, Theme.Text);
    }

    static void Extintor(Pen p)
    {
        p.Box(-4, -10, 38, 70, Theme.Red);
        p.Rect(-4, 30, 10, 12, Theme.Text);
        p.Rect(8, 36, 26, 6, Theme.Text);
        p.Line(20, 36, 28, 8, 4, Theme.Text);
    }

    static void Cadeado(Pen p)
    {
        p.Semi(0, 14, 42, Theme.Text, 0, true);
        p.Rect(-17, 6, 7, 18, Theme.Text);
        p.Rect(17, 6, 7, 18, Theme.Text);
        p.Box(0, -16, 58, 46, Theme.Yellow);
        p.Disc(0, -14, 12, Theme.Background);
    }

    static void Etiqueta(Pen p)
    {
        p.Box(0, -2, 58, 80, Theme.Yellow);
        p.Disc(0, 28, 12, Theme.Background);
        p.Tri(0, -10, 32, 28, Theme.Background);
    }

    static void Disjuntor(Pen p)
    {
        p.Box(0, 0, 58, 82, Theme.Surface);
        p.Rect(0, 14, 16, 28, Theme.Text);
        p.Disc(0, -26, 16, Theme.Green);
    }

    static void Plugue(Pen p)
    {
        p.Box(0, -12, 52, 52, Theme.CardFace);
        p.Rect(-12, 26, 8, 24, Theme.TextMuted);
        p.Rect(12, 26, 8, 24, Theme.TextMuted);
    }

    static void Tomada(Pen p)
    {
        p.Box(0, 0, 72, 72, Theme.CardFace);
        p.Rect(-14, 6, 8, 20, Theme.Background);
        p.Rect(14, 6, 8, 20, Theme.Background);
        p.Disc(0, -18, 12, Theme.Background);
    }

    static void Fio(Pen p)
    {
        p.Line(-42, -20, -14, -20, 10, Theme.Yellow);
        p.Line(-14, -20, 14, 20, 10, Theme.Yellow);
        p.Line(14, 20, 42, 20, 10, Theme.Yellow);
    }

    static void Alerta(Pen p)
    {
        p.Tri(0, 0, 86, 76, Theme.Yellow);
        p.Label(0, -8, "!", 44, Theme.Background);
    }

    static void Chama(Pen p)
    {
        p.Disc(0, -14, 58, Theme.Red);
        p.Tri(0, 20, 46, 52, Theme.Red);
        p.Disc(0, -20, 30, Theme.Yellow);
        p.Tri(0, -2, 22, 28, Theme.Yellow);
    }

    static void Gota(Pen p)
    {
        var c = Theme.Alpha(Theme.Text, 0.6f);
        p.Disc(0, -4, 48, c);
        p.Tri(0, 26, 32, 34, c);
        p.Box(0, -38, 72, 10, c);
    }

    static void Escada(Pen p)
    {
        p.Rect(-20, 0, 8, 92, Theme.CardFace);
        p.Rect(20, 0, 8, 92, Theme.CardFace);
        for (int k = 0; k < 4; k++) p.Rect(0, -30 + k * 20, 40, 6, Theme.CardFace);
    }

    static void Maquina(Pen p)
    {
        p.Box(0, -6, 84, 66, Theme.SurfaceHigh);
        for (int k = 0; k < 6; k++)
        {
            float a = k * 60 * Mathf.Deg2Rad;
            p.Rect(-12 + Mathf.Cos(a) * 18, -6 + Mathf.Sin(a) * 18, 10, 10, Theme.Yellow, k * 60);
        }
        p.Disc(-12, -6, 30, Theme.Yellow);
        p.Disc(-12, -6, 10, Theme.SurfaceHigh);
        p.Disc(26, 16, 14, Theme.Green);
    }

    static void Painel(Pen p)
    {
        p.Box(0, 0, 78, 88, Theme.Surface);
        p.Disc(-22, 24, 14, Theme.Green);
        p.Disc(0, 24, 14, Theme.Yellow);
        p.Disc(22, 24, 14, Theme.Red);
        p.Box(-14, -16, 22, 16, Theme.SurfaceHigh);
        p.Box(14, -16, 22, 16, Theme.SurfaceHigh);
    }

    static void PlacaSaida(Pen p)
    {
        p.Box(0, 0, 88, 58, Theme.Green);
        p.Rect(-8, 0, 38, 10, Theme.Text);
        p.Tri(20, 0, 24, 24, Theme.Text, -90);
    }

    static void Cruz(Pen p)
    {
        p.Disc(0, 0, 82, Theme.CardFace);
        p.Rect(0, 0, 18, 54, Theme.Green);
        p.Rect(0, 0, 54, 18, Theme.Green);
    }

    static void Documento(Pen p)
    {
        p.Rect(0, 0, 62, 82, Theme.CardFace);
        for (int k = 0; k < 3; k++) p.Rect(-4, 14 - k * 16, 40, 5, Theme.TextMuted);
        p.Tri(24, 34, 16, 16, Theme.TextMuted, 45);
    }

    static void Som(Pen p)
    {
        p.Disc(0, 0, 22, Theme.Text);
        for (int k = 0; k < 3; k++)
        {
            float d = 40 + k * 20;
            p.Semi(0, 0, d, Theme.Yellow, -90, true);
            p.Semi(0, 0, d, Theme.Yellow, 90, true);
        }
    }

    static void Pessoa(Pen p)
    {
        p.Box(0, -18, 50, 54, Theme.Yellow);
        p.Disc(0, 26, 34, Theme.CardFace);
    }

    static void Tanque(Pen p)
    {
        p.Box(0, -4, 62, 84, Theme.SurfaceHigh);
        p.Rect(0, 40, 46, 6, Theme.TextMuted);
        p.Disc(0, 22, 22, Theme.Background);
    }

    static void Gas(Pen p)
    {
        var c = Theme.Alpha(Theme.TextMuted, 0.7f);
        p.Disc(-18, -6, 42, c);
        p.Disc(16, -4, 46, c);
        p.Disc(0, 14, 42, c);
    }

    static void Multimetro(Pen p)
    {
        p.Line(-12, -30, -26, -46, 4, Theme.Text);
        p.Line(12, -30, 26, -46, 4, Theme.Text);
        p.Box(0, 4, 58, 76, Theme.Yellow);
        p.Rect(0, 18, 42, 24, Theme.Background);
        p.Label(0, 18, "0 V", 18, Theme.Green);
    }

    static void Cone(Pen p)
    {
        p.Tri(0, 4, 58, 76, Theme.Yellow);
        p.Rect(0, -40, 78, 10, Theme.Yellow);
        p.Rect(0, 2, 30, 10, Theme.Background);
    }

    static void BotaoEmergencia(Pen p)
    {
        p.Box(0, -16, 78, 40, Theme.Yellow);
        p.Disc(0, 8, 58, Theme.Red);
    }

    static void Lupa(Pen p)
    {
        p.Rect(20, -22, 12, 36, Theme.Text, 45);
        p.Ring(-8, 8, 54, Theme.Text);
    }

    static void Ferramenta(Pen p)
    {
        p.Rect(0, -10, 10, 66, Theme.TextMuted);
        p.Rect(0, 26, 52, 14, Theme.CardFace);
    }
}

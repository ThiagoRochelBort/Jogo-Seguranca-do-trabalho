using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animações curtas das cenas desenhadas ("Regras visuais comuns" do GDD): brilho de acerto,
/// tremida de erro, faíscas, pulso e linhas/cabos que esticam. Tudo por coroutine; Clear() para tudo.
/// </summary>
public partial class GameUI
{
    // Posição de descanso de quem está tremendo (para duas tremidas seguidas não deslocarem o objeto)
    readonly Dictionary<RectTransform, Vector2> shakeHome = new Dictionary<RectTransform, Vector2>();

    /// <summary>Peça solta (forma) posicionada pelo centro dentro de uma área de centro 0,0.</summary>
    static RectTransform Piece(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var center = new Vector2(0.5f, 0.5f);
        var rt = UIFactory.Rect(parent, name, center, center);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image PieceImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color, Sprite sprite, bool sliced)
    {
        var img = Piece(parent, name, pos, size).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    /// <summary>Acerto: contorno verde que cresce de 0,9 para 1,1, volta a 1,0 e some (até 0,4 s).</summary>
    IEnumerator GlowFx(RectTransform target, bool round, Color color)
    {
        if (target == null) yield break;
        var size = Vector2.Scale(target.rect.size, (Vector2)target.localScale) + Vector2.one * (2 * Theme.GlowWidth);
        var rt = UIFactory.Rect(target.parent, "Brilho", target.anchorMin, target.anchorMax);
        rt.pivot = target.pivot;
        rt.anchoredPosition = target.anchoredPosition;
        rt.sizeDelta = target.anchorMin == target.anchorMax ? size : target.sizeDelta + Vector2.one * (2 * Theme.GlowWidth);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = round ? Theme.IconRing : Theme.GlowFrame;
        img.type = round ? Image.Type.Simple : Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;

        for (float t = 0; t < Theme.GlowTime; t += Time.deltaTime)
        {
            float k = t / Theme.GlowTime; // 0,9 -> 1,1 -> 1,0
            float s = k < 0.5f ? Mathf.Lerp(0.9f, 1.1f, k * 2) : Mathf.Lerp(1.1f, 1f, (k - 0.5f) * 2);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;
        for (float t = 0; t < Theme.GlowFade; t += Time.deltaTime)
        {
            img.color = Theme.Alpha(color, 1 - t / Theme.GlowFade);
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    /// <summary>Erro: tremida horizontal de ±ShakeDistance, 3 idas e vindas.</summary>
    IEnumerator ShakeFx(RectTransform target)
    {
        if (target == null) yield break;
        Vector2 home;
        if (!shakeHome.TryGetValue(target, out home))
        {
            home = target.anchoredPosition;
            shakeHome[target] = home;
        }
        for (float t = 0; t < Theme.ShakeTime; t += Time.deltaTime)
        {
            if (target == null) yield break;
            float x = Mathf.Sin(t / Theme.ShakeTime * 3 * 2 * Mathf.PI) * Theme.ShakeDistance;
            target.anchoredPosition = home + new Vector2(x, 0);
            yield return null;
        }
        if (target != null) target.anchoredPosition = home;
    }

    /// <summary>Faíscas: quadradinhos que voam para fora do ponto e somem.</summary>
    IEnumerator SparksFx(Transform parent, Vector2 pos, Color color, int count, float size, float distance)
    {
        var sparks = new List<Image>();
        var dirs = new List<Vector2>();
        for (int k = 0; k < count; k++)
        {
            float a = (45 + k * 360f / count) * Mathf.Deg2Rad;
            dirs.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
            sparks.Add(PieceImage(parent, "Faisca", pos, new Vector2(size, size), color, null, false));
        }
        for (float t = 0; t < Theme.SparkTime; t += Time.deltaTime)
        {
            float k = t / Theme.SparkTime;
            for (int i = 0; i < sparks.Count; i++)
            {
                if (sparks[i] == null) continue;
                sparks[i].rectTransform.anchoredPosition = pos + dirs[i] * distance * k;
                sparks[i].color = Theme.Alpha(color, 1 - k);
            }
            yield return null;
        }
        foreach (var s in sparks) if (s != null) Destroy(s.gameObject);
    }

    /// <summary>Dica: pulso de escala 1,0 -> PulseScale -> 1,0, 'times' vezes em 'total' segundos.</summary>
    IEnumerator PulseFx(RectTransform target, int times, float total)
    {
        float each = total / times;
        for (int n = 0; n < times; n++)
        {
            for (float t = 0; t < each; t += Time.deltaTime)
            {
                if (target == null) yield break;
                target.localScale = Vector3.one * Mathf.Lerp(1, Theme.PulseScale, Mathf.Sin(t / each * Mathf.PI));
                yield return null;
            }
        }
        if (target != null) target.localScale = Vector3.one;
    }

    // ------------------------------------------------------------ Linhas com vários trechos

    /// <summary>Um caminho (lista de pontos) desenhado com retângulos finos, que pode aparecer só até uma fração.</summary>
    class Path
    {
        public RectTransform root;
        public readonly List<Image> segments = new List<Image>();
        public Vector2[] points;
    }

    Path NewPath(Transform parent, Vector2[] points, float thick, Color color, bool outline)
    {
        var path = new Path();
        path.root = UIFactory.Rect(parent, "Linha", Vector2.zero, Vector2.one);
        path.points = points;
        for (int i = 0; i < points.Length - 1; i++)
        {
            var img = PieceImage(path.root, "Trecho", Vector2.zero, new Vector2(0, thick), color, null, false);
            if (outline)
            {
                var o = img.gameObject.AddComponent<Outline>();
                o.effectColor = Theme.Text;
                o.effectDistance = new Vector2(Theme.IconOutline, -Theme.IconOutline);
            }
            path.segments.Add(img);
        }
        SetPath(path, 0);
        return path;
    }

    /// <summary>Mostra o caminho do começo até a fração 'progress' (0 a 1) do comprimento total.</summary>
    static void SetPath(Path path, float progress)
    {
        float total = 0;
        for (int i = 0; i < path.points.Length - 1; i++) total += Vector2.Distance(path.points[i], path.points[i + 1]);
        float left = total * Mathf.Clamp01(progress);

        for (int i = 0; i < path.segments.Count; i++)
        {
            var seg = path.segments[i];
            Vector2 a = path.points[i];
            Vector2 b = path.points[i + 1];
            float len = Vector2.Distance(a, b);
            float shown = Mathf.Clamp(left, 0, len);
            left -= len;
            seg.enabled = shown > 0;
            if (shown <= 0) continue;

            Vector2 dir = len > 0 ? (b - a) / len : Vector2.right;
            Vector2 end = a + dir * shown;
            var rt = seg.rectTransform;
            rt.anchoredPosition = (a + end) / 2;
            rt.sizeDelta = new Vector2(shown + seg.rectTransform.sizeDelta.y * 0.5f, rt.sizeDelta.y); // meia espessura cobre a emenda
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
    }

    IEnumerator GrowPath(Path path, float from, float to, float time)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (path.root == null) yield break;
            SetPath(path, Mathf.Lerp(from, to, t / time));
            yield return null;
        }
        if (path.root != null) SetPath(path, to);
    }

    /// <summary>Listras diagonais ao longo do caminho (fio verde-amarelo).</summary>
    static void StripePath(Path path, float thick, Color color)
    {
        for (int i = 0; i < path.points.Length - 1; i++)
        {
            Vector2 a = path.points[i];
            Vector2 b = path.points[i + 1];
            float len = Vector2.Distance(a, b);
            if (len <= 0) continue;
            Vector2 dir = (b - a) / len;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 45;
            for (float d = Theme.CableStripeEvery / 2; d < len; d += Theme.CableStripeEvery)
            {
                var img = PieceImage(path.root, "Listra", a + dir * d, new Vector2(thick * 0.5f, thick), color, null, false);
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    /// <summary>Ponto de brilho que corre pelo caminho (acerto do match).</summary>
    IEnumerator RunnerFx(Path path, Color color, float size)
    {
        var dot = PieceImage(path.root, "Brilho", path.points[0], new Vector2(size, size), color, Theme.Circle, false);
        for (float t = 0; t < Theme.CableTime; t += Time.deltaTime)
        {
            if (dot == null) yield break;
            dot.rectTransform.anchoredPosition = Vector2.Lerp(path.points[0], path.points[path.points.Length - 1], t / Theme.CableTime);
            yield return null;
        }
        if (dot != null) Destroy(dot.gameObject);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Aplica um ButtonStyle ao botão (cores de cada estado, cor do texto e borda)
/// e encolhe um pouco o botão enquanto ele está pressionado.
/// O UIFactory adiciona este componente sozinho; não precisa colocar na mão.
/// </summary>
public class ButtonLook : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
    IPointerEnterHandler, IPointerClickHandler
{
    public ButtonStyle style;
    public Text label;
    public Image border;
    Button button;
    bool pressedWhileActive; // o clique pode desativar o botão antes deste script tocar o som

    Button Button
    {
        get
        {
            if (button == null) button = GetComponent<Button>();
            return button;
        }
    }

    public void Apply()
    {
        if (style == null || Button == null) return;

        var cb = Button.colors;
        cb.normalColor = style.normal;
        cb.highlightedColor = style.hover;
        cb.selectedColor = style.normal;
        cb.pressedColor = style.pressed;
        cb.disabledColor = style.disabled;
        cb.colorMultiplier = 1;
        cb.fadeDuration = Theme.ButtonFade;
        Button.colors = cb;

        bool on = Button.interactable;
        if (label != null) label.color = on ? style.text : style.disabledText;
        if (border != null)
        {
            Color bc = on ? style.border : style.disabledBorder;
            border.color = bc;
            border.enabled = bc.a > 0;
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        pressedWhileActive = Button != null && Button.interactable;
        if (style != null && style.pressScale && pressedWhileActive)
            transform.localScale = Vector3.one * Theme.PressedScale;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (pressedWhileActive && (style == null || style.clickSound)) SoundManager.Play(Sfx.Click);
        pressedWhileActive = false;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (Button != null && Button.interactable) SoundManager.Play(Sfx.Hover);
    }

    public void OnPointerUp(PointerEventData e) { Release(); }

    public void OnPointerExit(PointerEventData e) { Release(); }

    void Release()
    {
        if (style != null && style.pressScale) transform.localScale = Vector3.one;
    }
}

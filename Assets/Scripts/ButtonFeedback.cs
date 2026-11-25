using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Put this next to a Button. Pressing shrinks it, releasing springs it back.
// UI sounds come from the shared UIAudio player - this button has no AudioSource.
public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
                              IPointerExitHandler, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] float pressedScale = 0.93f;
    [SerializeField] bool playHoverSound = true;
    [SerializeField] bool playPressSound = true;

    Selectable selectable;
    NavButton navButton;
    Coroutine running;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
        navButton = GetComponent<NavButton>();
    }

    void OnDisable() { transform.localScale = Vector3.one; running = null; }

    public void OnPointerEnter(PointerEventData e) => Hover();
    public void OnSelect(BaseEventData e) => Hover();          // keyboard / gamepad focus

    public void OnPointerDown(PointerEventData e)
    {
        if (selectable != null && !selectable.interactable) return;
        Press();
        ScaleTo(pressedScale, 0.08f, Ease.OutCubic);   // quick squash = "I felt that"
    }

    public void OnPointerUp(PointerEventData e) { ScaleTo(1f, 0.28f, Ease.OutBack); }     // bouncy release
    public void OnPointerExit(PointerEventData e) { ScaleTo(1f, 0.15f, Ease.OutCubic); }  // finger slid away

    void Hover()
    {
        if (!playHoverSound || !isActiveAndEnabled) return;
        if (selectable != null && !selectable.interactable) return;
        UIAudio.PlayHover();
    }

    void Press()
    {
        if (!playPressSound) return;
        if (navButton != null && navButton.IsBack) UIAudio.PlayBack();
        else UIAudio.PlayClick();
    }

    void ScaleTo(float target, float duration, Ease ease)
    {
        if (!isActiveAndEnabled) return;
        if (running != null) StopCoroutine(running);
        float start = transform.localScale.x;
        running = StartCoroutine(Tween.Run(duration, ease, p =>
        {
            float s = Mathf.LerpUnclamped(start, target, p);
            transform.localScale = new Vector3(s, s, 1f);
        }));
    }
}

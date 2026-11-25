using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Transition { Push, Fade, Modal }

// The brain of the menu flow. Only this script shows, hides and moves screens.
// It remembers where you came from, so Back always works.
public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    // Other scripts (like the pause controller) can listen to this.
    public static event Action<UIScreen> ScreenChanged;

    [SerializeField] RectTransform root;          // the ScreenRoot object (used to measure the screen size)
    [SerializeField] UIScreen startScreen;        // the Title screen
    [SerializeField] List<UIScreen> allScreens = new List<UIScreen>();

    // One remembered step: "I was on 'from' and got here using 'how'".
    struct Step { public UIScreen from; public Transition how; }
    readonly Stack<Step> history = new Stack<Step>();

    UIScreen current;
    bool busy;
    public UIScreen Current => current;

    void Awake() { Instance = this; }

    void Start()
    {
        // Hide everything, then show only the start screen.
        foreach (var s in allScreens) { s.ResetVisual(); s.gameObject.SetActive(false); }
        current = startScreen;
        current.gameObject.SetActive(true);
        current.SetInteractable(true);
        current.NotifyShown();
        ScreenChanged?.Invoke(current);
    }

    void Update()
    {
        // Esc / Android Back / gamepad East, through the new Input System.
        if (BackInput.WasPressedThisFrame() && current != null && !current.ignoreBackKey) Back();
    }

    // Go to a new screen and remember where we were.
    public void Show(UIScreen next, Transition how)
    {
        if (busy || next == null || next == current) return;
        history.Push(new Step { from = current, how = how });
        StartCoroutine(Go(current, next, how, false));
    }

    // Go back one step, playing the same animation in reverse.
    public void Back()
    {
        if (busy || history.Count == 0) return;
        Step step = history.Pop();
        StartCoroutine(Go(current, step.from, step.how, true));
    }

    // Jump to the hub and forget everything before it.
    public void GoHome(UIScreen home, Transition how)
    {
        if (busy || home == null || home == current) return;
        history.Clear();
        StartCoroutine(Go(current, home, how, false));
    }

    // Jump back to a screen we visited earlier (for example Results -> Level select).
    public void Rewind(UIScreen target, Transition how)
    {
        if (busy || target == null || target == current) return;
        if (!history.Any(s => s.from == target)) { GoHome(target, how); return; }
        while (history.Count > 0 && history.Pop().from != target) { }
        StartCoroutine(Go(current, target, how, true));
    }

    IEnumerator Go(UIScreen from, UIScreen to, Transition how, bool reverse)
    {
        busy = true;
        bool modalForward = how == Transition.Modal && !reverse;
        bool modalBack = how == Transition.Modal && reverse;

        if (modalForward) UIAudio.PlayOpen();
        if (modalBack) UIAudio.PlayClose();

        float w = root.rect.width;
        float h = root.rect.height;

        from.SetInteractable(false);
        to.SetInteractable(false);

        if (!modalBack)
        {
            to.gameObject.SetActive(true);
            // Draw order: the screen that moves in should be on top.
            if (reverse && how == Transition.Push) from.transform.SetAsLastSibling();
            else to.transform.SetAsLastSibling();
        }

        // Pick the animation length and curve, and place the incoming screen at its start.
        float duration;
        Ease ease;
        switch (how)
        {
            case Transition.Push:
                duration = reverse ? 0.34f : 0.38f;
                ease = Ease.OutExpo;
                if (reverse) { to.Rect.anchoredPosition = new Vector2(-w * 0.28f, 0); to.Group.alpha = 0.6f; }
                else { to.Rect.anchoredPosition = new Vector2(w, 0); }
                break;
            case Transition.Fade:
                duration = 0.42f;
                ease = Ease.OutBack;
                to.Rect.localScale = Vector3.one * 0.9f;
                to.Group.alpha = 0f;
                break;
            default: // Modal (pause pop-up)
                if (reverse) { duration = 0.28f; ease = Ease.InCubic; }
                else
                {
                    duration = 0.42f; ease = Ease.OutBack;
                    to.Rect.anchoredPosition = new Vector2(0, -h);
                    to.Group.alpha = 0f;
                }
                break;
        }

        yield return Tween.Run(duration, ease, p => Apply(from, to, how, reverse, p, w, h));

        // Tidy up: hide every screen except the new one (and, for a pop-up, the screen under it).
        foreach (var s in allScreens)
        {
            bool keep = s == to || (modalForward && s == from);
            if (!keep) { s.gameObject.SetActive(false); s.ResetVisual(); }
        }
        to.ResetVisual();
        to.SetInteractable(true);
        current = to;
        busy = false;
        to.NotifyShown();
        ScreenChanged?.Invoke(to);
    }

    // Moves both screens for one frame of the animation. p is the eased progress.
    void Apply(UIScreen from, UIScreen to, Transition how, bool reverse, float p, float w, float h)
    {
        float a = Mathf.Clamp01(p);
        switch (how)
        {
            case Transition.Push:
                if (!reverse)
                {
                    to.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(w, 0, p), 0);
                    from.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(0, -w * 0.28f, p), 0);
                    from.Group.alpha = Mathf.Lerp(1f, 0.6f, a);
                }
                else
                {
                    from.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(0, w, p), 0);
                    to.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-w * 0.28f, 0, p), 0);
                    to.Group.alpha = Mathf.Lerp(0.6f, 1f, a);
                }
                break;
            case Transition.Fade:
                to.Rect.localScale = Vector3.one * Mathf.LerpUnclamped(0.9f, 1f, p);
                to.Group.alpha = a;
                from.Rect.localScale = Vector3.one * Mathf.Lerp(1f, 1.06f, a);
                from.Group.alpha = 1f - Mathf.Clamp01(a * 1.6f);
                break;
            case Transition.Modal:
                if (!reverse)
                {
                    to.Rect.anchoredPosition = new Vector2(0, Mathf.LerpUnclamped(-h, 0, p));
                    to.Group.alpha = Mathf.Clamp01(a * 2f);
                }
                else
                {
                    from.Rect.anchoredPosition = new Vector2(0, Mathf.Lerp(0, -h, a));
                    from.Group.alpha = 1f - a;
                }
                break;
        }
    }
}
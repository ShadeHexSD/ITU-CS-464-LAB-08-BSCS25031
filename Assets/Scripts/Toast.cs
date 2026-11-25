using System.Collections;
using TMPro;
using UnityEngine;

// A short message such as "Purchased!" or "Not enough gems".
// Any script can call Toast.Instance.Show("...").
public class Toast : MonoBehaviour
{
    public static Toast Instance { get; private set; }

    [SerializeField] RectTransform box;     // the visible rounded box
    [SerializeField] CanvasGroup group;     // lets us fade it
    [SerializeField] TMP_Text label;
    [SerializeField] float hiddenY = -160f; // where it waits, below the screen edge
    [SerializeField] float shownY = 280f;   // where it rests while visible

    Coroutine running;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        group.alpha = 0f;
        box.anchoredPosition = new Vector2(0f, hiddenY);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    /// <param name="isError">Plays the error cue instead of the success cue.</param>
    public void Show(string message, bool isError = false)
    {
        label.text = message;
        if (running != null) StopCoroutine(running);   // a new message replaces the old one

        if (isError) UIAudio.PlayError();
        else UIAudio.PlaySuccess();

        running = StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        // Rise with a little bounce.
        yield return Tween.Run(0.30f, Ease.OutBack, p =>
        {
            box.anchoredPosition = new Vector2(0f, Mathf.LerpUnclamped(hiddenY, shownY, p));
            group.alpha = Mathf.Clamp01(p * 2f);
        });
        yield return new WaitForSecondsRealtime(1.4f);   // stay readable
        // Leave quickly.
        yield return Tween.Run(0.25f, Ease.InCubic, p =>
        {
            box.anchoredPosition = new Vector2(0f, Mathf.Lerp(shownY, hiddenY, p));
            group.alpha = 1f - p;
        });
    }
}

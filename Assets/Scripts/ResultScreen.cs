using System.Collections;
using UnityEngine;

// Plays the reward animation every time Results opens.
// NOTE: the class name must match this file name so Unity can attach it.
public class ResultScreen : MonoBehaviour
{
    [SerializeField] UIScreen screen;               // the Screen_Results object itself
    [SerializeField] RectTransform title;           // the "Victory!" text
    [SerializeField] RectTransform[] earnedStars;   // only the stars the player earned
    [SerializeField] CanvasGroup rewards;           // the reward pills]

    readonly System.Collections.Generic.Dictionary<RectTransform, Vector3> starSizes =
    new System.Collections.Generic.Dictionary<RectTransform, Vector3>();

    void Awake()
    {
        screen.Shown += Play;
        foreach (var star in earnedStars) starSizes[star] = star.localScale;
    }
    void OnDestroy() { screen.Shown -= Play; }

    void Play()
    {
        StopAllCoroutines();
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // Start everything hidden.
        title.localScale = Vector3.zero;
        foreach (var star in earnedStars) star.localScale = Vector3.zero;
        rewards.alpha = 0f;

        // 1) Title pops.
        yield return Tween.Run(0.45f, Ease.OutBack, p => title.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, p));

        // 2) Stars pop one after another, 0.22 s apart.
        foreach (var star in earnedStars)
        {
            StartCoroutine(Pop(star));
            yield return new WaitForSecondsRealtime(0.22f);
        }

        // 3) Rewards fade in last.
        yield return Tween.Run(0.30f, Ease.OutCubic, p => rewards.alpha = p);
    }

    IEnumerator Pop(RectTransform star)
    {
        Vector3 fullSize = starSizes[star];
        yield return Tween.Run(0.45f, Ease.OutBack, p => star.localScale = fullSize * Mathf.LerpUnclamped(0f, 1f, p));
    }
}
using TMPro;
using UnityEngine;

// A fake "match": just a countdown, so you can see that Pause really stops time.
// It is now an event SOURCE: it announces GameStarted / GameLost instead of
// directly calling the HUD, music or sound scripts.
public class GameplayStub : MonoBehaviour
{
    [SerializeField] TMP_Text timerLabel;
    [SerializeField] float startSeconds = 134f;
    float remaining;
    bool ended;

    void OnEnable() { ResetTimer(); }

    public void ResetTimer()
    {
        remaining = startSeconds;
        ended = false;
        Show();
        GameEvents.RaiseGameStarted();
    }

    void Update()
    {
        // Time.deltaTime is 0 while paused, so the clock freezes by itself.
        if (ended) return;
        remaining = Mathf.Max(0f, remaining - Time.deltaTime);
        Show();

        if (remaining <= 0f)
        {
            ended = true;
            GameEvents.RaiseGameLost();
        }
    }

    void Show()
    {
        int s = Mathf.CeilToInt(remaining);
        timerLabel.text = (s / 60) + ":" + (s % 60).ToString("00");
    }
}

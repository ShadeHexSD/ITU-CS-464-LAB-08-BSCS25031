using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The real match's core gameplay system. It owns the clock and win/lose decision, but it only
/// RAISES C# events. HUD, music, sounds, camera shake and results are all independent listeners.
/// </summary>
public class Lab08MatchController : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] TMP_Text timerLabel;
    [SerializeField] TMP_Text scoreLabel;
    [SerializeField] float matchSeconds = 75f;
    [SerializeField] int targetCoins = 8;

    float remaining;
    int coins;
    bool finished;

    void OnEnable()
    {
        GameEvents.CoinCollected += OnCoin;
        GameEvents.PlayerDied += OnDied;
    }

    void OnDisable()
    {
        GameEvents.CoinCollected -= OnCoin;
        GameEvents.PlayerDied -= OnDied;
    }

    void Start()
    {
        remaining = matchSeconds;
        coins = 0;
        finished = false;
        health?.Restore(1f);
        Show();
        GameEvents.RaiseGameStarted();
    }

    void Update()
    {
        if (finished) return;
        remaining = Mathf.Max(0f, remaining - Time.deltaTime);
        Show();
        if (remaining <= 0f) Finish(false);
    }

    void OnCoin(int value)
    {
        if (finished || value <= 0) return;
        coins += value;
        Show();
        if (coins >= targetCoins) Finish(true);
    }

    void OnDied() => Finish(false);

    void Finish(bool won)
    {
        if (finished) return;
        finished = true;
        if (won) GameEvents.RaiseGameWon();
        else GameEvents.RaiseGameLost();
    }

    void Show()
    {
        int seconds = Mathf.CeilToInt(remaining);
        if (timerLabel != null) timerLabel.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");
        if (scoreLabel != null) scoreLabel.text = coins + " / " + targetCoins;
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

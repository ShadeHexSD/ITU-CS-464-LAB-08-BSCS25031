using TMPro;
using UnityEngine;

/// <summary>
/// Listener for GameEvents.CoinCollected. Adds the value to the shared Wallet and
/// optionally updates a run-score label. Subscribes/unsubscribes correctly so a
/// replay never leaves a dead static subscriber behind.
/// </summary>
public class CoinCollector : MonoBehaviour
{
    [SerializeField] TMP_Text scoreLabel;
    int score;

    void OnEnable()
    {
        GameEvents.CoinCollected += Collect;
        GameEvents.GameStarted += ResetScore;
    }

    void OnDisable()
    {
        GameEvents.CoinCollected -= Collect;
        GameEvents.GameStarted -= ResetScore;
    }

    void ResetScore()
    {
        score = 0;
        if (scoreLabel != null) scoreLabel.text = "0";
    }

    void Collect(int value)
    {
        if (value <= 0) return;
        score += value;
        Wallet.AddCoins(value);                 // CurrencyLabel bounces through Wallet.Changed
        if (scoreLabel != null) scoreLabel.text = score.ToString();
    }
}

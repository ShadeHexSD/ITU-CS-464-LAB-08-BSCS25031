using System;
using UnityEngine;

/// <summary>
/// Events with no particular sender, so the HUD, music and effects can hear the game without a
/// reference to it. Static events outlive every scene: whatever subscribes MUST unsubscribe in
/// OnDisable, or a scene reload leaves a dead subscriber and the next raise throws
/// MissingReferenceException.
/// </summary>
public static class GameEvents
{
    public static event Action GameStarted;
    public static event Action GameWon;
    public static event Action GameLost;

    public static event Action<int, int> PlayerDamaged;   // current, max
    public static event Action<int, int> PlayerHealed;    // current, max
    public static event Action PlayerDied;
    public static event Action<int> CoinCollected;        // value

    // Only this class can invoke its events, so the rest of the game raises them through these.
    public static void RaiseGameStarted() => GameStarted?.Invoke();
    public static void RaiseGameWon() => GameWon?.Invoke();
    public static void RaiseGameLost() => GameLost?.Invoke();
    public static void RaisePlayerDamaged(int current, int max) => PlayerDamaged?.Invoke(current, max);
    public static void RaisePlayerHealed(int current, int max) => PlayerHealed?.Invoke(current, max);
    public static void RaisePlayerDied() => PlayerDied?.Invoke();
    public static void RaiseCoinCollected(int value) => CoinCollected?.Invoke(value);

    // If "Enter Play Mode Options > Reload Domain" is off, statics survive between Play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetAll()
    {
        GameStarted = null;
        GameWon = null;
        GameLost = null;
        PlayerDamaged = null;
        PlayerHealed = null;
        PlayerDied = null;
        CoinCollected = null;
    }
}

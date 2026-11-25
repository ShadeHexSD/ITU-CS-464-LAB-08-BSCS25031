using System;
using UnityEngine;

/// <summary>
/// Tiny persistent unlock store for the demo Chrome Skin. This gives the Shop's
/// Reward.Skin option a real effect instead of silently spending gems.
/// </summary>
public static class SkinStore
{
    const string ChromeKey = "Skin_Chrome_Unlocked";
    public static event Action Changed;

    public static bool HasChrome => PlayerPrefs.GetInt(ChromeKey, 0) == 1;

    public static void UnlockChrome()
    {
        if (HasChrome) return;
        PlayerPrefs.SetInt(ChromeKey, 1);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Changed = null;
}

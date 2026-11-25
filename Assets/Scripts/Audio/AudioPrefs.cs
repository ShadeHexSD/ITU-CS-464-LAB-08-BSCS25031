using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Owns every audio PlayerPrefs key and the slider -> decibel maths, so keys can't be mistyped
/// and nobody assigns a linear 0-1 value to the mixer.
/// (Named AudioPrefs, not AudioSettings, because UnityEngine.AudioSettings already exists.)
///
/// Each mixer group's Volume must be EXPOSED in the Audio Mixer and named exactly as the
/// constants below.
/// </summary>
public static class AudioPrefs
{
    // Exposed parameter names on the Audio Mixer. Also used as the PlayerPrefs keys.
    public const string Master = "MasterVolume";
    public const string Music  = "MusicVolume";
    public const string Sfx    = "SfxVolume";
    public const string Ui     = "UiVolume";

    public static readonly string[] All = { Master, Music, Sfx, Ui };

    public const float MinLinear = 0.0001f;   // slider minimum; Log10(0) is -infinity
    public const float SilentDb  = -80f;

    /// <summary>Set by AudioBootstrap. Sliders and toggles go through this.</summary>
    public static AudioMixer Mixer { get; set; }

    static string MuteKey(string parameter) => parameter + "_Muted";

    // ---- maths ----------------------------------------------------------------------------

    /// <summary>0.0001..1 -> -80..0 dB. 1 = 0 dB, 0.5 = -6 dB.</summary>
    public static float ToDecibels(float linear) =>
        Mathf.Log10(Mathf.Clamp(linear, MinLinear, 1f)) * 20f;

    // ---- saved values ---------------------------------------------------------------------

    public static float GetVolume(string parameter) =>
        Mathf.Clamp(PlayerPrefs.GetFloat(parameter, 1f), MinLinear, 1f);   // default: full volume

    public static bool IsMuted(string parameter) =>
        PlayerPrefs.GetInt(MuteKey(parameter), 0) == 1;

    /// <summary>Stores the slider value and applies it. The mute flag is untouched.</summary>
    public static void SetVolume(string parameter, float linear)
    {
        linear = Mathf.Clamp(linear, MinLinear, 1f);
        PlayerPrefs.SetFloat(parameter, linear);
        Apply(parameter);
    }

    /// <summary>
    /// Mute never changes the stored volume, so un-muting restores the old level.
    /// </summary>
    public static void SetMuted(string parameter, bool muted)
    {
        PlayerPrefs.SetInt(MuteKey(parameter), muted ? 1 : 0);
        Apply(parameter);
    }

    public static void Save() => PlayerPrefs.Save();

    // ---- applying to the mixer ------------------------------------------------------------

    public static void Apply(string parameter)
    {
        if (Mixer == null) return;
        float db = IsMuted(parameter) ? SilentDb : ToDecibels(GetVolume(parameter));
        if (!Mixer.SetFloat(parameter, db))
            Debug.LogWarning($"AudioPrefs: mixer has no exposed parameter named '{parameter}'.");
    }

    public static void ApplyAll()
    {
        foreach (string parameter in All) Apply(parameter);
    }
}

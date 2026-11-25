using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Every gameplay sound effect goes through here: PlayOneShot (so short effects overlap instead
/// of cutting each other off) plus a small random pitch and volume on every call.
/// Routed to the mixer's SFX group. Pauses with the game (no ignoreListenerPause).
///
/// Because this object outlives scenes and pickups, a pickup can play its sound and destroy
/// itself in the same frame without the sound being cut off.
/// </summary>
public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField, Min(1)] int voices = 6;
    [SerializeField] float pitchRange = 2f;                       // +/- semitones
    [SerializeField, Range(0f, 1f)] float minVolume = 0.85f;

    AudioSource[] sources;
    int next;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxGroup == null) Debug.LogWarning("SfxPlayer: assign the mixer's SFX group.", this);

        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            var child = new GameObject("SfxSource" + i);
            child.transform.SetParent(transform, false);
            AudioSource s = child.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.outputAudioMixerGroup = sfxGroup;
            sources[i] = s;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <param name="vary">false for stingers (win, lose) that should sound identical every time.</param>
    public void Play(AudioClip clip, float volume = 1f, bool vary = true)
    {
        if (clip == null) return;

        AudioSource source = sources[next];
        next = (next + 1) % sources.Length;

        float semitones = vary ? Random.Range(-pitchRange, pitchRange) : 0f;
        source.pitch = Mathf.Pow(2f, semitones / 12f);
        source.PlayOneShot(clip, volume * (vary ? Random.Range(minVolume, 1f) : 1f));
    }
}

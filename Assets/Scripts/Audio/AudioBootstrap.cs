using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Holds the one Audio Mixer, applies the saved volumes at launch (not only when the settings
/// screen opens), and owns the pause behaviour for audio.
///
/// Put it on a ROOT GameObject (e.g. "AudioSystem") in the first scene. Copies in other scenes
/// destroy themselves, so any scene can still be tested on its own.
/// </summary>
[DefaultExecutionOrder(-100)]
public class AudioBootstrap : MonoBehaviour
{
    public static AudioBootstrap Instance { get; private set; }

    [SerializeField] AudioMixer mixer;

    [Header("Optional pause snapshots")]
    [Tooltip("Do NOT let a snapshot control an exposed volume: SetFloat overrides it. Use snapshots for " +
             "things like a low-pass on Music or Sfx.")]
    [SerializeField] AudioMixerSnapshot normalSnapshot;
    [SerializeField] AudioMixerSnapshot pausedSnapshot;
    [SerializeField] float snapshotSeconds = 0.15f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        AudioListener.pause = false;
        AudioPrefs.Mixer = mixer;
        AudioPrefs.ApplyAll();
    }

    // Applied again in Start: setting mixer parameters in Awake can be overridden by the
    // mixer's own initialisation in some Unity versions.
    void Start() => AudioPrefs.ApplyAll();

    void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        AudioPrefs.Mixer = null;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) AudioPrefs.Save();   // mobile: the app may be killed in the background
    }

    /// <summary>
    /// Call from the pause menu. AudioListener.pause silences every AudioSource EXCEPT those with
    /// ignoreListenerPause (UI sounds and music, set in UIAudio and MusicPlayer).
    /// </summary>
    public static void SetPaused(bool paused)
    {
        AudioListener.pause = paused;

        AudioBootstrap self = Instance;
        if (self == null) return;
        AudioMixerSnapshot snapshot = paused ? self.pausedSnapshot : self.normalSnapshot;
        if (snapshot != null) snapshot.TransitionTo(self.snapshotSeconds);
    }
}

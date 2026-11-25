using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// The one reusable UI sound player. Buttons never get their own AudioSource: they call
/// UIAudio.PlayClick() and friends. Everything is routed to the mixer's UI group and keeps
/// playing while the game is paused (ignoreListenerPause).
/// </summary>
public class UIAudio : MonoBehaviour
{
    public static UIAudio Instance { get; private set; }

    [SerializeField] AudioMixerGroup uiGroup;
    [SerializeField, Min(1)] int voices = 4;

    [Header("Clips (leave empty to skip)")]
    [SerializeField] AudioClip hoverClip;
    [SerializeField] AudioClip clickClip;
    [SerializeField] AudioClip backClip;
    [SerializeField] AudioClip openClip;
    [SerializeField] AudioClip closeClip;
    [SerializeField] AudioClip pauseClip;
    [SerializeField] AudioClip resumeClip;
    [SerializeField] AudioClip successClip;
    [SerializeField] AudioClip errorClip;

    [Header("Feel")]
    [SerializeField, Range(0f, 3f)] float pitchRange = 0.5f;     // +/- semitones
    [SerializeField, Range(0f, 1f)] float minVolume = 0.9f;
    [SerializeField] float hoverCooldown = 0.05f;                // stops hover spam when sweeping

    AudioSource[] sources;
    int next;
    float lastHover = -10f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (uiGroup == null) Debug.LogWarning("UIAudio: assign the mixer's UI group.", this);

        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            var child = new GameObject("UISource" + i);
            child.transform.SetParent(transform, false);
            AudioSource s = child.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;                    // UI is always 2D
            s.outputAudioMixerGroup = uiGroup;
            s.ignoreListenerPause = true;           // UI keeps working in the pause menu
            sources[i] = s;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---- static one-liners: UIAudio.PlayClick() -------------------------------------------

    public static void PlayHover()
    {
        if (Instance == null) return;
        if (Time.unscaledTime - Instance.lastHover < Instance.hoverCooldown) return;
        Instance.lastHover = Time.unscaledTime;
        Instance.Play(Instance.hoverClip);
    }

    public static void PlayClick()   { if (Instance != null) Instance.Play(Instance.clickClip); }
    public static void PlayBack()    { if (Instance != null) Instance.Play(Instance.backClip); }
    public static void PlayOpen()    { if (Instance != null) Instance.Play(Instance.openClip); }
    public static void PlayClose()   { if (Instance != null) Instance.Play(Instance.closeClip); }
    public static void PlayPause()   { if (Instance != null) Instance.Play(Instance.pauseClip); }
    public static void PlayResume()  { if (Instance != null) Instance.Play(Instance.resumeClip); }
    public static void PlaySuccess() { if (Instance != null) Instance.Play(Instance.successClip); }
    public static void PlayError()   { if (Instance != null) Instance.Play(Instance.errorClip); }

    // ---- internals ------------------------------------------------------------------------

    void Play(AudioClip clip)
    {
        if (clip == null) return;
        AudioSource source = sources[next];
        next = (next + 1) % sources.Length;     // pitch belongs to the source, so rotate sources
        source.pitch = Mathf.Pow(2f, Random.Range(-pitchRange, pitchRange) / 12f);
        source.PlayOneShot(clip, Random.Range(minVolume, 1f));
    }
}

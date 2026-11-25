using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Looping background music with an equal-power crossfade between tracks.
/// Persists across scene loads (singleton check included), so music carries on through the
/// loading screen without doubling up. Music keeps playing while the game is paused.
///
/// The user's music volume is the mixer's job; this component's own 'volume' is just the
/// source level (leave it at 1).
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [SerializeField] AudioMixerGroup musicGroup;
    [SerializeField] AudioClip startClip;
    [SerializeField] float fadeSeconds = 1.5f;
    [SerializeField, Range(0f, 1f)] float volume = 1f;

    AudioSource a, b, current;
    Coroutine fade;

    public AudioClip CurrentClip => current != null ? current.clip : null;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicGroup == null) Debug.LogWarning("MusicPlayer: assign the mixer's Music group.", this);

        a = MakeSource("MusicA");
        b = MakeSource("MusicB");
        current = a;

        if (startClip != null) PlayImmediate(startClip);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    AudioSource MakeSource(string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform, false);
        AudioSource s = child.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = true;
        s.spatialBlend = 0f;                   // 2D: same volume wherever the camera is
        s.priority = 0;                        // music is the last thing Unity drops
        s.outputAudioMixerGroup = musicGroup;
        s.ignoreListenerPause = true;          // music continues under the pause menu
        return s;
    }

    /// <summary>Start a track straight away, no fade.</summary>
    public void PlayImmediate(AudioClip clip)
    {
        if (clip == null) return;
        StopFade();
        a.Stop();
        b.Stop();
        current = a;
        current.clip = clip;
        current.loop = true;
        current.volume = volume;
        current.Play();
    }

    /// <summary>Fade the current track out and a looping track in. No-op if it is already playing.</summary>
    public void CrossfadeTo(AudioClip clip) => CrossfadeTo(clip, true);

    /// <summary>
    /// Fade to a track. Use loop:false for short win/lose stingers - every source is otherwise
    /// looping, which made result sounds repeat forever.
    /// </summary>
    public void CrossfadeTo(AudioClip clip, bool loop)
    {
        if (clip == null) return;
        if (current.clip == clip && current.isPlaying && current.loop == loop) return;

        // If a fade is still running, finish it abruptly rather than juggling three tracks.
        if (fade != null)
        {
            StopCoroutine(fade);
            fade = null;
            (current == a ? b : a).Stop();
            current.volume = volume;
        }

        AudioSource from = current;
        AudioSource to = from == a ? b : a;
        to.clip = clip;
        to.loop = loop;
        to.volume = 0f;
        to.Play();
        current = to;

        fade = StartCoroutine(Fade(from, to));
    }

    void StopFade()
    {
        if (fade == null) return;
        StopCoroutine(fade);
        fade = null;
    }

    // Equal power: cosine out, sine in, so total power stays constant and there is no dip in
    // the middle. Unscaled time so it also works while paused.
    IEnumerator Fade(AudioSource from, AudioSource to)
    {
        float start = from.volume;
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            float u = t / fadeSeconds;
            from.volume = start * Mathf.Cos(u * Mathf.PI / 2f);
            to.volume = volume * Mathf.Sin(u * Mathf.PI / 2f);
            yield return null;
        }
        from.Stop();
        to.volume = volume;
        fade = null;
    }
}

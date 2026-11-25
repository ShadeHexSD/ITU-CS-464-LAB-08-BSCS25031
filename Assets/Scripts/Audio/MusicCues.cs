using UnityEngine;

/// <summary>
/// Event-driven music changes: gameplay -> results, calm -> combat, etc.
/// Listens to GameEvents; nothing in gameplay code needs to know the music exists.
/// Put one in the gameplay scene.
/// </summary>
public class MusicCues : MonoBehaviour
{
    [SerializeField] AudioClip gameplayClip;   // on GameStarted
    [SerializeField] AudioClip winClip;        // on GameWon
    [SerializeField] AudioClip loseClip;       // on GameLost

    void OnEnable()
    {
        GameEvents.GameStarted += OnStarted;
        GameEvents.GameWon += OnWon;
        GameEvents.GameLost += OnLost;
    }

    void OnDisable()
    {
        GameEvents.GameStarted -= OnStarted;
        GameEvents.GameWon -= OnWon;
        GameEvents.GameLost -= OnLost;
    }

    // The gameplay scene must sound like gameplay as soon as it loads. Do not wait for
    // GameStarted: that is only raised by the real match controller (added in Task 09), and
    // a scene with only MusicCues would otherwise be silent. CrossfadeTo ignores a clip that
    // is already playing, so the later event is harmless.
    void Start() => Crossfade(gameplayClip);

    void OnStarted() => Crossfade(gameplayClip);
    void OnWon()     => Crossfade(winClip);
    void OnLost()    => Crossfade(loseClip);

    static void Crossfade(AudioClip clip)
    {
        if (MusicPlayer.Instance != null && clip != null)
            MusicPlayer.Instance.CrossfadeTo(clip);
    }
}

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

    void OnStarted() => Crossfade(gameplayClip);
    void OnWon()     => Crossfade(winClip);
    void OnLost()    => Crossfade(loseClip);

    static void Crossfade(AudioClip clip)
    {
        if (MusicPlayer.Instance != null && clip != null)
            MusicPlayer.Instance.CrossfadeTo(clip);
    }
}

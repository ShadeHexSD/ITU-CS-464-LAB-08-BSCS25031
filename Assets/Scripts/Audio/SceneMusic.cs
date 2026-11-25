using UnityEngine;

/// <summary>
/// Put one in a scene (menu, gameplay...) to crossfade the music to this scene's track on load.
/// </summary>
public class SceneMusic : MonoBehaviour
{
    [SerializeField] AudioClip clip;

    void Start()
    {
        if (MusicPlayer.Instance != null && clip != null)
            MusicPlayer.Instance.CrossfadeTo(clip);
    }
}

using UnityEngine;

/// <summary>
/// Listener for SkinStore.Changed: swaps the player's material once the skin is owned.
/// Put it on the player (or a child renderer holder) in the gameplay scene.
/// </summary>
public class SkinApplier : MonoBehaviour
{
    [SerializeField] Renderer target;
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material chromeMaterial;

    void OnEnable()
    {
        SkinStore.Changed += Apply;
        Apply();
    }

    void OnDisable() => SkinStore.Changed -= Apply;

    void Apply()
    {
        if (target == null || defaultMaterial == null) return;
        target.sharedMaterial = SkinStore.HasChrome && chromeMaterial != null ? chromeMaterial : defaultMaterial;
    }
}

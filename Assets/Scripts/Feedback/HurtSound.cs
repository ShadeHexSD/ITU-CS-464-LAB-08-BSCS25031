using UnityEngine;

/// <summary>
/// Listener: plays the hurt sound on Damaged and the death sound on Died, through SfxPlayer.
/// </summary>
public class HurtSound : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] AudioClip hurtClip;
    [SerializeField] AudioClip deathClip;

    void OnEnable()
    {
        if (health == null) return;
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health == null) return;
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
    }

    void OnDamaged(int current, int max)
    {
        if (current <= 0) return;                     // the killing blow plays the death sound instead
        if (SfxPlayer.Instance != null) SfxPlayer.Instance.Play(hurtClip);
    }

    void OnDied()
    {
        if (SfxPlayer.Instance != null) SfxPlayer.Instance.Play(deathClip, 1f, false);
    }
}

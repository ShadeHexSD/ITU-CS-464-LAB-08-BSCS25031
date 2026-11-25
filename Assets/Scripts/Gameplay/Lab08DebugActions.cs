using UnityEngine;

/// <summary>Buttons-only helper for QA: no keyboard code, just Inspector-wired methods.</summary>
public class Lab08DebugActions : MonoBehaviour
{
    [SerializeField] Health health;

    public void Damage() => health?.TakeDamage(1);
    public void Heal() => health?.Heal(1);
    public void Kill()
    {
        // Restore with no grace period so the QA button always lands, even after a recent hit.
        if (health == null) return;
        health.Restore(0f);
        health.TakeDamage(health.Max);
    }
    public void Win() => GameEvents.RaiseGameWon();
    public void Lose() => GameEvents.RaiseGameLost();
}

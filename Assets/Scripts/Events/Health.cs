using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The core gameplay system, refactored to RAISE EVENTS instead of calling other scripts by name.
/// Health raises Damaged / Healed / Died once and knows nothing about who is listening
/// (UI bar, sound, sparks, camera shake, hit flash, music...).
///
/// C# events for code-driven listeners; a UnityEvent (onDied) for designer wiring in the Inspector.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] int max = 5;
    [SerializeField] float iFrames = 1f;                 // invulnerability after a hit
    [Tooltip("Also raise the static GameEvents (for listeners that live in other scenes, like the HUD).")]
    [SerializeField] bool raiseGlobalEvents = true;

    public event Action<int, int> Damaged;               // current, max
    public event Action<int, int> Healed;                // current, max
    public event Action Died;
    public UnityEvent onDied = new UnityEvent();         // Inspector-authored wiring

    int current;
    bool initialised;
    float invulnerableUntil;

    public int Current { get { Init(); return current; } }
    public int Max => max;
    public bool IsDead => Current <= 0;

    // Lazy, because a listener's OnEnable can run before this component's Awake.
    void Init()
    {
        if (initialised) return;
        initialised = true;
        current = max;
    }

    void Awake() => Init();

    /// <returns>true if the hit landed.</returns>
    public bool TakeDamage(int amount)
    {
        Init();
        if (amount <= 0 || current <= 0 || Time.time < invulnerableUntil) return false;

        invulnerableUntil = Time.time + iFrames;
        current = Mathf.Max(0, current - amount);

        Damaged?.Invoke(current, max);
        if (raiseGlobalEvents) GameEvents.RaisePlayerDamaged(current, max);

        if (current <= 0)
        {
            Died?.Invoke();
            onDied.Invoke();
            if (raiseGlobalEvents) GameEvents.RaisePlayerDied();
        }
        return true;
    }

    public void Heal(int amount)
    {
        Init();
        if (amount <= 0 || current <= 0 || current >= max) return;

        current = Mathf.Min(max, current + amount);
        Healed?.Invoke(current, max);
        if (raiseGlobalEvents) GameEvents.RaisePlayerHealed(current, max);
    }

    /// <summary>Full health again (replay / respawn), with a short grace period.</summary>
    public void Restore(float protectionSeconds = 1f)
    {
        Init();
        current = max;
        invulnerableUntil = Time.time + protectionSeconds;
        Healed?.Invoke(current, max);
        if (raiseGlobalEvents) GameEvents.RaisePlayerHealed(current, max);
    }
}

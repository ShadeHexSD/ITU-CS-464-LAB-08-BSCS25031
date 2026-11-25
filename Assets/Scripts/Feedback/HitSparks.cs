using UnityEngine;

/// <summary>
/// Listener: a burst of sparks on every hit. Particle System settings: Looping off, Play On Awake
/// off, no rate, one burst at time 0, Stop Action None (the system is reused).
/// </summary>
public class HitSparks : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] ParticleSystem sparks;

    void OnEnable()
    {
        if (health != null) health.Damaged += Burst;
    }

    void OnDisable()
    {
        if (health != null) health.Damaged -= Burst;
    }

    void Burst(int current, int max)
    {
        if (sparks != null) sparks.Play();
    }
}

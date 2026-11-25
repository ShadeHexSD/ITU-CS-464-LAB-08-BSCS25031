using UnityEngine;

/// <summary>
/// Listener: adds camera trauma whenever the Health takes damage.
/// </summary>
public class ShakeOnHit : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] CameraShake shake;
    [SerializeField, Range(0f, 1f)] float trauma = 0.55f;

    void OnEnable()
    {
        if (health != null) health.Damaged += OnDamaged;
    }

    void OnDisable()
    {
        if (health != null) health.Damaged -= OnDamaged;
    }

    void OnDamaged(int current, int max)
    {
        if (shake != null) shake.AddTrauma(trauma);
    }
}

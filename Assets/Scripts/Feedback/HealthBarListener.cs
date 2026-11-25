using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Listener: keeps a health bar (an Image with Type = Filled) in step with a Health component.
/// The fill eases towards the target, so a hit reads as a drop rather than a jump.
/// </summary>
public class HealthBarListener : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Image fill;                        // Image Type: Filled
    [SerializeField] TMP_Text label;                    // optional, "3 / 5"
    [SerializeField] float easeSpeed = 2f;              // fill fraction per second

    float target = 1f;

    void OnEnable()
    {
        if (health == null) return;
        health.Damaged += OnChanged;
        health.Healed += OnChanged;
    }

    void OnDisable()
    {
        if (health == null) return;
        health.Damaged -= OnChanged;
        health.Healed -= OnChanged;
    }

    void Start()
    {
        if (health == null) return;
        OnChanged(health.Current, health.Max);
        if (fill != null) fill.fillAmount = target;     // no easing on the first frame
    }

    void OnChanged(int current, int max)
    {
        target = max > 0 ? (float)current / max : 0f;
        if (label != null) label.SetText("{0} / {1}", current, max);
    }

    void Update()
    {
        if (fill == null) return;
        fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, easeSpeed * Time.unscaledDeltaTime);
    }
}

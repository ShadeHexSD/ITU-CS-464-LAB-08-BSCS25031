using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Listener: a red flash over the screen when the player is hurt. It lives on the HUD, so it
/// listens to the static GameEvents.PlayerDamaged rather than to one particular Health.
/// </summary>
public class HitFlash : MonoBehaviour
{
    [SerializeField] Image overlay;              // full screen, red, alpha 0, Raycast Target OFF
    [SerializeField] float alpha = 0.45f;
    [SerializeField] float fadeSpeed = 2.5f;     // alpha per second

    void OnEnable() => GameEvents.PlayerDamaged += Flash;
    void OnDisable() => GameEvents.PlayerDamaged -= Flash;   // without it, a reload leaves a dead subscriber

    void Flash(int current, int max) => SetAlpha(alpha);

    void Update()
    {
        if (overlay == null) return;
        SetAlpha(Mathf.MoveTowards(overlay.color.a, 0f, fadeSpeed * Time.unscaledDeltaTime));
    }

    void SetAlpha(float a)
    {
        if (overlay == null) return;
        Color c = overlay.color;
        c.a = a;
        overlay.color = c;
    }
}

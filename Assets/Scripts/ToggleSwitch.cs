using UnityEngine;
using UnityEngine.UI;

// Put on a Button that has a round knob child.
// With muteParameter set, it drives AudioPrefs mute for that mixer bus;
// otherwise it is a plain visual toggle (Music / Effects / Vibration placeholders).
[RequireComponent(typeof(Button))]
public class ToggleSwitch : MonoBehaviour
{
    [SerializeField] RectTransform knob;
    [SerializeField] Image background;
    [SerializeField] bool isOn = true;
    [SerializeField] float travel = 84f;     // how far the knob moves left to right
    [SerializeField] Color onColor = new Color32(0x3D, 0xDC, 0x84, 255);
    [SerializeField] Color offColor = new Color32(0x6B, 0x5B, 0xB0, 255);

    [Tooltip("Exposed mixer parameter to mute, e.g. MasterVolume. Leave empty for a visual-only switch.")]
    [SerializeField] string muteParameter = "";

    public bool IsOn => isOn;
    float position;   // 0 = off, 1 = on (can overshoot while bouncing)

    void Awake()
    {
        if (!string.IsNullOrEmpty(muteParameter))
            isOn = !AudioPrefs.IsMuted(muteParameter);   // On = sound enabled

        GetComponent<Button>().onClick.AddListener(Flip);
        Apply(isOn ? 1f : 0f);
    }

    /// <summary>Wired by VolumeSlider on startup; does not re-apply the mixer twice.</summary>
    public void SetMuteParameter(string parameter, bool muted, bool apply = true)
    {
        muteParameter = parameter;
        isOn = !muted;
        if (apply) Apply(isOn ? 1f : 0f);
    }

    void Flip()
    {
        isOn = !isOn;
        if (!string.IsNullOrEmpty(muteParameter))
        {
            AudioPrefs.SetMuted(muteParameter, !isOn);
            AudioPrefs.Save();
        }

        float start = position;
        float end = isOn ? 1f : 0f;
        StopAllCoroutines();
        StartCoroutine(Tween.Run(0.30f, Ease.OutBack, p => Apply(Mathf.LerpUnclamped(start, end, p))));
    }

    void Apply(float value)
    {
        position = value;
        if (knob != null)
            knob.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-travel / 2f, travel / 2f, value), 0f);
        if (background != null)
            background.color = Color.Lerp(offColor, onColor, Mathf.Clamp01(value));
    }
}

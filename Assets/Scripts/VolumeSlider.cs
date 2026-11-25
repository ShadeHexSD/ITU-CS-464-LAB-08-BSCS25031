using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Put this on a Slider. Shows the value as a percentage; the handle grows
// while held and bounces back on release. All four sliders now drive the
// matching Audio Mixer bus through AudioPrefs (logarithmic dB + PlayerPrefs).
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum AudioBus { Master, Music, Sfx, UI }

    [SerializeField] AudioBus bus = AudioBus.Master;
    [SerializeField] ToggleSwitch muteToggle;   // optional companion mute switch

    [SerializeField] TMP_Text valueLabel;
    [SerializeField] RectTransform handle;

    Slider slider;
    Coroutine running;

    string Parameter
    {
        get
        {
            switch (bus)
            {
                case AudioBus.Master: return AudioPrefs.Master;
                case AudioBus.Music: return AudioPrefs.Music;
                case AudioBus.Sfx: return AudioPrefs.Sfx;
                default: return AudioPrefs.Ui;
            }
        }
    }

    public float Value => slider != null ? slider.value : AudioPrefs.GetVolume(Parameter);

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = AudioPrefs.MinLinear;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        // Load saved value BEFORE subscribing so initialization does not write back.
        slider.value = AudioPrefs.GetVolume(Parameter);
        slider.onValueChanged.AddListener(OnChanged);
        Refresh(slider.value);

        if (muteToggle != null)
            muteToggle.SetMuteParameter(Parameter, AudioPrefs.IsMuted(Parameter), false);
    }

    void OnDestroy()
    {
        if (slider != null) slider.onValueChanged.RemoveListener(OnChanged);
    }

    void OnChanged(float linear)
    {
        AudioPrefs.SetVolume(Parameter, linear);
        Refresh(linear);
    }

    void Refresh(float value)
    {
        int percent = Mathf.RoundToInt(value * 100f);
        if (valueLabel != null) valueLabel.text = percent <= 0 ? "Off" : percent + "%";
    }

    public void OnPointerDown(PointerEventData e) { ScaleHandle(1.25f, 0.15f, Ease.OutCubic); }
    public void OnPointerUp(PointerEventData e)
    {
        ScaleHandle(1f, 0.30f, Ease.OutBack);
        AudioPrefs.Save();                     // persist once the finger lifts
    }

    void ScaleHandle(float target, float duration, Ease ease)
    {
        if (handle == null) return;
        if (running != null) StopCoroutine(running);
        float start = handle.localScale.x;
        running = StartCoroutine(Tween.Run(duration, ease, p =>
        {
            float s = Mathf.LerpUnclamped(start, target, p);
            handle.localScale = new Vector3(s, s, 1f);
        }));
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put on a level card (it must also have a Button).
[RequireComponent(typeof(Button))]
public class LevelCard : MonoBehaviour
{
    [SerializeField] bool locked;
    [Range(0, 3)][SerializeField] int stars = 3;
    [SerializeField] RectTransform[] starIcons;     // the 3 Star objects
    [SerializeField] GameObject lockedLabel;        // the word "Locked"
    [SerializeField] CanvasGroup cardGroup;         // dims the card when locked
    [SerializeField] Shake shake;                   // the Shake script on this card
    [SerializeField] UIScreen nextScreen;           // Loadout
    [SerializeField] Color starOn = new Color32(0xFF, 0xD2, 0x3F, 255);
    [SerializeField] Color starOff = new Color32(0x7D, 0x6A, 0xD0, 255);

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        Refresh();
    }

    void Refresh()
    {
        lockedLabel.SetActive(locked);
        cardGroup.alpha = locked ? 0.55f : 1f;
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].gameObject.SetActive(!locked);
            Color c = i < stars ? starOn : starOff;   // shape stays; colour shows earned or not
            foreach (var img in starIcons[i].GetComponentsInChildren<Image>()) img.color = c;
        }
    }

    void OnClick()
    {
        if (locked)
        {
            shake.Play();
            Toast.Instance?.Show("Earn more stars to unlock", true);   // say how to fix it
            return;
        }
        ScreenManager.Instance.Show(nextScreen, Transition.Push);
    }
}
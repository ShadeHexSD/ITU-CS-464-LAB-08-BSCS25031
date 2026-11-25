using UnityEngine;
using UnityEngine.UI;

// Put this next to a Button. Pick what it does in the Inspector.
[RequireComponent(typeof(Button))]
public class NavButton : MonoBehaviour
{
    public enum Mode
    {
        Forward,   // open 'target' and remember where we came from
        Back,      // go back one step
        Home,      // jump to 'target' (the Main menu) and forget the history
        Rewind     // jump back to 'target', a screen we visited earlier
    }

    [SerializeField] Mode mode = Mode.Forward;
    [SerializeField] UIScreen target;
    [SerializeField] Transition transition = Transition.Push;

    // ButtonFeedback uses this to choose the Back sound instead of the Click sound.
    public bool IsBack => mode == Mode.Back;

    void Awake() { GetComponent<Button>().onClick.AddListener(OnClick); }

    void OnClick()
    {
        var manager = ScreenManager.Instance;
        switch (mode)
        {
            case Mode.Forward: manager?.Show(target, transition); break;
            case Mode.Back: manager?.Back(); break;
            case Mode.Home: manager?.GoHome(target, transition); break;
            case Mode.Rewind: manager?.Rewind(target, transition); break;
        }
    }
}

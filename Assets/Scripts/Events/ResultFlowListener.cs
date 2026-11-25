using UnityEngine;

/// <summary>
/// Turns the outcome events into the Results screen transition and its feedback.
/// Keep it in the gameplay scene and assign the Results UIScreen that lives there.
/// </summary>
public class ResultFlowListener : MonoBehaviour
{
    [SerializeField] UIScreen resultsScreen;
    [SerializeField] AudioClip winClip;
    [SerializeField] AudioClip loseClip;
    [SerializeField] Transition transition = Transition.Fade;

    void OnEnable()
    {
        GameEvents.GameWon += OnWon;
        GameEvents.GameLost += OnLost;
    }

    void OnDisable()
    {
        GameEvents.GameWon -= OnWon;
        GameEvents.GameLost -= OnLost;
    }

    void OnWon() => Finish(winClip, true);
    void OnLost() => Finish(loseClip, false);

    void Finish(AudioClip stinger, bool won)
    {
        SfxPlayer.Instance?.Play(stinger, 1f, false);

        if (won) UIAudio.PlaySuccess();
        else UIAudio.PlayError();

        if (resultsScreen != null && ScreenManager.Instance != null)
            ScreenManager.Instance.Show(resultsScreen, transition);
    }
}

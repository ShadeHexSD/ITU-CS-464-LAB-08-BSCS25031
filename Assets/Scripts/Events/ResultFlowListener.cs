using TMPro;
using UnityEngine;

/// <summary>
/// Turns the outcome events into the Results screen transition and its feedback.
/// Keep it in the gameplay scene and assign the Results UIScreen that lives there.
/// </summary>
public class ResultFlowListener : MonoBehaviour
{
    [SerializeField] UIScreen resultsScreen;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text rewardText;
    [SerializeField] AudioClip winClip;
    [SerializeField] AudioClip loseClip;
    [SerializeField] Transition transition = Transition.Fade;

    bool finished;

    void OnEnable()
    {
        finished = false;
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
        if (finished) return;   // mashing both QA buttons must not stack stingers
        finished = true;

        SfxPlayer.Instance?.Play(stinger, 1f, false);

        if (won) UIAudio.PlaySuccess();
        else UIAudio.PlayError();

        // The Menus results panel is shared by win and lose; retitle it for this outcome.
        if (titleText != null) titleText.text = won ? "Victory!" : "Defeat!";
        if (rewardText != null) rewardText.text = won ? "+12 coins" : "Try again";

        if (resultsScreen != null && ScreenManager.Instance != null)
            ScreenManager.Instance.Show(resultsScreen, transition);
    }
}

using UnityEngine;

/// <summary>
/// Inspector-friendly outcome triggers for "End Match" / test buttons.
/// They only raise events; ResultFlowListener decides what happens next.
/// </summary>
public class GameOutcomeButtons : MonoBehaviour
{
    public void Win() => GameEvents.RaiseGameWon();
    public void Lose() => GameEvents.RaiseGameLost();
}

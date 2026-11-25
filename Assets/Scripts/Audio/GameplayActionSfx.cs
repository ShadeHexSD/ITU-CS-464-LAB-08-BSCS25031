using UnityEngine;

/// <summary>
/// One-shot gameplay cues for actions that are not covered by Health/Pickup yet.
/// The new gameplay builder script can call these from attack / jump / boost /
/// movement code. Every sound goes through SfxPlayer (PlayOneShot + pitch variation),
/// never through an AudioSource on the acting object.
/// </summary>
public class GameplayActionSfx : MonoBehaviour
{
    [SerializeField] AudioClip attackClip;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip boostClip;
    [SerializeField] AudioClip healClip;

    public void PlayAttack() => Play(attackClip);
    public void PlayJump() => Play(jumpClip);
    public void PlayBoost() => Play(boostClip);
    public void PlayHeal() => Play(healClip);

    static void Play(AudioClip clip)
    {
        if (clip != null) SfxPlayer.Instance?.Play(clip);
    }
}

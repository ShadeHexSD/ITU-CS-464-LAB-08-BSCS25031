using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Small, self-contained player controller for the Lab 08 gameplay scene.
/// Uses the NEW Input System only (the project's activeInputHandler is 2).
/// Movement and camera shake stop while the game is paused (timeScale == 0).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Lab08PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 6f;
    [SerializeField] float turnSpeed = 12f;
    [SerializeField] GameplayActionSfx cuePlayer;

    CharacterController controller;

    void Awake() => controller = GetComponent<CharacterController>();

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        Vector2 move = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
            if (keyboard.spaceKey.wasPressedThisFrame) cuePlayer?.PlayJump();
            if (keyboard.jKey.wasPressedThisFrame) cuePlayer?.PlayAttack();
        }

        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            move += pad.leftStick.ReadValue();
            if (pad.buttonSouth.wasPressedThisFrame) cuePlayer?.PlayJump();
            if (pad.buttonWest.wasPressedThisFrame) cuePlayer?.PlayAttack();
        }

        move = Vector2.ClampMagnitude(move, 1f);
        Vector3 direction = new Vector3(move.x, 0f, move.y);

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        float vertical = controller.isGrounded ? -1f : controller.velocity.y + Physics.gravity.y * Time.deltaTime;
        Vector3 velocity = direction * speed;
        velocity.y = vertical;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Attack() => cuePlayer?.PlayAttack();
    public void Jump() => cuePlayer?.PlayJump();
    public void Boost() => cuePlayer?.PlayBoost();
    public void Heal() => cuePlayer?.PlayHeal();
}

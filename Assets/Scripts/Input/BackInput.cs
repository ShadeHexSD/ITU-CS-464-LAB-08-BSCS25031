using UnityEngine.InputSystem;

/// <summary>
/// Single place for "Back / Cancel" input. The project uses the NEW Input System
/// (activeInputHandler = 2), so UnityEngine.Input.GetKeyDown would throw at runtime.
/// On Android the system Back key arrives as Escape; gamepad East is also treated as Back.
/// </summary>
public static class BackInput
{
    public static bool WasPressedThisFrame()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool gamepad = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        return keyboard || gamepad;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace CityCourier
{
    public struct CourierInput
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump, Slide, SlideHeld, Walk, Interact, Recenter;

        public static CourierInput Read()
        {
            var input = new CourierInput();
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;
            if (keyboard != null)
            {
                input.Move = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
                    (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
                input.Jump = keyboard.spaceKey.wasPressedThisFrame;
                input.Slide = keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
                input.SlideHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                input.Walk = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
                input.Interact = keyboard.eKey.wasPressedThisFrame;
            }
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                input.Look = mouse.delta.ReadValue() * 0.12f;
                input.Recenter = mouse.middleButton.wasPressedThisFrame;
            }
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (stick.sqrMagnitude > input.Move.sqrMagnitude) input.Move = stick;
                input.Look += pad.rightStick.ReadValue() * (130f * Time.deltaTime);
                input.Jump |= pad.buttonSouth.wasPressedThisFrame;
                input.Slide |= pad.buttonEast.wasPressedThisFrame;
                input.SlideHeld |= pad.buttonEast.isPressed;
                input.Interact |= pad.buttonWest.wasPressedThisFrame;
                input.Recenter |= pad.buttonNorth.wasPressedThisFrame;
            }
            input.Move = Vector2.ClampMagnitude(input.Move, 1);
            return input;
        }
    }
}

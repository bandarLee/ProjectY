using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class PlayerInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnMove(InputValue value) => move = value.Get<Vector2>();

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                look = value.Get<Vector2>();
            }
        }

        public void OnJump(InputValue value) => jump = value.isPressed;

        private void Update()
        {
            sprint = Keyboard.current.leftShiftKey.isPressed;
        }
#endif
    }
}

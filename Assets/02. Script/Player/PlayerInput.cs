using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class PlayerInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump;
        public bool Sprint;
        public bool Attack; // ← Attack 입력 변수 추가

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

        public void OnMove(InputValue value) => Move = value.Get<Vector2>();

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                Look = value.Get<Vector2>();
            }
        }

        public void OnJump(InputValue value) => Jump = value.isPressed;

        // 공격
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                Attack = true;
                Debug.Log("공격!"); 
            }
        }

        private void Update()
        {
            Sprint = Keyboard.current.leftShiftKey.isPressed;
        }
#endif
    }
}

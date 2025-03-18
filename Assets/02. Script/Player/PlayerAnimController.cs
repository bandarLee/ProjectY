using StarterAssets;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private Animator _animator;
    private PlayerInputs _input;
    private CharacterController _controller;
    private PlayerController _playerController;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _input = GetComponent<PlayerInputs>();
        _controller = GetComponent<CharacterController>();
        _playerController = GetComponent<PlayerController>();

        // PlayerController의 점프 이벤트 구독
        if (_playerController != null)
        {
            _playerController.OnJumpTriggered += HandleJump;
        }
    }

    private void OnDestroy()
    {
        if (_playerController != null)
        {
            _playerController.OnJumpTriggered -= HandleJump;
        }
    }

    private void Update()
    {
        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
        bool isWalking = speed > 0.1f && !_input.sprint;
        bool isRunning = _input.sprint;

        _animator.SetBool("IsWalk", isWalking);
        _animator.SetBool("IsRun", isRunning);

        // 점프 처리는 이벤트에서 처리하므로 여기서는 제거함.
    }

    private void HandleJump()
    {
        _animator.SetTrigger("IsJump");
    }
}

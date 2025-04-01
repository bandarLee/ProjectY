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
        bool isWalking = speed > 0.1f && !_input.Sprint;
        bool isRunning = _input.Sprint;

        _animator.SetBool("IsWalk", isWalking);
        _animator.SetBool("IsRun", isRunning);

        if (_input.Attack)
        {
            _animator.SetTrigger("IsSwordAttack");
            _input.Attack = false;
        }
    }

    private void HandleJump()
    {
        // 수평 속도 계산 (y축 제외)
        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        // speed가 낮으면 Idle 상태로 판단
        if (speed < 0.1f)
        {
            _animator.SetTrigger("IsIdleJump");
        }
        else
        {
            _animator.SetTrigger("IsJump");
        }
    }

    public void OnAttackEnd()
    {
        _animator.SetBool("IsIdle", true);
    }
}

using StarterAssets;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private Animator _animator;
    private PlayerInputs _input;
    private CharacterController _controller;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _input = GetComponent<PlayerInputs>();
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
        bool isWalking = speed > 0.1f && !_input.sprint;
        bool isRunning = _input.sprint;

        _animator.SetBool("IsWalk", isWalking);
        _animator.SetBool("IsRun", isRunning);

        // 점프는 Trigger 사용
        if (_input.jump)
        {
            _animator.SetTrigger("IsJump");
        }
    }
}

using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    private float MoveSpeed; // PlayerStat에서 받아오기
    public float SprintSpeed = 15f;
    public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10.0f;

    [Header("Jump Settings")]
    public float JumpHeight = 2.0f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;

    [Header("Grounded Settings")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    private float _speed;
    private float _verticalVelocity;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private CharacterController _controller;
    private PlayerInputs _input;
    private Transform _mainCameraTransform;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#endif
        _input = GetComponent<PlayerInputs>();
        _mainCameraTransform = Camera.main?.transform;
    }

    private void Start()
    {
        // PlayerStat의 속도를 가져와서 MoveSpeed에 적용
        MoveSpeed = PlayerStat.Instance.GetSpeed();
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        // PlayerStat의 속도 변경 사항을 반영
        MoveSpeed = PlayerStat.Instance.GetSpeed();

        GroundedCheck();
        JumpAndGravity();
        Move();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (!Grounded)
        {
            Grounded = _controller.isGrounded;
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            if (_input.jump)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _input.jump = false;
            }
        }
        else
        {
            _fallTimeoutDelta -= Time.deltaTime;
        }

        if (_verticalVelocity < 53f)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Move()
    {
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        if (_input.move == Vector2.zero)
        {
            targetSpeed = 0f;
        }

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
        _speed = Mathf.Round(_speed * 1000f) / 1000f;

        if (_input.move != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y).normalized;
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCameraTransform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
        _controller.Move(targetDirection * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
    }
}

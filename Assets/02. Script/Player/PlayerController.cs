using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    private float MoveSpeed; 
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

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 500.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;
    [Tooltip("Vertical offset for the camera target (affects camera height)")]
    public float CameraHeightOffset = 2.0f;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private const float _threshold = 0.01f;


    private float _speed;
    private float _verticalVelocity;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private PlayerInputs _input;
    private GameObject _mainCamera;
    private Transform _mainCameraTransform;

    // 점프 발생 이벤트 추가
    public event System.Action OnJumpTriggered;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        _controller = GetComponent<CharacterController>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#endif
        _input = GetComponent<PlayerInputs>();
        _mainCameraTransform = Camera.main?.transform;
    }

    private void Start()
    {
        MoveSpeed = PlayerStat.Instance.GetSpeed();
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        // 초기 카메라 회전값 설정: 현재 카메라 대상의 회전값을 기준으로 함
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;
        _cinemachineTargetPitch = CinemachineCameraTarget.transform.eulerAngles.x;

        // 초기 인풋 Look 값 초기화
        _input.Look = Vector2.zero;
    }
    private void LateUpdate()
    {
        // 플레이어 위치에 높이 오프셋을 추가하여 카메라 대상 위치 업데이트
        CinemachineCameraTarget.transform.position = transform.position + Vector3.up * CameraHeightOffset;
        CameraRotation();
    }


    private void Update()
    {
        // PlayerStat의 속도 변경 사항을 반영
        MoveSpeed = PlayerStat.Instance.GetSpeed();

        GroundedCheck();
        JumpAndGravity();
        Move();
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void CameraRotation()
    {
        // 인풋이 있고 카메라 고정이 해제된 경우에만 회전 적용
        if (_input.Look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            // 마우스 입력은 Time.deltaTime을 곱하지 않음
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.Look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch -= _input.Look.y * deltaTimeMultiplier; // 마우스 y입력을 반전시킴
        }

        // 회전 값 제한
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine이 이 대상의 회전을 따르도록 설정
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
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

            if (_input.Jump)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                // 이벤트를 통해 점프를 알림
                OnJumpTriggered?.Invoke();
                _input.Jump = false;
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
        float targetSpeed = _input.Sprint ? SprintSpeed : MoveSpeed;
        if (_input.Move == Vector2.zero)
        {
            targetSpeed = 0f;
        }

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
        float inputMagnitude = _input.analogMovement ? _input.Move.magnitude : 1f;

        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
        _speed = Mathf.Round(_speed * 1000f) / 1000f;

        if (_input.Move != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(_input.Move.x, 0f, _input.Move.y).normalized;
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCameraTransform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
        _controller.Move(targetDirection * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
    }
}

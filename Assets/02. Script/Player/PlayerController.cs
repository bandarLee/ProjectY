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

    // cinemachine 회전 제어용 변수
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

        // 초기 카메라 회전값 설정 (CinemachineCameraTarget의 현재 회전값 기준)
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;
        _cinemachineTargetPitch = CinemachineCameraTarget.transform.eulerAngles.x;

        _input.Look = Vector2.zero;
    }

    private void LateUpdate()
    {
        // Cinemachine에 “위치 제어” 코드로 “회전만” 제어하는 방법으로 변경하여 역할 분리
        // 위치 제어는 Cinemachine이 담당하므로 코드는 회전만 업데이트
        CameraRotation();
    }

    private void Update()
    {
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
        if (_input.Look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            _cinemachineTargetYaw += _input.Look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch -= _input.Look.y * deltaTimeMultiplier; 
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // 회전 값만 업데이트하는 부뷰ㅜㄴ
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

        // 현재 속도 계산
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
        float inputMagnitude = _input.analogMovement ? _input.Move.magnitude : 1f;
        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
        _speed = Mathf.Round(_speed * 1000f) / 1000f;

        // 이동 방향 계산
        if (_input.Move != Vector2.zero)
        {
            // 만약 전진, 전진+측면
            if (_input.Move.y >= 0)
            {
                Vector3 inputDirection = new Vector3(_input.Move.x, 0f, _input.Move.y).normalized;
                float desiredRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCameraTransform.eulerAngles.y;
                float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.y, desiredRotation);

                // 90도 이상=== 회전을 업데이트하지 않음
                if (Mathf.Abs(angleDifference) > 90f)
                {
                    _targetRotation = transform.eulerAngles.y;
                }
                else
                {
                    _targetRotation = desiredRotation;
                }

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);

                Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
                _controller.Move(targetDirection * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
            }
            // S 키이동만일 때
            else
            {
                Vector3 backwardComponent = -transform.forward * Mathf.Abs(_input.Move.y);
                Vector3 lateralComponent = transform.right * _input.Move.x;
                Vector3 moveDir = (backwardComponent + lateralComponent).normalized;

                _controller.Move(moveDir * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
            }
        }
        else
        {
            _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }
    }

}

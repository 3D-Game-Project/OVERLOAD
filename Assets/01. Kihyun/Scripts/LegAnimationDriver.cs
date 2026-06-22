using UnityEngine;

public class LegAnimationDriver : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator _animator;

    [Header("Smoothing")]
    [SerializeField] private float _moveParamSmooth = 10f;
    [SerializeField] private float _turnParamSmooth = 10f;

    [Header("Threshold")]
    [SerializeField] private float _movingThreshold = 0.1f;
    [SerializeField] private float _turningThreshold = 5f;

    private float _moveX;
    private float _moveY;
    private float _speed;
    private float _turn;

    private float _previousYaw;
    private bool _hasPreviousYaw;

    private bool _hasMoveX;
    private bool _hasMoveY;
    private bool _hasSpeed;
    private bool _hasTurn;
    private bool _hasIsMoving;
    private bool _hasIsTurning;
    private bool _hasIsGrounded;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int TurnHash = Animator.StringToHash("Turn");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsTurningHash = Animator.StringToHash("IsTurning");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int MoveSpeedMultiplierHash = Animator.StringToHash("MoveSpeedMultiplier");

    public void Initialize(RuntimeAnimatorController controller)
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Animator가 없습니다.");
            return;
        }

        if (controller != null)
            _animator.runtimeAnimatorController = controller;

        _animator.applyRootMotion = false;

        CacheAnimatorParameters();

        _previousYaw = transform.eulerAngles.y;
        _hasPreviousYaw = true;
    }

    public void UpdateAnimation(
        Vector3 worldVelocity,
        Transform legYawRoot,
        bool isGrounded,
        float turnInput,
        bool isTurning,
        float deltaTime)
    {
        if (_animator == null)
            return;

        if (legYawRoot == null)
            legYawRoot = transform;

        Vector3 horizontalVelocity = worldVelocity;
        horizontalVelocity.y = 0f;

        Vector3 localVelocity = legYawRoot.InverseTransformDirection(horizontalVelocity);

        float targetSpeed = horizontalVelocity.magnitude;
        float targetMoveX = 0f;
        float targetMoveY = 0f;

        if (targetSpeed > 0.001f)
        {
            targetMoveX = localVelocity.x / targetSpeed;
            targetMoveY = localVelocity.z / targetSpeed;
        }

        float targetTurn = Mathf.Clamp(turnInput, -1f, 1f);

        _moveX = Mathf.Lerp(_moveX, targetMoveX, deltaTime * _moveParamSmooth);
        _moveY = Mathf.Lerp(_moveY, targetMoveY, deltaTime * _moveParamSmooth);
        _speed = Mathf.Lerp(_speed, targetSpeed, deltaTime * _moveParamSmooth);
        _turn = Mathf.Lerp(_turn, targetTurn, deltaTime * _turnParamSmooth);

        bool isMoving = _speed > _movingThreshold;

        SetFloatIfExists(MoveXHash, _hasMoveX, _moveX);
        SetFloatIfExists(MoveYHash, _hasMoveY, _moveY);
        SetFloatIfExists(SpeedHash, _hasSpeed, _speed);
        SetFloatIfExists(TurnHash, _hasTurn, _turn);

        SetBoolIfExists(IsMovingHash, _hasIsMoving, isMoving);
        SetBoolIfExists(IsTurningHash, _hasIsTurning, isTurning);
        SetBoolIfExists(IsGroundedHash, _hasIsGrounded, isGrounded);
    }

    public void Stop()
    {
        _moveX = 0f;
        _moveY = 0f;
        _speed = 0f;
        _turn = 0f;

        if (_animator == null)
            return;

        SetFloatIfExists(MoveXHash, _hasMoveX, 0f);
        SetFloatIfExists(MoveYHash, _hasMoveY, 0f);
        SetFloatIfExists(SpeedHash, _hasSpeed, 0f);
        SetFloatIfExists(TurnHash, _hasTurn, 0f);

        SetBoolIfExists(IsMovingHash, _hasIsMoving, false);
        SetBoolIfExists(IsTurningHash, _hasIsTurning, false);
    }

    private void CacheAnimatorParameters()
    {
        _hasMoveX = false;
        _hasMoveY = false;
        _hasSpeed = false;
        _hasTurn = false;
        _hasIsMoving = false;
        _hasIsTurning = false;
        _hasIsGrounded = false;

        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            switch (parameter.name)
            {
                case "MoveX":
                    _hasMoveX = true;
                    break;

                case "MoveY":
                    _hasMoveY = true;
                    break;

                case "Speed":
                    _hasSpeed = true;
                    break;

                case "Turn":
                    _hasTurn = true;
                    break;

                case "IsMoving":
                    _hasIsMoving = true;
                    break;

                case "IsTurning":
                    _hasIsTurning = true;
                    break;

                case "IsGrounded":
                    _hasIsGrounded = true;
                    break;
            }
        }
    }

    private void SetFloatIfExists(int hash, bool exists, float value)
    {
        if (!exists)
            return;

        _animator.SetFloat(hash, value);
    }

    private void SetBoolIfExists(int hash, bool exists, bool value)
    {
        if (!exists)
            return;

        _animator.SetBool(hash, value);
    }

    public void SetRunning(bool isRunning)
    {
        if (_animator == null)
            return;

        _animator.SetBool(IsRunningHash, isRunning);
    }

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        if (_animator == null)
            return;

        _animator.SetFloat(
            MoveSpeedMultiplierHash,
            Mathf.Max(0.01f, multiplier)
        );
    }
}
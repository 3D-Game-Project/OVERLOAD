using UnityEngine;

public class BossLegAnimationBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossLocomotionMotor _motor;
    [SerializeField] private Transform _legYawRoot;

    [Header("Turning")]
    [SerializeField] private float _fullTurnSpeed = 180f;
    [SerializeField] private float _turningThreshold = 5f;

    private LegAnimationDriver _driver;
    private float _previousYaw;
    private bool _hasPreviousYaw;

    private void Awake()
    {
        if (_motor == null)
            _motor = GetComponent<BossLocomotionMotor>();

        _previousYaw = transform.eulerAngles.y;
        _hasPreviousYaw = true;
    }

    private void LateUpdate()
    {
        if (_motor == null)
            return;

        if (_driver == null ||
            !_driver.gameObject.activeInHierarchy)
        {
            FindLegAnimationDriver();
        }

        if (_driver == null)
            return;

        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return;

        float currentYaw = transform.eulerAngles.y;
        float yawDelta = _hasPreviousYaw
            ? Mathf.DeltaAngle(_previousYaw, currentYaw)
            : 0f;

        float angularSpeed = yawDelta / deltaTime;

        _previousYaw = currentYaw;
        _hasPreviousYaw = true;

        float turnInput = Mathf.Clamp(
            angularSpeed / _fullTurnSpeed,
            -1f,
            1f
        );

        bool isTurning =
            Mathf.Abs(angularSpeed) >= _turningThreshold;

        _driver.UpdateAnimation(
            _motor.CurrentMoveVelocity,
            _legYawRoot,
            _motor.IsGrounded,
            turnInput,
            isTurning,
            deltaTime
        );
    }

    private void FindLegAnimationDriver()
    {
        _driver =
            GetComponentInChildren<LegAnimationDriver>();

        if (_driver != null)
            _driver.Initialize(null);
    }
}
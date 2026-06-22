using UnityEngine;

public class BuggyWheelVisualController : MonoBehaviour
{
    [Header("Steering")]
    [SerializeField] private Transform _steerFL;
    [SerializeField] private Transform _steerFR;
    [SerializeField] private Vector3 _steerLocalAxis = Vector3.up;
    [SerializeField] private float _maxSteerAngle = 30f;
    [SerializeField] private float _steerSmooth = 10f;

    [Header("Wheel Roll")]
    [SerializeField] private Transform _wheelFL;
    [SerializeField] private Transform _wheelFR;
    [SerializeField] private Transform _wheelRL;
    [SerializeField] private Transform _wheelRR;

    [SerializeField] private Vector3 _rollLocalAxis = Vector3.right;
    [SerializeField] private float _wheelRadius = 0.35f;

    [Header("Roll Direction")]
    [SerializeField] private float _flRollSign = 1f;
    [SerializeField] private float _frRollSign = 1f;
    [SerializeField] private float _rlRollSign = 1f;
    [SerializeField] private float _rrRollSign = 1f;

    private Quaternion _steerFLBase;
    private Quaternion _steerFRBase;

    private Quaternion _wheelFLBase;
    private Quaternion _wheelFRBase;
    private Quaternion _wheelRLBase;
    private Quaternion _wheelRRBase;

    private float _targetSteeringInput;
    private float _currentSteerAngle;
    private float _rollAngle;
    private float _signedSpeed;

    private void Awake()
    {
        CacheBaseRotations();
    }

    public void SetMotion(float signedSpeed, float steeringInput)
    {
        _signedSpeed = signedSpeed;
        _targetSteeringInput = Mathf.Clamp(steeringInput, -1f, 1f);
    }

    private void LateUpdate()
    {
        UpdateSteering(Time.deltaTime);
        UpdateWheelRoll(Time.deltaTime);
    }

    private void UpdateSteering(float deltaTime)
    {
        float targetAngle =
            _targetSteeringInput * _maxSteerAngle;

        float blend =
            1f - Mathf.Exp(-_steerSmooth * deltaTime);

        _currentSteerAngle = Mathf.LerpAngle(
            _currentSteerAngle,
            targetAngle,
            blend
        );

        Quaternion offset = Quaternion.AngleAxis(
            _currentSteerAngle,
            _steerLocalAxis.normalized
        );

        if (_steerFL != null)
            _steerFL.localRotation = _steerFLBase * offset;

        if (_steerFR != null)
            _steerFR.localRotation = _steerFRBase * offset;
    }

    private void UpdateWheelRoll(float deltaTime)
    {
        if (_wheelRadius <= 0.001f)
            return;

        float rollDelta =
            (_signedSpeed / _wheelRadius) *
            Mathf.Rad2Deg *
            deltaTime;

        _rollAngle = Mathf.Repeat(
            _rollAngle + rollDelta,
            360f
        );

        ApplyRoll(_wheelFL, _wheelFLBase, _flRollSign);
        ApplyRoll(_wheelFR, _wheelFRBase, _frRollSign);
        ApplyRoll(_wheelRL, _wheelRLBase, _rlRollSign);
        ApplyRoll(_wheelRR, _wheelRRBase, _rrRollSign);
    }

    private void ApplyRoll(
        Transform wheel,
        Quaternion baseRotation,
        float direction
    )
    {
        if (wheel == null)
            return;

        Quaternion offset = Quaternion.AngleAxis(
            _rollAngle * direction,
            _rollLocalAxis.normalized
        );

        wheel.localRotation = baseRotation * offset;
    }

    private void CacheBaseRotations()
    {
        if (_steerFL != null)
            _steerFLBase = _steerFL.localRotation;

        if (_steerFR != null)
            _steerFRBase = _steerFR.localRotation;

        if (_wheelFL != null)
            _wheelFLBase = _wheelFL.localRotation;

        if (_wheelFR != null)
            _wheelFRBase = _wheelFR.localRotation;

        if (_wheelRL != null)
            _wheelRLBase = _wheelRL.localRotation;

        if (_wheelRR != null)
            _wheelRRBase = _wheelRR.localRotation;
    }
}
using UnityEngine;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public class FootIKPoint
{
    public string name;
    public Transform animatedFoot;      // 애니메이션 발 위치
    public Transform ikTarget;          // chain ik가 갈 위치
    public TwoBoneIKConstraint ikConstraint;
    public float footHeightOffset = 0.05f;

    public string weightParameterName;

    [System.NonSerialized] public float currentWeight;
}

public class LegTerrainAdapter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _tiltRoot;
    [SerializeField] private Transform[] _terrainProbes;

    [Header("Raycast")]
    [SerializeField] private LayerMask _groundLayer = ~0;
    [SerializeField] private float _rayStartHeight = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugRay = true;
    [SerializeField] private bool _logGroundHit = false;

    [Header("Foot IK")]
    [SerializeField] private FootIKPoint[] _footIKPoints;
    [SerializeField] private float _footRayStartHeight = 1.5f;
    [SerializeField] private float _footRayDistance = 3f;
    [SerializeField] private float _footIKSmooth = 20f;
    [SerializeField] private float _footPlantHeight = 0.12f;
    [SerializeField] private float _footReleaseHeight = 0.45f;
    [SerializeField] private float _footWeightSmooth = 12f;
    [SerializeField] private float _landingBlendInTime = 0.2f;
    [SerializeField] private bool _useFootIK = true;

    [Header("Animator Foot Weight")]
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _useAnimationFootWeight = true;

    [Header("Airborne Leg Pose")]
    [SerializeField] private bool _useAirborneLegPose = true;
    [SerializeField] private float _airborneIKWeight = 0.5f;
    [SerializeField] private float _airborneFootDropDistance = 0.35f;
    [SerializeField] private float _airborneBlendSpeed = 8f;

    private LegTypeProfile _profile;
    private Quaternion _baseLocalRotation;
    private bool _initialized;
    private PlayerLocomotionMotor _locomotionMotor;
    private float _groundedTime;

    public void Initialize(LegTypeProfile profile, PlayerLocomotionMotor locomotionMotor)
    {
        _profile = profile;
        _locomotionMotor = locomotionMotor;

        if (_tiltRoot == null)
            _tiltRoot = transform;

        _baseLocalRotation = _tiltRoot.localRotation;
        _initialized = true;

        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    private void LateUpdate()
    {
        if (!_initialized)
            return;

        if (_profile == null)
            return;

        if (_profile.TerrainAdaptMode == LegTerrainAdaptMode.None)
            return;

        if (_profile.TerrainAdaptMode != LegTerrainAdaptMode.BodyTiltOnly &&
            _profile.TerrainAdaptMode != LegTerrainAdaptMode.FootIK &&
            _profile.TerrainAdaptMode != LegTerrainAdaptMode.Suspension)
            return;

        UpdateBodyTilt(Time.deltaTime);

        if (_useFootIK && _profile.TerrainAdaptMode == LegTerrainAdaptMode.FootIK)
        {
            UpdateFootIK(Time.deltaTime);
        }
    }

    private void UpdateBodyTilt(float deltaTime)
    {
        if (_tiltRoot == null)
            return;

        if (_terrainProbes == null || _terrainProbes.Length == 0)
            return;

        if (!TryGetAverageGroundNormal(out Vector3 averageNormal))
        {
            ResetTilt(deltaTime);
            return;
        }

        Transform parent = _tiltRoot.parent;

        Vector3 baseUp = parent != null ? parent.up : Vector3.up;

        float angle = Vector3.Angle(baseUp, averageNormal);

        if (angle > _profile.MaxBodyTiltAngle)
        {
            float t = _profile.MaxBodyTiltAngle / angle;
            averageNormal = Vector3.Slerp(baseUp, averageNormal, t).normalized;
        }

        averageNormal = Vector3.Slerp(baseUp, averageNormal, _profile.BodyTiltWeight).normalized;

        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, averageNormal);

        if (projectedForward.sqrMagnitude < 0.001f)
            projectedForward = Vector3.ProjectOnPlane(transform.forward, averageNormal);

        if (projectedForward.sqrMagnitude < 0.001f)
            return;

        Quaternion targetWorldRotation = Quaternion.LookRotation(projectedForward.normalized, averageNormal);

        Quaternion targetLocalRotation = parent != null
            ? Quaternion.Inverse(parent.rotation) * targetWorldRotation
            : targetWorldRotation;

        _tiltRoot.localRotation = Quaternion.Slerp(_tiltRoot.localRotation, targetLocalRotation, deltaTime * _profile.BodyTiltSmooth);
    }

    private void UpdateFootIK(float deltaTime)
    {
        if (_footIKPoints == null)
            return;

        bool isGrounded =
            _locomotionMotor == null ||
            _locomotionMotor.IsGrounded;

        if (isGrounded)
            _groundedTime += deltaTime;
        else
            _groundedTime = 0f;

        float landingWeight = _landingBlendInTime <= 0f
            ? 1f
            : Mathf.Clamp01(_groundedTime / _landingBlendInTime);

        foreach (FootIKPoint foot in _footIKPoints)
        {
            if (foot == null)
                continue;

            if (foot.animatedFoot == null || foot.ikTarget == null)
                continue;

            Vector3 origin = foot.animatedFoot.position + Vector3.up * _footRayStartHeight;
            float distance = _footRayStartHeight + _footRayDistance;

            bool hasHit = Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                distance,
                _groundLayer,
                QueryTriggerInteraction.Ignore
            );

            if (_showDebugRay)
            {
                Debug.DrawRay(
                    origin,
                    Vector3.down * distance,
                    hasHit ? Color.cyan : Color.red
                );
            }

            float targetWeight = 0f;

            if (hasHit && isGrounded)
            {
                float animationFootWeight = 1f;

                if (_useAnimationFootWeight &&
                    _animator != null &&
                    !string.IsNullOrEmpty(foot.weightParameterName))
                {
                    animationFootWeight = Mathf.Clamp01(
                        _animator.GetFloat(foot.weightParameterName)
                    );
                }

                targetWeight = animationFootWeight * landingWeight;

                if (targetWeight > 0.01f)
                {
                    Vector3 targetPosition =
                        hit.point + Vector3.up * foot.footHeightOffset;

                    foot.ikTarget.position = Vector3.Lerp(
                        foot.ikTarget.position,
                        targetPosition,
                        deltaTime * _footIKSmooth
                    );
                }
                else
                {
                    foot.ikTarget.position = Vector3.Lerp(
                        foot.ikTarget.position,
                        foot.animatedFoot.position,
                        deltaTime * _footIKSmooth
                    );
                }
            }
            else
            {
                if (_useAirborneLegPose)
                {
                    Vector3 airborneTargetPosition =
                        foot.animatedFoot.position + Vector3.down * _airborneFootDropDistance;

                    foot.ikTarget.position = Vector3.Lerp(
                        foot.ikTarget.position,
                        airborneTargetPosition,
                        deltaTime * _airborneBlendSpeed
                    );

                    targetWeight = _airborneIKWeight;
                }
                else
                {
                    foot.ikTarget.position = Vector3.Lerp(
                        foot.ikTarget.position,
                        foot.animatedFoot.position,
                        deltaTime * _footIKSmooth
                    );

                    targetWeight = 0f;
                }
            }

            foot.currentWeight = Mathf.Lerp(
                foot.currentWeight,
                targetWeight,
                deltaTime * _footWeightSmooth
            );

            if (foot.ikConstraint != null)
            {
                foot.ikConstraint.weight = foot.currentWeight;
            }
        }
    }

    private bool TryGetAverageGroundNormal(out Vector3 averageNormal)
    {
        Vector3 normalSum = Vector3.zero;
        int hitCount = 0;

        float probeDistance = _profile != null ? _profile.TerrainProbeDistance : 4f;

        foreach(Transform probe in _terrainProbes)
        {
            if (probe == null)
                continue;

            Vector3 origin = probe.position + Vector3.up * _rayStartHeight;
            float distance = probeDistance + _rayStartHeight;

            bool hasHit = Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                distance,
                _groundLayer,
                QueryTriggerInteraction.Ignore
            );

            if (_showDebugRay)
            {
                Debug.DrawRay(
                    origin,
                    Vector3.down * distance,
                    hasHit ? Color.green : Color.red
                );
            }

            if (hasHit)
            {
                normalSum += hit.normal;
                hitCount++;

                if (_logGroundHit)
                {
                    Debug.Log(
                        $"[TerrainProbe Hit] {probe.name} / Hit: {hit.collider.name} / Normal: {hit.normal}"
                    );
                }
            }
        }

        if (hitCount <=0)
        {
            averageNormal = Vector3.up;
            return false;
        }

        averageNormal = (normalSum / hitCount).normalized;
        return true;
    }

    private void ResetTilt(float deltaTime)
    {
        if (_tiltRoot == null)
            return;

        _tiltRoot.localRotation = Quaternion.Slerp(
            _tiltRoot.localRotation,
            _baseLocalRotation,
            deltaTime * _profile.BodyTiltSmooth
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_terrainProbes == null)
            return;

        Gizmos.color = Color.yellow;

        foreach(Transform probe in _terrainProbes)
        {
            if (probe == null)
                continue;

            Gizmos.DrawSphere(probe.position, 0.08f);
            Gizmos.DrawLine(probe.position + Vector3.up * _rayStartHeight, probe.position + Vector3.down * 2f);
        }
    }
#endif
}

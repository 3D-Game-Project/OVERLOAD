using UnityEngine;

[CreateAssetMenu(fileName = "New_Leg_Type_Parts", menuName = "Data/Parts/Leg/Leg Type Profile")]
public class LegTypeProfile : ScriptableObject
{
    [Header("다리 타입")]
    [SerializeField] private LegType _legType;

    [Header("높이 정보")]
    [SerializeField] private Vector3 _coreMountPosition;

    [Header("지형 대응")]
    [SerializeField] private bool _canMoveOnRoughTerrain;
    [SerializeField] private float _roughTerrainSpeedMultiplier = 0.7f;
    [SerializeField] private float _maxClimbHeight;
    [SerializeField] private float _slopeLimit;

    [Header("이동 특성")]
    [SerializeField] private float _dashMultiplier;
    [SerializeField] private float _turnSpeedMultiplier = 1f;
    [SerializeField] private float _stability = 1f;

    [Header("회전 / 이동 제어 방식")]
    [SerializeField] private LegYawControlMode _yawControlMode;
    [SerializeField] private LegMoveControlMode _moveControlMode;

    [Header("카메라 추적 회전 설정")]
    [SerializeField] private float _turnStartAngle = 35f;
    [SerializeField] private float _turnStopAngle = 10f;

    [Header("애니메이션 지원 여부")]
    [SerializeField] private bool _hasStrafeAnimation = true;
    [SerializeField] private bool _hasTurnAnimation = true;

    [Header("지형 시각 보정")]
    [SerializeField] private LegTerrainAdaptMode _terrainAdaptMode = LegTerrainAdaptMode.BodyTiltOnly;
    [SerializeField] private float _bodyTiltWeight = 1f;
    [SerializeField] private float _bodyTiltSmooth = 8f;
    [SerializeField] private float _maxBodyTiltAngle = 25f;
    [SerializeField] private float _terrainProbeDistance = 4f;

    public LegType LegType => _legType;
    public Vector3 CoreMountPosition => _coreMountPosition;

    public bool CanMoveOnRoughTerrain => _canMoveOnRoughTerrain;
    public float RoughTerrainSpeedMultiplier => _roughTerrainSpeedMultiplier;
    public float MaxClimbHeight => _maxClimbHeight;
    public float SlopeLimit => _slopeLimit;

    public float DashMultiplier => _dashMultiplier;
    public float TurnSpeedMultiplier => _turnSpeedMultiplier;
    public float Stability => _stability;

    public LegYawControlMode YawControlMode => _yawControlMode;
    public LegMoveControlMode MoveControlMode => _moveControlMode;

    public float TurnStartAngle => _turnStartAngle;
    public float TurnStopAngle => _turnStopAngle;

    public bool HasStrafeAnimation => _hasStrafeAnimation;
    public bool HasTurnAnimation => _hasTurnAnimation;

    public LegTerrainAdaptMode TerrainAdaptMode => _terrainAdaptMode;
    public float BodyTiltWeight => _bodyTiltWeight;
    public float BodyTiltSmooth => _bodyTiltSmooth;
    public float MaxBodyTiltAngle => _maxBodyTiltAngle;
    public float TerrainProbeDistance => _terrainProbeDistance;
}

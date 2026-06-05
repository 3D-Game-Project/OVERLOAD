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
    [SerializeField] private float _turnSpeedMultiplier = 1f;
    [SerializeField] private float _stability = 1f;

    public LegType LegType => _legType;
    public Vector3 CoreMountPositino => _coreMountPosition;

    public bool CanMoveOnRoughTerrain => _canMoveOnRoughTerrain;
    public float RoughTerrainSpeedMultiplier => _roughTerrainSpeedMultiplier;
    public float MaxClimbHeight => _maxClimbHeight;
    public float SlopeLimit => _slopeLimit;

    public float TurnSpeedMultiplier => _turnSpeedMultiplier;
    public float Stability => _stability;
}

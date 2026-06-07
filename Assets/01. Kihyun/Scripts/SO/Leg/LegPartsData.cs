using UnityEngine;

[CreateAssetMenu(fileName = "New_Leg_Parts", menuName = "Data/Parts/Leg/LegParts")]
public class LegPartsData : PartsData
{
    public override PartsType PartsType => PartsType.Leg;

    [Header("다리 타입 프로필")]
    [SerializeField] private LegTypeProfile _legTypeProfile;

    [Header("다리 정보")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _decceleration;

    [Header("코어 에너지 소모")]
    [SerializeField] private float _dashEnergyCost;

    // 프로필
    public LegType LegType => _legTypeProfile.LegType;
    public LegTypeProfile LegTypeProfile => _legTypeProfile;

    public float MoveSpeed => _moveSpeed;
    public float Acceleration => _acceleration;
    public float Decceleration => _decceleration;
    public float DashEnergyCost => _dashEnergyCost;
}

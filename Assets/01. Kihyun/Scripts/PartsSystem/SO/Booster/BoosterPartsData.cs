using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New_Booster_Parts", menuName = "Data/Parts/Booster")]
public class BoosterPartsData : PartsData
{
    public override PartsType PartsType => PartsType.Booster;

    [Header("부스터 기능")]
    [SerializeField] private BoosterFunction _boosterFunction;

    [Header("점프")]
    [SerializeField] private float _jumpVelocity = 8f;
    [SerializeField] private float _jumpEnergyCost = 15f;

    [Header("대시")]
    [SerializeField] private float _dashSpeed = 12f;

    [FormerlySerializedAs("_dashEnergyCost")]
    [SerializeField] private float _dashEnergyCostPerSec = 20f;

    [Header("비행 / 상승")]
    [SerializeField] private float _flightVerticalAcceleration = 25f;
    [SerializeField] private float _flightHorizontalSpeed = 4f;
    [SerializeField] private float _flightEnergyCostPerSec = 20f;

    [Header("활공")]
    [SerializeField] private float _glideFallSpeedLimit = -3f;
    [SerializeField] private float _glideEnergyCostPerSec = 5f;

    public BoosterFunction BoosterFunction => _boosterFunction;

    public float JumpVelocity => _jumpVelocity;
    public float JumpEnergyCost => _jumpEnergyCost;

    public float DashSpeed => _dashSpeed;
    public float DashEnergyCostPerSec => _dashEnergyCostPerSec;

    public float FlightVerticalAcceleration => _flightVerticalAcceleration;
    public float FlightHorizontalSpeed => _flightHorizontalSpeed;
    public float FlightEnergyCostPerSec => _flightEnergyCostPerSec;

    public float GlideFallSpeedLimit => _glideFallSpeedLimit;
    public float GlideEnergyCostPerSec => _glideEnergyCostPerSec;

    public bool HasFunction(BoosterFunction function)
    {
        return (_boosterFunction & function) != 0;
    }
}
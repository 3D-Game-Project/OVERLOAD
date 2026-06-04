using UnityEngine;

// 코어 출력 전용 클래스
public class CorePowerController : MonoBehaviour
{
    [Header("Power")]
    [SerializeField] private float _maxPower = 100f;
    [SerializeField] private float _currentPower = 100f;
    [SerializeField] private float _recoverPowerPerSec = 15f;
    [SerializeField] private float _recoverDelayAfterUse = 0.5f;

    private float _lastPowerUsedTime = -999f;

    public float CurrentPower => _currentPower;
    public float MaxPower => _maxPower;

    // _현재 남은 출력 비율로 계산
    public float PowerRatio => _maxPower <= 0f ? 0f : _currentPower / _maxPower;

    private void Update()
    {
        RecoverPower();
    }

    // 코어 출력 회복
    // 시간마다 일정정도 지속적 회복
    private void RecoverPower()
    {
        // 출력이 계속 차는 거 방지용
        if (Time.time - _lastPowerUsedTime < _recoverDelayAfterUse)
            return;

        _currentPower += _recoverPowerPerSec * Time.deltaTime;
        _currentPower = Mathf.Min(_currentPower, _maxPower);
    }

    public bool CanUsePower(float amount)
    {
        if (amount <= 0f)
            return true;

        return _currentPower >= amount;
    }

    // public으로 외부에서 출력을 사용할 일이 있으면 가져가서 사용
    // 직관적으로 amount만큼 사용됨
    public bool TryUsePower(float amount)
    {
        if (amount <= 0f)
            return true;

        if (_currentPower < amount)
            return false;

        _currentPower -= amount;
        _lastPowerUsedTime = Time.time;
        return true;
    }

    // 점진적으로 줄어드는 출력의 경우 이거 사용
    // 위는 즉발적으로 줄어드는 경우, 이건 점진적으로 줄어드는 경우
    public bool TryUsePowerPerSec(float costPerSec)
    {
        float amount = costPerSec * Time.deltaTime;
        return TryUsePower(amount);
    }

    // 코어 교체로 최대 출력량 수정시 필요
    public void SetMaxPower(float newMaxPower)
    {
        if (newMaxPower <= 0f)
            return;

        _maxPower = newMaxPower;
        _currentPower = newMaxPower;
    }
}
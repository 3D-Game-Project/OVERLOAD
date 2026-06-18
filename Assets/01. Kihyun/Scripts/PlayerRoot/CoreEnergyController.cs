using UnityEngine;

// 코어 에너지 전용 클래스
// 대시, 공격, 부스터, 비행처럼 실제 행동 시 소모되는 자원
public class CoreEnergyController : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _currentEnergy = 100f;
    [SerializeField] private float _recoverEnergyPerSec = 15f;
    [SerializeField] private float _recoverDelayAfterUse = 0.5f;

    private float _lastEnergyUsedTime = -999f;

    public float CurrentEnergy => _currentEnergy;
    public float MaxEnergy => _maxEnergy;

    public float EnergyRatio => _maxEnergy <= 0f ? 0f : _currentEnergy / _maxEnergy;

    private void Update()
    {
        RecoverEnergy();
    }

    private void RecoverEnergy()
    {
        if (Time.time - _lastEnergyUsedTime < _recoverDelayAfterUse)
            return;

        _currentEnergy += _recoverEnergyPerSec * Time.deltaTime;
        _currentEnergy = Mathf.Min(_currentEnergy, _maxEnergy);
    }

    public bool CanUseEnergy(float amount)
    {
        if (amount <= 0f)
            return true;

        return _currentEnergy >= amount;
    }

    public bool TryUseEnergy(float amount)
    {
        if (amount <= 0f)
            return true;

        if (_currentEnergy < amount)
            return false;

        _currentEnergy -= amount;
        _lastEnergyUsedTime = Time.time;
        return true;
    }

    public bool TryUseEnergyPerSec(float costPerSec)
    {
        float amount = costPerSec * Time.deltaTime;
        return TryUseEnergy(amount);
    }

    public void SetMaxEnergy(float newMaxEnergy)
    {
        if (newMaxEnergy <= 0f)
            return;

        _maxEnergy = newMaxEnergy;
        _currentEnergy = newMaxEnergy;
    }
}
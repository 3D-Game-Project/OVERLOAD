using System;
using UnityEngine;

public class OverheatController : MonoBehaviour
{
    [Header("과열 관리 데이터")]
    [SerializeField] private float _maxEnergy = 200f;
    [SerializeField] private float _currentEnergy;
    [SerializeField] private float _energyRecoveryRate = 80f;
    [SerializeField] private float _recoveryDelay = 1.5f;

    private float _lastFireTime;
    private bool _isOverheat = false;


    public event Action<float, float> OnEnergyChanged;
    public event Action<bool> OnOverHeated;

    public float CurrentEnergy => _currentEnergy;
    public bool IsOverheat => _isOverheat;

    private void Start()
    {
        _currentEnergy = _maxEnergy;
    }

    private void Update()
    {
        RecoveryOverheat();
    }

    public bool TryConsumeEnergy(float energy)
    {
        Debug.Log("에너지 소모 시작");
        if (_isOverheat) return false;
        if (_currentEnergy <= 0f) return false;

        _currentEnergy -= energy;
        _lastFireTime = Time.time;

        if(_currentEnergy <= 0f)
        {
            _currentEnergy = 0f;
            _isOverheat = true;
            OnOverHeated?.Invoke(_isOverheat);
            Debug.Log("과열됨");
        }
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        return true;
    }

    private void RecoveryOverheat()
    {
        if (_currentEnergy >= _maxEnergy) return;

        if (_isOverheat)
        {
            _currentEnergy += _energyRecoveryRate * Time.deltaTime;

            if (_currentEnergy >= _maxEnergy)
            {
                _currentEnergy = _maxEnergy;
                _isOverheat = false; 
                OnOverHeated?.Invoke(_isOverheat);
                Debug.Log("과열 0이 되었다가 회복");
            }

            OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
        }
        else
        {
            if (Time.time >= _lastFireTime + _recoveryDelay)
            {
                Debug.Log("공격시간이 지나 과열 회복");
                _currentEnergy += _energyRecoveryRate * Time.deltaTime;

                if(_currentEnergy >= _maxEnergy)
                {
                    _currentEnergy = _maxEnergy;
                }

                OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
            }
        }

        
    }
}

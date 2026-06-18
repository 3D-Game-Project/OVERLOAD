using UnityEngine;

// 코어 장착 부하 전용 클래스
// 파츠를 얼마나 장착할 수 있는지 제한하는 값
public class CoreLoadController : MonoBehaviour
{
    [Header("Core Load")]
    [SerializeField] private float _maxLoad = 100f;
    [SerializeField] private float _currentLoad = 0f;

    public float MaxLoad => _maxLoad;
    public float CurrentLoad => _currentLoad;
    public float RemainingLoad => _maxLoad - _currentLoad;

    public bool CanEquip(float requiredLoad)
    {
        if (requiredLoad <= 0f)
            return true;

        return _currentLoad + requiredLoad <= _maxLoad;
    }

    public void AddLoad(float requiredLoad)
    {
        if (requiredLoad <= 0f)
            return;

        _currentLoad += requiredLoad;
        _currentLoad = Mathf.Min(_currentLoad, _maxLoad);
    }

    public void RemoveLoad(float requiredLoad)
    {
        if (requiredLoad <= 0f)
            return;

        _currentLoad -= requiredLoad;
        _currentLoad = Mathf.Max(_currentLoad, 0f);
    }

    public void SetMaxLoad(float newMaxLoad)
    {
        if (newMaxLoad <= 0f)
            return;

        _maxLoad = newMaxLoad;

        if (_currentLoad > _maxLoad)
            _currentLoad = _maxLoad;
    }
}
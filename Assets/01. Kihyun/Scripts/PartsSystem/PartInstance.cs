using UnityEngine;

// SO 말고 내구도는 따로 관리해야하는 변동되는 부분
public class PartInstance : MonoBehaviour
{
    [SerializeField] private PartDefinition _definition;

    private float _currentDurability;

    public PartDefinition Definition => _definition;
    public float CurrentDurability => _currentDurability;

    private void Awake()
    {
        if (_definition != null)
            _currentDurability = _definition.maxDurability;
    }

    public void Initialize()
    {
        if (_definition != null)
            _currentDurability = _definition.maxDurability;
    }

    public void TakeDamage(float damage)
    {
        _currentDurability -= damage;

        if (_currentDurability <= 0f)
        {
            BreakPart();
        }
    }

    private void BreakPart()
    {
        Debug.Log($"{_definition.partName} broken");
    }
}
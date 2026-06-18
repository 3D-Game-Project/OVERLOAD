using UnityEngine;

public class DroppedPartItem : MonoBehaviour
{
    [Header("Dropped Part Data")]
    [SerializeField] private PartsData _partsData;
    [SerializeField] private float _currentDurability;

    public PartsData PartsData => _partsData;
    public float CurrentDurability => _currentDurability;

    public float MaxDurability
    {
        get
        {
            if (_partsData == null)
                return 0f;

            return _partsData.MaxDurability;
        }
    }

    public void Initialize(PartsData partsData, float currentDurability)
    {
        if (partsData == null)
        {
            Debug.LogWarning("[DroppedPartItem] 초기화 실패: PartsData가 없습니다.");
            return;
        }

        _partsData = partsData;
        _currentDurability = Mathf.Clamp(currentDurability, 0f, partsData.MaxDurability);

        Debug.Log(
            $"[DroppedPartItem] 초기화 완료: {_partsData.PartsName} / " +
            $"{_currentDurability:0} / {MaxDurability:0}"
        );
    }
}

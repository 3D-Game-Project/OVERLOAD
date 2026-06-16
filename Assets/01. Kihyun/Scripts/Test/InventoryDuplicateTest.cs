using UnityEngine;

public class InventoryDuplicateTest : MonoBehaviour
{
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private PartsData _testPartData;

    [Header("Input")]
    [SerializeField] private KeyCode _addOneKey = KeyCode.F9;
    [SerializeField] private KeyCode _addTwoKey = KeyCode.F10;

    private void Awake()
    {
        if (_inventory == null)
            _inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_addOneKey))
        {
            AddOne();
        }

        if (Input.GetKeyDown(_addTwoKey))
        {
            AddOne();
            AddOne();
        }
    }

    private void AddOne()
    {
        if (_inventory == null)
        {
            Debug.LogWarning("[InventoryDuplicateTest] PlayerInventory가 없습니다.");
            return;
        }

        if (_testPartData == null)
        {
            Debug.LogWarning("[InventoryDuplicateTest] Test Part Data가 없습니다.");
            return;
        }

        _inventory.AddPart(_testPartData);

        Debug.Log($"[InventoryDuplicateTest] 테스트 파츠 추가: {_testPartData.PartsName}");
    }
}
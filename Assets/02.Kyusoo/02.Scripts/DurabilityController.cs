using UnityEngine;
using System.Collections.Generic;

public class DurabilityController : MonoBehaviour
{
    [SerializeField] private DurabilityType _durabilityType;
    [SerializeField] private float _currentDurability;
    [SerializeField] private float _maxDurability = 300f;
    [SerializeField] private PartsData _partsData;
    [SerializeField] private UnitData _unitData;
    [SerializeField] private string _attachedSlotId;

    private List<PartsData> _destroyedPartList = new List<PartsData>();

    private bool _isDestroyed = false;
    private DurabilityController _rootCoreController;

    private InventoryPartItem _linkedPartItem;

    public float CurrentDurability => _currentDurability;
    public bool IsDestroyed => _isDestroyed;
    public DurabilityType DurabilityType => _durabilityType;
    public PartsData PartsData => _partsData;

    private void Awake()
    {
        
        if(GetComponent<CharacterController>() != null)
        {
            Debug.Log("CharacterController 확인.");
            _durabilityType = DurabilityType.Core;

        }
        else if(GetComponent<Collider>() != null)
        {
            Debug.Log("Collider 확인.");
            _durabilityType = DurabilityType.Part;
        }
    }

    private void Start()
    {
        if (_durabilityType == DurabilityType.Core)
        {
            _currentDurability = _maxDurability;
        }
        else
        {
            _rootCoreController = GetRootCoreController();

            if (_linkedPartItem == null && _partsData != null && _currentDurability <= 0)
            {
                InitializePart(_partsData, false);
            }
        }
    }

    public void InitializePart(PartsData partData, bool isDropPart) 
    {
        _partsData = partData;
        _durabilityType = DurabilityType.Part;
        _maxDurability = partData.MaxDurability;

        if (isDropPart)
        {
            _currentDurability = _maxDurability * 0.8f;
        }
        else
        {
            _currentDurability = _maxDurability;
        }
    }

    public void InitializeFromInventoryItem(InventoryPartItem partItem)
    {
        if (partItem == null || partItem.PartsData == null)
        {
            Debug.LogWarning("[DurabilityController] 연결할 InventoryPartItem 또는 PartsData가 없습니다.");
            return;
        }

        _linkedPartItem = partItem;
        _partsData = partItem.PartsData;
        _durabilityType = DurabilityType.Part;

        _maxDurability = partItem.MaxDurability;
        _currentDurability = partItem.CurrentDurability;

        _isDestroyed = _currentDurability <= 0f;
    }

    private void SyncToInventoryItem()
    {
        if (_linkedPartItem == null)
            return;

        _linkedPartItem.SetDurability(_currentDurability);
    }

    public void TakeDamage(float attackDamage)
    {
        if (_isDestroyed) return;
        float finalDamage = attackDamage;

        float armorDefense = 0f;
        //float armorDefense = GetArmorPartsFromSameLayer();

        if (armorDefense > 0) 
        {
            finalDamage = attackDamage - armorDefense;

            if (finalDamage < 1f) finalDamage = 1f;
            
        }

        _currentDurability -= finalDamage;
        _currentDurability = Mathf.Clamp(_currentDurability, 0f, _maxDurability);

        SyncToInventoryItem();

        if (_currentDurability <= 0f) 
        {
            DestroyPart();
        }
    }

    // 내구도가 0으로 감소되었을 때, 코어, 파츠별 파괴 분기처리
    // 코어는 애니메이션, 드랍 후 Destroy
    // 파츠는 파괴되는 본인의 데이터를 상위로 전달하여 리스트에 추가
    private void DestroyPart()
    {
        if (_currentDurability > 0f) return;

        _isDestroyed = true;

        if(_durabilityType == DurabilityType.Core)
        {
            // 애니메이션 처리

            // DropRuntime.DropPart호출
            if(_unitData is EnemyData enemyData)
            {
                DropRuntime dropRuntime = new DropRuntime();

                dropRuntime.DropParts(enemyData, transform.position, _destroyedPartList);
            }
            Destroy(gameObject, 0.5f);
        }
        else
        {
            ControllComponent();

            if(_rootCoreController != null && _partsData != null)
            {
                _rootCoreController.AddDestroyedPartToList(_partsData);
            }

            Destroy(gameObject);
        }

    }

    // 같은 부위에 부착되는 Armor타입을 Layer로 가져오는 방식
    private float GetArmorPartsFromSameLayer()
    {
        CorePartsController coreParts = GetComponentInParent<CorePartsController>();
        
        if(coreParts == null) return 0f;

        AttachmentSlot[] attachedSlots = coreParts.GetComponentsInChildren<AttachmentSlot>();

        foreach (AttachmentSlot slot in attachedSlots) 
        {
            if (slot == null || slot.SlotId != _attachedSlotId) continue;

            if(slot.HasPart && slot.AttachedObject != null)
            {
                MonoBehaviour[] scripts = slot.AttachedObject.GetComponentsInChildren<MonoBehaviour>();

                foreach(MonoBehaviour script in scripts)
                {
                    if(script is IPart part && part.Data != null && part.Data.PartsType == PartsType.Armor)
                    {
                        //if(part.Data is ArmorPartsData armorData)
                        //{
                        //    return armorData.Defense; 
                        //}
                    }
                }
            }
        }
        return 0f;
    }

    // 부위 파괴시 컴포넌트들 false로 제어하여 발생할 수 있는 문제들(대표적으로 사격)차단
    private void ControllComponent()
    {
        if ((TryGetComponent(out Collider col))) col.enabled = false;

        MonoBehaviour[] component = GetComponentsInChildren<MonoBehaviour>(true);
        foreach(var script in component)
        {
            if (script == null || script == this) continue;

            string name = script.GetType().Name;
            if (name == "WeaponGimbalController" || name == "FireManager" || name == "AttackPartController") 
                script.enabled = false;
        }
    }

    // 코어가 아닌 파츠가 파괴될 때, 파괴된 정보를 코어에게 전달하여 나중에 드랍리스트에서 제거시키는 용도
    public void AddDestroyedPartToList(PartsData part)
    {
        if(_durabilityType == DurabilityType.Core && !_destroyedPartList.Contains(part))
        {
            _destroyedPartList.Add(part);
        }
    }

    // 최상위 부모 오브젝트 계측에서 코어 타입의 DurabilityController를 추적하는 함수
    private DurabilityController GetRootCoreController()
    {
        DurabilityController[] controllers = GetComponentsInParent<DurabilityController>(true);

        foreach (var ctrl in controllers)
        {
            if (ctrl._durabilityType == DurabilityType.Core)
            {
                return ctrl;
            }
        }

        return null;
    }

    public void SetAssociatedSlotId(string slotId)
    {
        Debug.Log($"SetAssociatedSlotId, {slotId}");
        _attachedSlotId = slotId;
    }

    public void InitializeDroppedPart(PartsData partData, float currentDurability)
    {
        if (partData == null)
        {
            Debug.LogWarning("[DurabilityController] 드랍 파츠 초기화 실패: PartsData 없음");
            return;
        }

        _linkedPartItem = null;

        _partsData = partData;
        _durabilityType = DurabilityType.Part;
        _maxDurability = partData.MaxDurability;
        _currentDurability = Mathf.Clamp(currentDurability, 0f, _maxDurability);

        _isDestroyed = _currentDurability <= 0f;
    }
}

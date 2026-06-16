using UnityEngine;

public class PlayerInteractController : MonoBehaviour
{
    private PlayerInputHandler _inputHandler;
    private PlayerInventory _inventory;
    private Shop _shop;

    private PartsData _targetParts;
    private Currency _targetCurrency;

    private GameObject _target;



    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _inventory = GetComponent<PlayerInventory>();
        _shop = FindFirstObjectByType<Shop>();
    }

    private void Update()
    {
        if (_inputHandler.IsPickupPressed && (_targetParts != null || _targetCurrency != null))
        {
            _inputHandler.IsPickupPressed = false;

            PickupParts();
        }
    }

    // 아이템에 플레이어 몸이 닿았을 때, 해당 아이템이 공격 파츠인지 확인하고, 공격 파츠라면 타겟으로 설정
    private void OnTriggerEnter(Collider other)
    {
        
        if(other != null)
        {
            FireManager weapon = other.GetComponent<FireManager>();
            if (weapon != null)
            {
                _targetParts = weapon.AttackPartsData;
                _target = other.gameObject;
            }
        }

        Currency currency = other.GetComponent<Currency>();
        if (currency != null)
        {
            _targetCurrency = currency;
            _targetParts = null;
            _target = other.gameObject;
        }
    }

    // 아이템의 범위에서 벗어났을 때, 타겟을 초기화하여 더 이상 상호작용할 수 없도록 설정
    private void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            FireManager weapon = other.GetComponent<FireManager>();
            Currency currency = other.GetComponent<Currency>();

            if ((weapon != null || currency != null) && other.gameObject == _target)
            {
                _targetParts = null;
                _targetCurrency = null;
                _target = null;
            }
        }
    }

    // 타겟이 존재할 때, 아이템 줍기 함수
    // 아이템을 주웠을 때 추후 인벤토리로 옮겨지도록 처리할 예정
    // 현재는 그 이후 동작처리인 아이템 삭제만 진행
    private void PickupParts()
    {
        if (_target == null || _inventory == null) return;

        //if (_targetParts != null)
        //{
        //    _inventory.AddPart(_targetParts);
        //    if (_shop != null) _shop.CheckExistParts(_targetParts);
        //}

        if (_targetParts != null)
        {
            InventoryPartItem addedItem = _inventory.AddPart(_targetParts);

            if (addedItem != null)
            {
                DurabilityController durability =
                    _target.GetComponent<DurabilityController>();

                if (durability == null)
                    durability = _target.GetComponentInChildren<DurabilityController>();

                if (durability != null)
                {
                    addedItem.SetDurability(durability.CurrentDurability);

                    Debug.Log(
                        $"[PlayerInteract] 드랍 파츠 내구도 반영: " +
                        $"{_targetParts.PartsName} / {addedItem.CurrentDurability} / {addedItem.MaxDurability}"
                    );

                    _inventory.NotifyInventoryChanged();
                }
            }

            if (_shop != null)
                _shop.CheckExistParts(_targetParts);
        }

        else if (_targetCurrency != null)
        {
            string type = _targetCurrency.Data.CurrencyType.ToString();
            int finalAmount = 0;

            if (type == "Scrap")
            {
                finalAmount = Random.Range(10, 21); 
            }
            else if (type == "Gear")
            {
                finalAmount = 10;
            }

            if (_inventory.CurrencyList.ContainsKey(type))
            {
                _inventory.CurrencyList[type] += finalAmount;
                _inventory.AddPart(null); 
            }
        }
        Destroy(_target.gameObject);

        _targetParts = null;
        _target = null;
    }
}

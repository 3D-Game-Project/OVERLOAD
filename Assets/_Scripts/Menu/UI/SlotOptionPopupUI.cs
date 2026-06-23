using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotOptionPopupUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _equipOrUnequipButton;
    [SerializeField] private Button _repairButton;
    [SerializeField] private Button _dismantleButton;
    [SerializeField] private Button _dropButton; 

    [Header("Text")]
    [SerializeField] private TMP_Text _equipOrUnequipText;

    [Header("Popup")]
    [SerializeField] private GameObject _popupCloseArea;
    [SerializeField] private AttachTargetPopupUI _attachTargetPopup;
    [SerializeField] private Vector2 _offset = new Vector2(120f, -40f);

    [Header("References")]
    [SerializeField] private CorePartsController _corePartsController;
    [SerializeField] private PartHoverInfoPopupUI _hoverInfoPopup;
    [SerializeField] private PartEquipActionController _partEquipActionController;
    [SerializeField] private PartRepairActionController _partRepairActionController;

    [Header("Dismantle")]
    [Range(0f, 1f)] [SerializeField] private float _dismantleRefundRate = 0.2f;

    private RectTransform _rectTransform;

    private InventorySlot _currentSlot;
    private PartsData _currentPart;
    private PlayerInventory _inventory;
    private InventoryPartItem _currentPartItem;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_corePartsController == null)
            _corePartsController = FindFirstObjectByType<CorePartsController>();

        if (_equipOrUnequipButton != null)
            _equipOrUnequipButton.onClick.AddListener(OnEquipOrUnequipClicked);

        if (_repairButton != null)
            _repairButton.onClick.AddListener(OnRepairClicked);

        if (_dismantleButton != null)
            _dismantleButton.onClick.AddListener(OnDismantleClicked);

        if (_dropButton != null)
            _dropButton.onClick.AddListener(OnDropClicked);

        if (_hoverInfoPopup == null)
            _hoverInfoPopup = FindFirstObjectByType<PartHoverInfoPopupUI>(FindObjectsInactive.Include);

        if (_partEquipActionController == null)
            _partEquipActionController = FindFirstObjectByType<PartEquipActionController>();

        if (_partRepairActionController == null)
            _partRepairActionController = FindFirstObjectByType<PartRepairActionController>();

        //Close();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open(InventorySlot slot, InventoryPartItem partItem, PlayerInventory inventory)
    {
        if (slot == null || partItem == null || partItem.PartsData == null)
            return;

        _currentSlot = slot;
        _currentPartItem = partItem;
        _currentPart = partItem.PartsData;
        _inventory = inventory;

        if (_equipOrUnequipText != null)
        {
            bool isEquipped =
                _corePartsController != null &&
                _corePartsController.IsEquipped(_currentPartItem);

            _equipOrUnequipText.text = isEquipped ? "Unequip" : "Equip";
        }

        gameObject.SetActive(true);

        if (_popupCloseArea != null)
            _popupCloseArea.SetActive(true);

        MoveNextToSlot(slot);
    }

    public void Close()
    {
        _currentSlot = null;
        _currentPart = null;
        _inventory = null;
        _currentPartItem = null;

        if (_attachTargetPopup != null)
            _attachTargetPopup.Close();

        if (_hoverInfoPopup != null)
            _hoverInfoPopup.Hide();

        gameObject.SetActive(false);

        if (_popupCloseArea != null)
            _popupCloseArea.SetActive(false);
    }

    private void MoveNextToSlot(InventorySlot slot)
    {
        RectTransform slotRect = slot.transform as RectTransform;

        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform == null || slotRect == null)
            return;

        _rectTransform.position = slotRect.position + new Vector3(_offset.x, _offset.y, 0f);
    }

    private void OnEquipOrUnequipClicked()
    {
        if (_currentPart == null)
            return;

        if (IsAnyActionBusy())
            return;

        bool isEquipped = _corePartsController.IsEquipped(_currentPartItem);

        if (isEquipped)
        {
            if (_partEquipActionController == null)
            {
                Debug.LogWarning("[SlotOptionPopup] PartEquipActionController가 없습니다.");
                return;
            }

            AttachmentSlot equippedSlot = _corePartsController.FindEquippedSlot(_currentPartItem);

            if (equippedSlot == null)
            {
                Debug.LogWarning($"[SlotOptionPopup] 장착된 슬롯을 찾을 수 없습니다: {_currentPart?.name}");
                return;
            }

            bool started = _partEquipActionController.TryStartDetach(equippedSlot);

            if (started)
            {
                Debug.Log($"[SlotOptionPopup] 해제 작업 시작: {_currentPart?.name}");
                Close();
            }
            else
            {
                Debug.LogWarning($"[SlotOptionPopup] 해제 작업 시작 실패: {_currentPart?.name}");
            }

            return;
        }

        Debug.Log($"[SlotOptionPopup] 장착 버튼 클릭: {_currentPart.PartsName}");

        if (_attachTargetPopup == null)
        {
            Debug.LogWarning("AttachTargetPopupUI가 연결되지 않았습니다.");
            return;
        }

        _attachTargetPopup.Open(
            _rectTransform,
            _currentSlot,
            _currentPartItem,
            _inventory
        );
    }

    private void OnRepairClicked()
    {
        if (_currentPartItem == null || _currentPartItem.PartsData == null)
            return;

        if (IsAnyActionBusy())
            return;

        if (_partRepairActionController == null)
        {
            Debug.LogWarning("[SlotOptionPopup] PartRepairActionController가 없습니다.");
            return;
        }

        bool started = _partRepairActionController.TryStartRepair(_currentPartItem);

        if (started)
        {
            Debug.Log($"[SlotOptionPopup] 수리 작업 시작: {_currentPartItem.PartsData.PartsName}");
            Close();
        }
        else
        {
            Debug.LogWarning($"[SlotOptionPopup] 수리 작업 시작 실패: {_currentPartItem.PartsData.PartsName}");
        }
    }


    private void OnDismantleClicked()
    {
        if (_currentPartItem == null ||
            _currentPartItem.PartsData == null)
        {
            return;
        }

        if (_inventory == null)
        {
            Debug.LogWarning(
                "[SlotOptionPopup] PlayerInventory가 없습니다.");
            return;
        }

        if (_corePartsController != null &&
            _corePartsController.IsEquipped(_currentPartItem))
        {
            Debug.LogWarning(
                "[SlotOptionPopup] 장착 중인 파츠는 분해할 수 없습니다. " +
                "먼저 해제해주세요.");
            return;
        }

        if (IsAnyActionBusy())
            return;

        InventoryPartItem dismantledItem = _currentPartItem;
        PartsData part = dismantledItem.PartsData;

        CurrencyCost reward = new CurrencyCost
        {
            Gear = Mathf.FloorToInt(
                part.BuyCost.Gear * _dismantleRefundRate),

            Scrap = Mathf.FloorToInt(
                part.BuyCost.Scrap * _dismantleRefundRate)
        };

        bool removed = _inventory.RemovePartItem(dismantledItem);

        if (!removed)
        {
            Debug.LogWarning(
                $"[SlotOptionPopup] 분해할 파츠를 찾지 못했습니다: " +
                $"{part.PartsName}");
            return;
        }

        _inventory.AddCurrency(reward);

        Debug.Log(
            $"[SlotOptionPopup] {part.PartsName} 분해 완료\n" +
            $"획득 Gear: {reward.Gear}, Scrap: {reward.Scrap}"
        );

        Close();
    }

    private void OnDropClicked()
    {
        if (_currentPartItem == null || _currentPartItem.PartsData == null)
            return;

        if (_inventory == null)
        {
            Debug.LogWarning("[SlotOptionPopup] PlayerInventory가 없습니다.");
            return;
        }

        if (_corePartsController != null && _corePartsController.IsEquipped(_currentPartItem))
        {
            Debug.LogWarning("[SlotOptionPopup] 장착 중인 파츠는 바로 버릴 수 없습니다. 먼저 해제해주세요.");
            return;
        }

        if (IsAnyActionBusy())
            return;

        Vector3 dropPosition = _inventory.transform.position + _inventory.transform.forward * 2f;

        DropRuntime dropRuntime = new DropRuntime();
        dropRuntime.DropInventoryPart(_currentPartItem, dropPosition);

        bool removed = _inventory.RemovePartItem(_currentPartItem);

        if (removed)
        {
            Debug.Log($"[SlotOptionPopup] 파츠 버리기 완료: {_currentPartItem.PartsData.PartsName}");
        }

        Close();
    }

    private bool IsAnyActionBusy()
    {
        if (_partRepairActionController != null && _partRepairActionController.IsBusy)
        {
            Debug.LogWarning("[SlotOptionPopup] 수리 중에는 다른 작업을 할 수 없습니다.");
            return true;
        }

        if (_partEquipActionController != null && _partEquipActionController.IsBusy)
        {
            Debug.LogWarning("[SlotOptionPopup] 장착/해제 작업 중에는 다른 작업을 할 수 없습니다.");
            return true;
        }

        return false;
    }
}

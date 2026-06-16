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

        Close();
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

        MoveNextToSlot(slot);

        gameObject.SetActive(true);

        if (_popupCloseArea != null)
            _popupCloseArea.SetActive(true);
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

        if (_rectTransform == null || slotRect == null)
            return;

        _rectTransform.position = slotRect.position + new Vector3(_offset.x, _offset.y, 0f);
    }

    private void OnEquipOrUnequipClicked()
    {
        if (_currentPart == null)
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

        if (!_currentPartItem.CanRepair())
        {
            Debug.Log($"[SlotOptionPopup] 이미 최대 내구도입니다: {_currentPartItem.PartsData.PartsName}");
            Close();
            return;
        }

        _currentPartItem.RepairToFull();

        Debug.Log(
            $"[SlotOptionPopup] 수리 완료: {_currentPartItem.PartsData.PartsName} / " +
            $"{_currentPartItem.CurrentDurability} / {_currentPartItem.MaxDurability}"
        );

        // 장착 중인 파츠라면, 실제 장착된 프리팹의 DurabilityController도 같이 갱신
        if (_corePartsController != null)
        {
            AttachmentSlot equippedSlot = _corePartsController.FindEquippedSlot(_currentPartItem);

            if (equippedSlot != null && equippedSlot.AttachedObject != null)
            {
                DurabilityController[] durabilities =
                    equippedSlot.AttachedObject.GetComponentsInChildren<DurabilityController>(true);

                foreach (DurabilityController durability in durabilities)
                {
                    durability.InitializeFromInventoryItem(_currentPartItem);
                }
            }
        }

        _inventory?.NotifyInventoryChanged();

        Close();
    }

    private void OnDismantleClicked()
    {
        if (_currentPart == null)
            return;

        Debug.Log($"[SlotOptionPopup] 분해 버튼 클릭: {_currentPart.PartsName}");

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
}

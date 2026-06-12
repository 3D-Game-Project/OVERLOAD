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

    private RectTransform _rectTransform;

    private InventorySlot _currentSlot;
    private PartsData _currentPart;
    private PlayerInventory _inventory;

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

        Close();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open(InventorySlot slot, PartsData part, PlayerInventory inventory)
    {
        if (slot == null || part == null)
            return;

        _currentSlot = slot;
        _currentPart = part;
        _inventory = inventory;

        if (_equipOrUnequipText != null)
        {
            bool isEquipped =
                _corePartsController != null &&
                _corePartsController.IsEquipped(_currentPart);
            Debug.LogWarning($"isEquipped : {isEquipped}");

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

    // 추후 추가 예정
    private void OnEquipOrUnequipClicked()
    {
        if (_currentPart == null)
            return;

        bool isEquipped = _corePartsController.IsEquipped(_currentPart);

        if (isEquipped)
        {
            bool success = _corePartsController.DetachPart(_currentPart);

            if (success)
            {
                Debug.Log($"[SlotOptionPopup] Unequip 완료: {_currentPart.name}");
                _inventory?.NotifyInventoryChanged();
                Close();
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
            _currentPart,
            _inventory
        );
    }

    private void OnRepairClicked()
    {
        if (_currentPart == null)
            return;

        Debug.Log($"[SlotOptionPopup] 수리 버튼 클릭: {_currentPart.PartsName}");

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
        if (_currentPart == null)
            return;

        Debug.Log($"[SlotOptionPopup] 버리기 버튼 클릭: {_currentPart.PartsName}");

        Close();
    }
}

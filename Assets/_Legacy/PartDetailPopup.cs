using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartDetailPopup : MonoBehaviour
{
    [SerializeField] private Image _partIcon;
    [SerializeField] private TextMeshProUGUI _nameText;

    [Header("파츠 정보, 타입에 관계없이 추가되도록 처리")]
    [SerializeField] private TextMeshProUGUI _statText1;
    [SerializeField] private TextMeshProUGUI _statText2;
    [SerializeField] private TextMeshProUGUI _statText3;
    [SerializeField] private TextMeshProUGUI _statText4;
    [SerializeField] private TextMeshProUGUI _statText5;

    [Header("부착 부위 버튼")]
    [SerializeField] private Button[] _attachSlotButtons = new Button[6];

    [Header("버튼 순서에 대응되는 AttachmentSlot SlotId")]
    [SerializeField]
    private string[] _slotIds = new string[6]
    {
        "TopHardPoint",
        "LeftHardPoint",
        "RightHardPoint",
        "Backpack",
        "LegModule",
        ""
    };

    [Header("파츠 장착 처리")]
    [SerializeField] private PartEquipActionController _partEquipActionController;

    [SerializeField] private Button _closeButton;

    private PartsData _selectedPart;
    private PlayerInventory _inventory;

    private PartsData _pendingRemovePart;
    private PlayerInventory _pendingInventory;
    private bool _waitingEquipResult;

    private void Awake()
    {
        if (_partEquipActionController == null)
            _partEquipActionController = FindFirstObjectByType<PartEquipActionController>();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(ClosePopup);

        for (int i = 0; i < _attachSlotButtons.Length; i++)
        {
            if (_attachSlotButtons[i] == null)
                continue;

            int slotIndex = i;
            _attachSlotButtons[i].onClick.AddListener(() => OnAttachSlotClicked(slotIndex));
        }
    }

    private void OnEnable()
    {
        if (_partEquipActionController == null)
            _partEquipActionController = FindFirstObjectByType<PartEquipActionController>();

        if (_partEquipActionController != null)
        {
            _partEquipActionController.OnActionFinished -= HandleEquipActionFinished;
            _partEquipActionController.OnActionFinished += HandleEquipActionFinished;
        }
    }

    private void OnDisable()
    {
        if (_partEquipActionController != null && !_waitingEquipResult)
        {
            _partEquipActionController.OnActionFinished -= HandleEquipActionFinished;
        }
    }

    public void OpenPopup(PartsData part, PlayerInventory inventory)
    {
        if (part == null || inventory == null)
            return;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        _selectedPart = part;
        _inventory = inventory;

        if (_nameText != null)
            _nameText.text = part.PartsName;

        if (_partIcon != null)
        {
            _partIcon.sprite = part.PartsImage;
            _partIcon.gameObject.SetActive(part.PartsImage != null);
        }

        ClearStatTexts();
        RefreshStatTexts(part);
        RefreshSlotButtons(part);

        gameObject.SetActive(true);
    }

    private void RefreshStatTexts(PartsData part)
    {
        switch (part.PartsType)
        {
            case PartsType.Attack:
                AttackPartsData attackData = part as AttackPartsData;

                if (attackData != null)
                {
                    if (_statText1 != null) _statText1.text = $"Damage: <color=#FFCC00>{attackData.Damage}</color>";
                    if (_statText2 != null) _statText2.text = $"Range: <color=#FFCC00>{attackData.Range}m</color>";
                    if (_statText3 != null) _statText3.text = $"Cooldown: <color=#FFCC00>{attackData.FireCooldown}s</color>";
                    if (_statText5 != null) _statText5.text = $"Power Cost: <color=#FF3333>{attackData.FirePowerCost}</color>";
                }
                break;

            case PartsType.Leg:
                if (_statText1 != null) _statText1.text = "Type: <color=#FFCC00>Leg</color>";
                break;

            case PartsType.Booster:
                if (_statText1 != null) _statText1.text = "Type: <color=#FFCC00>Booster</color>";
                break;

            case PartsType.Armor:
                if (_statText1 != null) _statText1.text = "Type: <color=#FFCC00>Armor</color>";
                break;

            case PartsType.Utility:
                if (_statText1 != null) _statText1.text = "Type: <color=#FFCC00>Utility</color>";
                break;
        }
    }

    private void RefreshSlotButtons(PartsData part)
    {
        for (int i = 0; i < _attachSlotButtons.Length; i++)
        {
            Button button = _attachSlotButtons[i];

            if (button == null)
                continue;

            bool hasSlotId =
                _slotIds != null &&
                i < _slotIds.Length &&
                !string.IsNullOrEmpty(_slotIds[i]);

            button.interactable = hasSlotId && _partEquipActionController != null;
        }
    }

    private void ClearStatTexts()
    {
        if (_statText1 != null) _statText1.text = "";
        if (_statText2 != null) _statText2.text = "";
        if (_statText3 != null) _statText3.text = "";
        if (_statText4 != null) _statText4.text = "";
        if (_statText5 != null) _statText5.text = "";
    }

    private void OnAttachSlotClicked(int slotIndex)
    {
        if (_selectedPart == null || _inventory == null)
            return;

        if (_partEquipActionController == null)
        {
            Debug.LogWarning("[PartDetailPopup] PartEquipActionController가 없습니다.");
            return;
        }

        if (_slotIds == null || slotIndex < 0 || slotIndex >= _slotIds.Length)
        {
            Debug.LogWarning($"[PartDetailPopup] 잘못된 슬롯 인덱스입니다: {slotIndex}");
            return;
        }

        string slotId = _slotIds[slotIndex];

        if (string.IsNullOrEmpty(slotId))
        {
            Debug.LogWarning($"[PartDetailPopup] {slotIndex}번 버튼에 SlotId가 설정되어 있지 않습니다.");
            return;
        }

        Debug.Log(
            $"[PartDetailPopup] 파츠 장착 요청 / Part: {_selectedPart.PartsName} / Slot: {slotId}"
        );

        bool started =
            _partEquipActionController.TryStartReplace(_selectedPart, slotId);

        if (!started)
        {
            Debug.LogWarning("[PartDetailPopup] 파츠 장착 작업을 시작하지 못했습니다.");
            return;
        }

        _pendingRemovePart = _selectedPart;
        _pendingInventory = _inventory;
        _waitingEquipResult = true;

        ClosePopup();
    }

    private void HandleEquipActionFinished(bool success)
    {
        if (!_waitingEquipResult)
            return;

        _waitingEquipResult = false;

        if (_partEquipActionController != null)
        {
            _partEquipActionController.OnActionFinished -= HandleEquipActionFinished;
        }

        if (!success)
        {
            Debug.LogWarning("[PartDetailPopup] 파츠 장착 작업 실패. 인벤토리에서 제거하지 않습니다.");

            _pendingRemovePart = null;
            _pendingInventory = null;
            return;
        }

        if (_pendingInventory != null && _pendingRemovePart != null)
        {
            // PlayerInventory의 실제 함수명에 맞춰 수정 필요
            _pendingInventory.RemovePart(_pendingRemovePart);

            Debug.Log($"[PartDetailPopup] 장착 성공으로 인벤토리에서 제거: {_pendingRemovePart.PartsName}");
        }

        _pendingRemovePart = null;
        _pendingInventory = null;
    }

    public void ClosePopup()
    {
        _selectedPart = null;
        _inventory = null;
        gameObject.SetActive(false);
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttachTargetPopupUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private Vector2 _offset = new Vector2(160f, 0f);

    [Header("References")]
    [SerializeField] private CorePartsController _corePartsController;
    [SerializeField] private SlotOptionPopupUI _slotOptionPopup;

    [Header("Slot Button UI")]
    [SerializeField] private Transform _slotButtonParent;
    [SerializeField] private Button _targetSlotButtonPrefab;

    [Header("Auto Size")]
    [SerializeField] private float _buttonHeight = 39f;
    [SerializeField] private float _buttonSpacing = 8f;
    [SerializeField] private float _verticalPadding = 10f;
    [SerializeField] private float _minHeight = 40f;
    [SerializeField] private float _maxHeight = 500f;

    private readonly List<Button> _createdButtons = new List<Button>();

    private RectTransform _rectTransform;

    private PartsData _currentPart;
    private InventorySlot _currentSlot;
    private PlayerInventory _inventory;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_corePartsController == null)
            _corePartsController = FindFirstObjectByType<CorePartsController>();

        Close();
    }

    public void Open(
        RectTransform baseRect,
        InventorySlot slot,
        PartsData part,
        PlayerInventory inventory)
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform == null)
        {
            Debug.LogWarning("[AttachTargetPopup] RectTransform을 찾을 수 없습니다.");
            return;
        }

        if (baseRect == null)
        {
            Debug.LogWarning("[AttachTargetPopup] 기준 RectTransform이 없습니다.");
            return;
        }

        if (slot == null)
        {
            Debug.LogWarning("[AttachTargetPopup] 선택된 InventorySlot이 없습니다.");
            return;
        }

        if (part == null)
        {
            Debug.LogWarning("[AttachTargetPopup] 선택된 PartsData가 없습니다.");
            return;
        }

        _currentSlot = slot;
        _currentPart = part;
        _inventory = inventory;

        MoveNextToBase(baseRect);

        gameObject.SetActive(true);

        string partName = _currentPart != null ? _currentPart.name : "Unknown Part";
        Debug.Log($"[AttachTargetPopup] 장착 위치 선택 팝업 열림: {partName}");

        BuildSlotButtons();
    }

    public void Close()
    {
        _currentSlot = null;
        _currentPart = null;
        _inventory = null;

        ClearSlotButtons();

        gameObject.SetActive(false);
    }

    private void MoveNextToBase(RectTransform baseRect)
    {
        if (_rectTransform == null || baseRect == null)
            return;

        Vector3[] corners = new Vector3[4];
        baseRect.GetWorldCorners(corners);

        // corners[2] = 오른쪽 위 모서리
        Vector3 baseTopRight = corners[2];

        _rectTransform.position = baseTopRight + new Vector3(_offset.x, _offset.y, 0f);
    }

    private void BuildSlotButtons()
    {
        ClearSlotButtons();

        if (_corePartsController == null)
        {
            Debug.LogWarning("[AttachTargetPopup] CorePartsController를 찾을 수 없습니다.");
            return;
        }

        if (_currentPart == null)
        {
            Debug.LogWarning("[AttachTargetPopup] 선택된 파츠가 없습니다.");
            return;
        }

        if (_slotButtonParent == null)
        {
            Debug.LogWarning("[AttachTargetPopup] SlotButtonParent가 연결되지 않았습니다.");
            return;
        }

        if (_targetSlotButtonPrefab == null)
        {
            Debug.LogWarning("[AttachTargetPopup] TargetSlotButtonPrefab이 연결되지 않았습니다.");
            return;
        }

        List<AttachmentSlot> compatibleSlots =
            _corePartsController.GetCompatibleSlots(_currentPart, false);

        if (compatibleSlots.Count == 0)
        {
            Debug.Log($"[AttachTargetPopup] {_currentPart.name} 장착 가능 슬롯 없음");
            return;
        }

        foreach (AttachmentSlot slot in compatibleSlots)
        {
            if (slot == null)
                continue;

            Button button = Instantiate(_targetSlotButtonPrefab, _slotButtonParent);
            button.gameObject.SetActive(true);

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);
            if (buttonText != null)
                buttonText.text = slot.SlotId;

            AttachmentSlot capturedSlot = slot;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[AttachTargetPopup] 슬롯 버튼 클릭: {capturedSlot.SlotId}");
                TryAttachToSlot(capturedSlot);
            });

            _createdButtons.Add(button);
        }

        if (compatibleSlots.Count == 0)
        {
            Debug.Log($"[AttachTargetPopup] {_currentPart.name} 장착 가능 슬롯 없음");
            ResizePopupByButtonCount(0);
            return;
        }

        ResizePopupByButtonCount(_createdButtons.Count);
    }

    private void ClearSlotButtons()
    {
        for (int i = 0; i < _createdButtons.Count; i++)
        {
            if (_createdButtons[i] != null)
                Destroy(_createdButtons[i].gameObject);
        }

        _createdButtons.Clear();
    }

    private void ResizePopupByButtonCount(int buttonCount)
    {
        if (_rectTransform == null)
            return;

        float height = _verticalPadding * 2f;

        if (buttonCount >0)
        {
            height += _buttonHeight * buttonCount;
            height += _buttonSpacing * (buttonCount - 1);
        }

        height = Mathf.Clamp(height, _minHeight, _maxHeight);

        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void TryAttachToSlot(AttachmentSlot targetSlot)
    {
        if (_corePartsController == null)
        {
            Debug.LogWarning("[AttachTargetPopup] CorePartsController가 없습니다.");
            return;
        }

        if (_currentPart == null)
        {
            Debug.LogWarning("[AttachTargetPopup] 선택된 파츠가 없습니다.");
            return;
        }

        if (targetSlot == null)
        {
            Debug.LogWarning("[AttachTargetPopup] 선택된 슬롯이 없습니다.");
            return;
        }

        bool success;

        if (targetSlot.HasPart)
        {
            Debug.Log($"[AttachTargetPopup] 교체 시도: {_currentPart.name} → {targetSlot.SlotId}");
            success = _corePartsController.ReplacePart(_currentPart, targetSlot);
        }
        else
        {
            Debug.Log($"[AttachTargetPopup] 장착 시도: {_currentPart.name} → {targetSlot.SlotId}");
            success = _corePartsController.AttachPart(_currentPart, targetSlot);
        }

        if (success)
        {
            Debug.Log($"[AttachTargetPopup] 장착/교체 완료: {_currentPart.name} → {targetSlot.SlotId}");

            _inventory?.NotifyInventoryChanged();
            MechPreviewStudio.Instance.RefreshPreview();

            if (_slotOptionPopup != null)
                _slotOptionPopup.Close();

            else
                Close();
        }
        else
        {
            Debug.LogWarning($"[AttachTargetPopup] 장착/교체 실패: {_currentPart.name} → {targetSlot.SlotId}");
        }
    }
}

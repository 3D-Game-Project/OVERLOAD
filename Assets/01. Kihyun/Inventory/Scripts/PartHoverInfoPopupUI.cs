using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartHoverInfoPopupUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _partNameText;
    [SerializeField] private TMP_Text _partTypeText;
    [SerializeField] private TMP_Text _partLoadText;
    [SerializeField] private TMP_Text _partDescriptionText;

    private bool _isPinned;
    private PartsData _currentPart;

    private void Awake()
    {
        Hide();
    }

    // 기존 Show 호출이 있어도 깨지지 않도록 유지
    public void Show(PartsData part)
    {
        ShowTemporary(part);
    }

    // 마우스 hover용
    public void ShowTemporary(PartsData part)
    {
        if (_isPinned)
            return;

        ShowInternal(part);
    }

    // 슬롯 클릭 고정용
    public void ShowPinned(PartsData part)
    {
        _isPinned = true;
        ShowInternal(part);
    }

    // 마우스가 슬롯에서 나갔을 때 사용
    public void HideIfNotPinned()
    {
        if (_isPinned)
            return;

        Hide();
    }

    // 강제 닫기
    public void Hide()
    {
        _isPinned = false;
        _currentPart = null;
        gameObject.SetActive(false);
    }

    private void ShowInternal(PartsData part)
    {
        if (part == null)
        {
            Hide();
            return;
        }

        _currentPart = part;

        if (_partNameText != null)
            _partNameText.text = part.PartsName;

        if (_partTypeText != null)
            _partTypeText.text = $"Type : {part.PartsType}";

        if (_partLoadText != null)
            _partLoadText.text = $"Load : {part.RequiredLoad}";

        if (_partDescriptionText != null)
            _partDescriptionText.text = "";

        gameObject.SetActive(true);
    }
}
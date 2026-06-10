using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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

    [SerializeField] private Button _closeButton; 

    private PartsData _selectedPart;
    private PlayerInventory _inventory;

    private void Awake()
    {
        if (_closeButton != null) _closeButton.onClick.AddListener(ClosePopup);

        for (int i = 0; i < _attachSlotButtons.Length; i++)
        {
            if (_attachSlotButtons[i] == null) continue;

            int slotIndex = i; 
            _attachSlotButtons[i].onClick.AddListener(() => OnAttachSlotClicked(slotIndex));
        }
    }

    public void OpenPopup(PartsData part, PlayerInventory inventory)
    {
        if (part == null || inventory == null) return;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        _selectedPart = part;
        _inventory = inventory;

        if (_nameText != null) _nameText.text = part.PartsName;
        if (_partIcon != null)
        {
            _partIcon.sprite = part.PartsImage;
            _partIcon.gameObject.SetActive(part.PartsImage != null);
        }

        ClearStatTexts();

        switch (part.PartsType)
        {
            case PartsType.Attack:
                AttackPartsData attackData = part as AttackPartsData;
                if (attackData != null)
                {
                    if (_statText1 != null) _statText1.text = $"Damage: <color=#FFCC00>{attackData.Damage}</color>";
                    if (_statText2 != null) _statText2.text = $"Range: <color=#FFCC00>{attackData.Range}m</color>";
                    if (_statText3 != null) _statText3.text = $"Cooldown: <color=#FFCC00>{attackData.FireCooldown}s</color>";
                    if (_statText4 != null) _statText4.text = $"Magazine: <color=#FFCC00>{attackData.MaxMagazineSize} Rnds</color>";
                    if (_statText5 != null) _statText5.text = $"Power Cost: <color=#FF3333>{attackData.FirePowerCost}</color>";
                }
                break;

            case PartsType.Leg:
                //if (_statText1 != null) _statText1.text = $"";
                //if (_statText2 != null) _statText2.text = $"";
                break;

            case PartsType.Booster:
                //if (_statText1 != null) _statText1.text = $"";
                //if (_statText2 != null) _statText2.text = $"";
                break;

            case PartsType.Armor:
                //if (_statText1 != null) _statText1.text = $"";
                //if (_statText2 != null) _statText2.text = $"";
                break;

            case PartsType.Utility:
                //if (_statText1 != null) _statText1.text = $"";
                //if (_statText2 != null) _statText2.text = $"";
                break;
        }

        gameObject.SetActive(true);
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
        if (_selectedPart == null || _inventory == null) return;

        // 여기에 파츠 부착에 대한 처리 진행하시면 됩니다.
        // 파츠 장착 이후 RemovePart호출처리해주시면 됩니다.
        ClosePopup();
    }

    private void ClosePopup()
    {
        _selectedPart = null;
        _inventory = null;
        gameObject.SetActive(false);
    }
}
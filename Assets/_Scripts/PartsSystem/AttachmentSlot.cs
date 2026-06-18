using UnityEngine;

public class AttachmentSlot : MonoBehaviour
{
    [Header("Slot Info")]
    [SerializeField] private string _slotId;
    [SerializeField] private PartsType[] _allowedPartsTypes;
    [SerializeField] private bool _mirrorVisualX;

    [Header("Attach Point")]
    [SerializeField] private Transform _attachPoint;

    private GameObject _attachedObject;

    public string SlotId => _slotId;

    public Transform AttachPoint => _attachPoint != null ? _attachPoint : transform;

    public bool MirrorVisualX => _mirrorVisualX;

    public GameObject AttachedObject => _attachedObject;

    // 실제로 파츠가 장착되어 있는지 확인
    public bool HasPart => _attachedObject != null;

    // 이 슬롯이 해당 파츠 타입을 허용하는지만 확인
    public bool AllowsPartType(PartsData partsData)
    {
        if (partsData == null)
            return false;

        for (int i = 0; i < _allowedPartsTypes.Length; i++)
        {
            if (_allowedPartsTypes[i] == partsData.PartsType)
                return true;
        }

        return false;
    }

    // 실제 장착 가능 여부 확인
    // 비어 있고 + 타입이 맞아야 장착 가능
    public bool CanAttach(PartsData partsData)
    {
        if (HasPart)
            return false;

        return AllowsPartType(partsData);
    }

    public void SetAttachedObject(GameObject attachedObject)
    {
        _attachedObject = attachedObject;
    }

    public void Clear()
    {
        _attachedObject = null;
    }
}
using UnityEngine;

public class AttachmentSlot : MonoBehaviour
{
    [Header("Slot Info")]
    [SerializeField] private string _slotId;
    [SerializeField] private PartsType[] _allowedPartsTypes;

    [Header("Attach Point")]
    [SerializeField] private Transform _attachPoint;

    private GameObject _attachedObject;

    public string SlotId => _slotId;
    public Transform AttachPoint => _attachPoint != null ? _attachPoint : transform;
    public GameObject AttachedObject => _attachedObject;
    public bool HasPart => _attachPoint != null;

    public bool CanAttach(PartsData partsData)
    {
        if (partsData == null)
            return false;

        if (HasPart)
            return false;

        for (int i = 0; i<_allowedPartsTypes.Length; i++)
        {
            if (_allowedPartsTypes[i] == partsData.PartsType)
                return true;
        }

        return false;
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

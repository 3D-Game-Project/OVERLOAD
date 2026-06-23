using UnityEngine;

public abstract class PartBehaviour : MonoBehaviour, IPart
{
    protected PartsData _data;
    protected CorePartContext _context;
    protected AttachmentSlot _slot;
    protected DurabilityController _durability;

    public PartsData Data => _data;
    public CorePartContext Context => _context;
    public AttachmentSlot Slot => _slot;

    public bool IsOperational =>
        isActiveAndEnabled &&
        _durability != null &&
        !_durability.IsDestroyed &&
        _durability.CurrentDurability > 0f;

    public virtual void Initialize(PartsData data, CorePartContext context)
    {
        _data = data;
        _context = context;

        _durability =
            GetComponent<DurabilityController>();

        if (_durability == null)
        {
            _durability =
                GetComponentInChildren<DurabilityController>(true);
        }

        if (_durability == null)
        {
            Debug.LogError(
                $"{gameObject.name}에 " +
                "DurabilityController가 없습니다."
            );
        }
    }

    public virtual void OnAttached(AttachmentSlot slot)
    {
        _slot = slot;
    }

    public virtual void OnDetached()
    {
    }

    protected bool IsInitialized()
    {
        return _data != null && _context != null;
    }

    protected bool TryGetData<T>(out T typedData) where T : PartsData
    {
        typedData = _data as T;
        return typedData != null;
    }
}
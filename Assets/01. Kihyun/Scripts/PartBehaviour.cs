using UnityEngine;

public abstract class PartBehaviour : MonoBehaviour, IPart
{
    protected PartsData _data;
    protected CorePartContext _context;
    protected AttachmentSlot _slot;

    public PartsData Data => _data;
    public CorePartContext Context => _context;
    public AttachmentSlot Slot => _slot;

    public virtual void Initialize(PartsData data, CorePartContext context)
    {
        _data = data;
        _context = context;
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
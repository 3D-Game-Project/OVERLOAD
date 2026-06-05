using UnityEngine;

public abstract class CorePartBehaviour : MonoBehaviour, IPart
{
    protected PartsData _data;
    protected CorePartContext _context;

    public PartsData Data => _data;

    public virtual void Initialize(PartsData data, CorePartContext context)
    {
        _data = data;
        _context = context;
    }

    public virtual void OnAttached()
    {
    }

    public virtual void OnDetached()
    {
    }
}
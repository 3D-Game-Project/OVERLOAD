public interface IPart
{
    PartsData Data { get; }
    CorePartContext Context { get; }
    AttachmentSlot Slot { get; }

    void Initialize(PartsData data, CorePartContext context);
    void OnAttached(AttachmentSlot slot);
    void OnDetached();
}
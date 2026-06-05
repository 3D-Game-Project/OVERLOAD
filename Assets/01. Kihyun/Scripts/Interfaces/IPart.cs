public interface IPart
{
    PartsData Data { get; }

    void Initialize(PartsData data, CorePartContext context);
    void OnAttached();
    void OnDetached();
}
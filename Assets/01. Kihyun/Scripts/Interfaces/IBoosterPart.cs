public interface IBoosterPart : IPart
{
    void HandleBooster(BoosterCommand command, float deltaTime);
    void StopBooster();
}
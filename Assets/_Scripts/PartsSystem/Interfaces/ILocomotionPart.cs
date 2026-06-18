public interface ILocomotionPart : IPart
{
    void HandleLocomotion(LocomotionCommand command, float deltaTime);
    void StopLocomotion();
}
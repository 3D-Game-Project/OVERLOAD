using UnityEngine;

public class CorePartContext
{
    public Transform CoreTransform { get; }
    public CorePowerController PowerController { get; }
    public PlayerInputHandler InputHandler { get; }

    public CorePartContext(
        Transform coreTransform,
        CorePowerController powerController,
        PlayerInputHandler inputHandler)
    {
        CoreTransform = coreTransform;
        PowerController = powerController;
        InputHandler = inputHandler;
    }
}
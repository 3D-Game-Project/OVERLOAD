using UnityEngine;

public class CorePartContext
{
    public Transform CoreTransform { get; }
    public CoreEnergyController PowerController { get; }
    public PlayerInputHandler InputHandler { get; }

    public CorePartContext(
        Transform coreTransform,
        CoreEnergyController powerController,
        PlayerInputHandler inputHandler)
    {
        CoreTransform = coreTransform;
        PowerController = powerController;
        InputHandler = inputHandler;
    }
}
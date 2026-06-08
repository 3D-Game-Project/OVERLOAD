using UnityEngine;

public class CorePartContext
{
    public Transform OwnerRoot { get; }
    public PlayerInputHandler InputHandler { get; }
    public PlayerLocomotionMotor LocomotionMotor { get; }
    public MovementCoordinator MovementCoordinator { get; }
    public Transform CameraTransform { get; }
    public Transform CoreYawRoot { get; }
    public Transform LegYawRoot { get; }
    public CoreEnergyController EnergyController { get; }

    public CorePartContext(
        Transform ownerRoot,
        PlayerInputHandler inputHandler,
        PlayerLocomotionMotor locomotionMotor,
        MovementCoordinator movementCoordinator,
        Transform cameraTransform,
        Transform coreYawRoot,
        Transform legYawRoot,
        CoreEnergyController energyController)
    {
        OwnerRoot = ownerRoot;
        InputHandler = inputHandler;
        LocomotionMotor = locomotionMotor;
        MovementCoordinator = movementCoordinator;
        CameraTransform = cameraTransform;
        CoreYawRoot = coreYawRoot;
        LegYawRoot = legYawRoot;
        EnergyController = energyController;
    }
}
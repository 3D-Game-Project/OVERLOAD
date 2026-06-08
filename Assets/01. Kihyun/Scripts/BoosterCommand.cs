using UnityEngine;

public struct BoosterCommand
{
    public Vector3 MoveDirection { get; }
    public Vector3 LookDirection { get; }

    public bool JumpPressed { get; }
    public bool JumpHeld { get; }
    public bool BoostHeld { get; }

    public BoosterCommand(
        Vector3 moveDirection,
        Vector3 lookDirection,
        bool jumpPressed,
        bool jumpHeld,
        bool boostHeld)
    {
        MoveDirection = moveDirection;
        LookDirection = lookDirection;

        JumpPressed = jumpPressed;
        JumpHeld = jumpHeld;
        BoostHeld = boostHeld;
    }
}
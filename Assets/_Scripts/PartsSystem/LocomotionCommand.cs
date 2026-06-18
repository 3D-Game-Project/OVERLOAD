using UnityEngine;

public struct LocomotionCommand
{
    public Vector3 MoveDirection { get; }
    public Vector3 LookDirection { get; }
    public bool HasMoveInput { get; }

    public LocomotionCommand(
        Vector3 moveDirection,
        Vector3 lookDirection,
        bool hasMoveInput)
    {
        MoveDirection = moveDirection;
        LookDirection = lookDirection;
        HasMoveInput = hasMoveInput;
    }
}
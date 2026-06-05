using UnityEngine;

public interface ILocomotionPart : IPart
{
    void Move(Vector3 moveDirection);
}

using UnityEngine;

public interface ITargetedAttackPart : IPart
{
    bool TryFireAt(
        Vector3 targetPoint,
        bool consumeOverheat
    );
}
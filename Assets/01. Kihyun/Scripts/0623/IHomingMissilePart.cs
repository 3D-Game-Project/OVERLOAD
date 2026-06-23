using System.Collections.Generic;
using UnityEngine;

public interface IHomingMissilePart : IPart
{
    HomingMissilePartsData MissileData { get; }

    IReadOnlyList<Transform> LaunchPoints { get; }
}
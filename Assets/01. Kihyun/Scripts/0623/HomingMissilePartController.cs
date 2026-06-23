using System.Collections.Generic;
using UnityEngine;

public class HomingMissilePartController :
    PartBehaviour,
    IHomingMissilePart
{
    [Header("Launch Points")]
    [SerializeField]
    private List<Transform> _launchPoints = new();

    private HomingMissilePartsData _missileData;

    public HomingMissilePartsData MissileData =>
        _missileData;

    public IReadOnlyList<Transform> LaunchPoints =>
        _launchPoints;

    public override void Initialize(
        PartsData data,
        CorePartContext context)
    {
        base.Initialize(data, context);

        if (!TryGetData(out _missileData))
        {
            Debug.LogError(
                $"{gameObject.name}에는 " +
                "HomingMissilePartsData가 필요합니다."
            );

            return;
        }

        _launchPoints.RemoveAll(
            point => point == null
        );

        Debug.Log(
            $"유도 미사일 모듈 초기화: " +
            $"{_missileData.PartsName} / " +
            $"발사구 {_launchPoints.Count}개"
        );
    }
}
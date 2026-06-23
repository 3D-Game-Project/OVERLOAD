using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHomingMissilePattern :
    BossAttackPatternBase
{
    [Header("References")]
    [SerializeField] private CorePartsController _corePartsController;

    [Header("Pattern")]
    [SerializeField] private float _prepareDuration = 0.7f;
    [SerializeField] private int _volleyCount = 3;
    [SerializeField] private float _volleyInterval = 0.5f;
    [SerializeField] private float _recoveryDuration = 1f;

    [Header("Target")]
    [SerializeField] private LayerMask _targetLayer;

    private List<IHomingMissilePart> _launchers = new();

    public override BossAttackPatternType PatternType =>
        BossAttackPatternType.HomingMissile;

    public override bool CanRunWhileMoving => true;

    private void Awake()
    {
        if (_corePartsController == null)
        {
            _corePartsController =
                GetComponentInParent<CorePartsController>();
        }
    }

    protected override bool CheckRequirements(Transform target)
    {
        if (_corePartsController == null)
        {
            Debug.LogWarning(
                "[미사일 조건] CorePartsController 없음"
            );

            return false;
        }

        List<IHomingMissilePart> launchers =
            _corePartsController
                .GetOperationalParts<IHomingMissilePart>();

        Debug.Log(
            $"[미사일 조건] 등록된 발사 모듈: " +
            $"{launchers.Count}개"
        );

        foreach (IHomingMissilePart launcher in launchers)
        {
            bool operational =
                launcher != null &&
                launcher.IsOperational;

            bool hasData =
                launcher?.MissileData != null;

            bool hasPrefab =
                hasData &&
                launcher.MissileData.MissilePrefab != null;

            int pointCount =
                launcher?.LaunchPoints?.Count ?? 0;

            Debug.Log(
                $"[미사일 조건] " +
                $"Operational: {operational}, " +
                $"Data: {hasData}, " +
                $"Prefab: {hasPrefab}, " +
                $"LaunchPoints: {pointCount}"
            );

            if (operational &&
                hasData &&
                hasPrefab &&
                pointCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnPatternStarted(Transform target)
    {
        _launchers = _corePartsController
            .GetOperationalParts<IHomingMissilePart>();
    }

    protected override IEnumerator ExecutePattern(Transform target)
    {
        yield return WaitPatternTime(
            _prepareDuration,
            target
        );

        if (ShouldStop(target))
            yield break;

        for (int volley = 0;
             volley < _volleyCount;
             volley++)
        {
            if (!HasUsableLauncher())
            {
                RequestCancel();
                yield break;
            }

            foreach (IHomingMissilePart launcher in _launchers)
            {
                if (!IsUsableLauncher(launcher))
                    continue;

                HomingMissilePartsData data =
                    launcher.MissileData;

                foreach (Transform launchPoint
                         in launcher.LaunchPoints)
                {
                    if (ShouldStop(target))
                        yield break;

                    if (!launcher.IsOperational)
                        break;

                    if (launchPoint == null)
                        continue;

                    LaunchMissile(
                        launcher,
                        launchPoint,
                        target
                    );

                    yield return WaitPatternTime(
                        data.LaunchInterval,
                        target
                    );

                    if (ShouldStop(target))
                        yield break;
                }
            }

            if (volley < _volleyCount - 1)
            {
                yield return WaitPatternTime(
                    _volleyInterval,
                    target
                );

                if (ShouldStop(target))
                    yield break;
            }
        }

        yield return WaitPatternTime(
            _recoveryDuration,
            target
        );
    }

    protected override void OnPatternEnded(bool wasCancelled)
    {
        _launchers.Clear();
    }

    private void LaunchMissile(
        IHomingMissilePart launcher,
        Transform launchPoint,
        Transform target)
    {
        HomingMissilePartsData data =
            launcher.MissileData;

        GameObject missileObject = Instantiate(
            data.MissilePrefab,
            launchPoint.position,
            launchPoint.rotation
        );

        if (!missileObject.TryGetComponent(
                out HomingMissileProjectile missile))
        {
            Debug.LogError(
                $"{missileObject.name}에 " +
                "HomingMissileProjectile이 없습니다."
            );

            Destroy(missileObject);
            return;
        }

        missile.Launch(
            target,
            _corePartsController.transform,
            data.Damage,
            _targetLayer,
            launchPoint.forward
        );
    }

    private bool HasUsableLauncher()
    {
        foreach (IHomingMissilePart launcher in _launchers)
        {
            if (IsUsableLauncher(launcher))
                return true;
        }

        return false;
    }

    private bool IsUsableLauncher(
        IHomingMissilePart launcher)
    {
        return
            launcher != null &&
            launcher.IsOperational &&
            launcher.MissileData != null &&
            launcher.MissileData.MissilePrefab != null &&
            launcher.LaunchPoints != null &&
            launcher.LaunchPoints.Count > 0;
    }

    private IEnumerator WaitPatternTime(
        float duration,
        Transform target)
    {
        float timer = 0f;

        while (timer < duration)
        {
            if (ShouldStop(target))
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
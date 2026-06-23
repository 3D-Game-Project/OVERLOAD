using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGunAttackPattern : BossAttackPatternBase
{
    [Header("References")]
    [SerializeField] private CorePartsController _corePartsController;
    [SerializeField] private BossAimController _aimController;

    [Header("Pattern Timing")]
    [SerializeField] private float _aimDuration = 1.5f;
    [SerializeField] private float _fireDuration = 3f;
    [SerializeField] private float _recoveryDuration = 0.75f;

    private List<ITargetedAttackPart> _weapons = new();

    public override BossAttackPatternType PatternType =>
        BossAttackPatternType.Gun;

    public override bool CanRunWhileMoving => true;

    private void Awake()
    {
        if (_corePartsController == null)
        {
            _corePartsController =
                GetComponentInParent<CorePartsController>();
        }

        if (_aimController == null)
        {
            _aimController =
                GetComponentInParent<BossAimController>();
        }
    }

    protected override bool CheckRequirements(Transform target)
    {
        if (_corePartsController == null)
            return false;

        if (_aimController == null)
            return false;

        return _corePartsController
            .HasOperationalPart<ITargetedAttackPart>();
    }

    protected override void OnPatternStarted(Transform target)
    {
        _weapons = _corePartsController
            .GetOperationalParts<ITargetedAttackPart>();

        _aimController.StartAiming(target);
    }

    protected override IEnumerator ExecutePattern(Transform target)
    {
        // 조준 단계
        float aimTimer = 0f;

        while (aimTimer < _aimDuration)
        {
            if (ShouldStop(target))
                yield break;

            if (!HasOperationalWeapon())
            {
                RequestCancel();
                yield break;
            }

            aimTimer += Time.deltaTime;
            yield return null;
        }

        // 발사 단계
        float fireTimer = 0f;

        while (fireTimer < _fireDuration)
        {
            if (ShouldStop(target))
                yield break;

            if (!HasOperationalWeapon())
            {
                RequestCancel();
                yield break;
            }

            FireOperationalWeapons();

            fireTimer += Time.deltaTime;
            yield return null;
        }

        // 후딜레이에는 조준선을 숨긴다.
        _aimController.StopAiming();

        float recoveryTimer = 0f;

        while (recoveryTimer < _recoveryDuration)
        {
            if (ShouldStop(target))
                yield break;

            recoveryTimer += Time.deltaTime;
            yield return null;
        }
    }

    protected override void OnPatternEnded(bool wasCancelled)
    {
        _aimController?.StopAiming();
        _weapons.Clear();
    }

    private void FireOperationalWeapons()
    {
        Vector3 aimPoint =
            _aimController.GetAimPoint();

        foreach (ITargetedAttackPart weapon in _weapons)
        {
            if (weapon == null)
                continue;

            if (!weapon.IsOperational)
                continue;

            // 보스이므로 오버히트를 사용하지 않는다.
            weapon.TryFireAt(
                aimPoint,
                false
            );
        }
    }

    private bool HasOperationalWeapon()
    {
        foreach (ITargetedAttackPart weapon in _weapons)
        {
            if (weapon != null &&
                weapon.IsOperational)
            {
                return true;
            }
        }

        return false;
    }
}
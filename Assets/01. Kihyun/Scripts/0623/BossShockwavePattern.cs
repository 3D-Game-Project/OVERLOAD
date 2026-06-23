using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShockwavePattern : BossAttackPatternBase
{
    [Header("References")]
    [SerializeField] private Transform _shockwaveOrigin;
    [SerializeField] private BossMovementController _movementController;

    [Header("Pattern Timing")]
    [SerializeField] private float _chargeDuration = 1.5f;
    [SerializeField] private float _recoveryDuration = 1f;

    [Header("Shockwave")]
    [SerializeField] private float _activationRange = 6f;
    [SerializeField] private float _damageRadius = 8f;
    [SerializeField] private float _damage = 30f;
    [SerializeField] private LayerMask _targetLayer;

    [Header("Optional Effects")]
    [SerializeField] private ParticleSystem _chargeEffect;
    [SerializeField] private ParticleSystem _releaseEffect;

    public override BossAttackPatternType PatternType =>
        BossAttackPatternType.Shockwave;

    public override bool CanRunWhileMoving => false;

    private void Awake()
    {
        if (_shockwaveOrigin == null)
            _shockwaveOrigin = transform;

        if (_movementController == null)
        {
            _movementController =
                GetComponentInParent<BossMovementController>();
        }
    }

    protected override bool CheckRequirements(Transform target)
    {
        if (_shockwaveOrigin == null)
            return false;

        float distance = Vector3.Distance(
            _shockwaveOrigin.position,
            target.position
        );

        return distance <= _activationRange;
    }

    protected override IEnumerator ExecutePattern(Transform target)
    {
        StopBossMovement();

        PlayEffect(_chargeEffect);

        float chargeTimer = 0f;

        while (chargeTimer < _chargeDuration)
        {
            if (ShouldStop(target))
                yield break;

            // 충전 중에는 이동하지 않는다.
            StopBossMovement();

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        StopEffect(_chargeEffect);
        PlayEffect(_releaseEffect);

        ApplyShockwaveDamage();

        float recoveryTimer = 0f;

        while (recoveryTimer < _recoveryDuration)
        {
            if (ShouldStop(target))
                yield break;

            StopBossMovement();

            recoveryTimer += Time.deltaTime;
            yield return null;
        }
    }

    protected override void OnPatternEnded(bool wasCancelled)
    {
        StopEffect(_chargeEffect);

        StopBossMovement();
    }

    private void ApplyShockwaveDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(
            _shockwaveOrigin.position,
            _damageRadius,
            _targetLayer,
            QueryTriggerInteraction.Collide
        );

        HashSet<DurabilityController> damagedTargets = new();

        foreach (Collider hitCollider in colliders)
        {
            if (hitCollider == null)
                continue;

            DurabilityController durability =
                hitCollider.GetComponentInParent<DurabilityController>();

            if (durability == null)
                continue;

            if (!damagedTargets.Add(durability))
                continue;

            durability.TakeDamage(_damage);
        }

        Debug.Log(
            $"충격파 발동: {damagedTargets.Count}개 부위 피해"
        );
    }

    private void PlayEffect(ParticleSystem effect)
    {
        if (effect == null)
            return;

        if (!effect.gameObject.activeSelf)
            effect.gameObject.SetActive(true);

        effect.Play();
    }

    private void StopEffect(ParticleSystem effect)
    {
        if (effect == null)
            return;

        effect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    private void StopBossMovement()
    {
        if (_movementController == null)
            return;

        _movementController.StopMovement();
        _movementController.ClearLookTarget();
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin =
            _shockwaveOrigin != null
                ? _shockwaveOrigin
                : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            origin.position,
            _activationRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            origin.position,
            _damageRadius
        );
    }
}
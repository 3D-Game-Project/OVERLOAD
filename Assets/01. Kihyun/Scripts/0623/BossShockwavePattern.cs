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

    [Header("Optional Effect")]
    [SerializeField] private ParticleSystem _shockwaveEffectPrefab;
    [SerializeField] private float _effectLifetime = 5f;

    [Header("Effect Ground Placement")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundRayHeight = 5f;
    [SerializeField] private float _groundRayDistance = 20f;
    [SerializeField] private float _groundOffset = 0.05f;
    [SerializeField] private bool _alignEffectToGround = true;

    private ParticleSystem _activeShockwaveEffect;

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

        SpawnShockwaveEffect();

        float chargeTimer = 0f;

        while (chargeTimer < _chargeDuration)
        {
            if (ShouldStop(target))
                yield break;

            StopBossMovement();

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        // 이펙트는 정지하지 않고 계속 재생한다.
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

    private void SpawnShockwaveEffect()
    {
        if (_shockwaveEffectPrefab == null ||
            _shockwaveOrigin == null)
        {
            return;
        }

        Vector3 rayOrigin =
            _shockwaveOrigin.position +
            Vector3.up * _groundRayHeight;

        Vector3 spawnPosition =
            _shockwaveOrigin.position;

        Quaternion spawnRotation =
            Quaternion.identity;

        if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                _groundRayDistance,
                _groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            spawnPosition =
                hit.point + hit.normal * _groundOffset;

            if (_alignEffectToGround)
            {
                spawnRotation = Quaternion.FromToRotation(
                    Vector3.up,
                    hit.normal
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "충격파 이펙트를 생성할 지면을 찾지 못했습니다."
            );
        }

        // 보스의 자식으로 두지 않아 보스가 움직여도 이펙트가 고정된다.
        _activeShockwaveEffect = Instantiate(
            _shockwaveEffectPrefab,
            spawnPosition,
            spawnRotation
        );

        _activeShockwaveEffect.Play(true);

        Destroy(
            _activeShockwaveEffect.gameObject,
            _effectLifetime
        );
    }

    protected override void OnPatternEnded(bool wasCancelled)
    {
        if (wasCancelled &&
            _activeShockwaveEffect != null)
        {
            Destroy(_activeShockwaveEffect.gameObject);
        }

        _activeShockwaveEffect = null;
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
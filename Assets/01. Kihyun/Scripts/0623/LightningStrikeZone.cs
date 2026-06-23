using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrikeZone : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform _warningIndicator;
    [SerializeField] private ParticleSystem _chargeEffect;
    [SerializeField] private ParticleSystem _strikeEffect;

    [Header("Hit Area")]
    [SerializeField] private float _hitHeight = 10f;

    [Header("Cleanup")]
    [SerializeField] private float _cleanupDelay = 1f;

    private float _radius;
    private float _warningDuration;
    private float _damage;
    private LayerMask _targetLayer;

    private bool _isActivated;

    public bool IsFinished { get; private set; }

    public event Action<LightningStrikeZone> Finished;

    public void Activate(
        float radius,
        float warningDuration,
        float damage,
        LayerMask targetLayer)
    {
        if (_isActivated)
            return;

        _radius = radius;
        _warningDuration = warningDuration;
        _damage = damage;
        _targetLayer = targetLayer;

        _isActivated = true;
        IsFinished = false;

        UpdateIndicatorSize();

        StartCoroutine(StrikeSequence());
    }

    private IEnumerator StrikeSequence()
    {
        if (_warningIndicator != null)
            _warningIndicator.gameObject.SetActive(true);

        _chargeEffect?.Play();

        yield return new WaitForSeconds(
            _warningDuration
        );

        if (_warningIndicator != null)
            _warningIndicator.gameObject.SetActive(false);

        if (_chargeEffect != null)
        {
            _chargeEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        _strikeEffect?.Play();

        ApplyDamage();

        yield return new WaitForSeconds(
            _cleanupDelay
        );

        IsFinished = true;
        Finished?.Invoke(this);

        Destroy(gameObject);
    }

    private void ApplyDamage()
    {
        Vector3 bottom =
            transform.position + Vector3.up * 0.1f;

        Vector3 top =
            bottom + Vector3.up * _hitHeight;

        Collider[] colliders = Physics.OverlapCapsule(
            bottom,
            top,
            _radius,
            _targetLayer,
            QueryTriggerInteraction.Collide
        );

        HashSet<DurabilityController> damagedParts =
            new();

        foreach (Collider hitCollider in colliders)
        {
            if (hitCollider == null)
                continue;

            DurabilityController durability =
                hitCollider.GetComponentInParent<
                    DurabilityController>();

            if (durability == null)
                continue;

            if (!damagedParts.Add(durability))
                continue;

            durability.TakeDamage(_damage);
        }

        Debug.Log(
            $"낙뢰 적중: {damagedParts.Count}개 부위"
        );
    }

    private void UpdateIndicatorSize()
    {
        if (_warningIndicator == null)
            return;

        Vector3 scale =
            _warningIndicator.localScale;

        scale.x = _radius * 2f;
        scale.z = _radius * 2f;

        _warningIndicator.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            _radius
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position +
            Vector3.up * _hitHeight
        );
    }
}
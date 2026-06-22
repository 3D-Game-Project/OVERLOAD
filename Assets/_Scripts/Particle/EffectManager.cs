using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    // Struct로 짝지어놓은 파티클 이펙트들을 List로 모아둔 다음 Dictionary로 검색 용이하도록 설정
    [Header("무기 타입별 공격 이펙트")]
    [SerializeField] private FireEffect[] _fireEffectList;
    private Dictionary<WeaponType, ParticleSystem> _fireDictionary;

    [Header("무기 타입별 피격 이펙트.")]
    [SerializeField] private TakeDamageEffect[] _takeDamageEffectList;
    private Dictionary<WeaponType, ParticleSystem> _takeDamageDictionary;

    [Header("부위 파괴 이펙트")]
    [SerializeField] private ParticleSystem _destroyParticle;

    [Header("스파크 이펙트")]
    [SerializeField] private ParticleSystem _sparkParticlePrefab;

    [Header("파츠 파괴 후 연기 이펙트")]
    [SerializeField] private ParticleSystem _smokeParticlePrefab;

    private Dictionary<WeaponType, ObjectPool<ParticleSystem>> _firePools = new Dictionary<WeaponType, ObjectPool<ParticleSystem>>();
    private Dictionary<WeaponType, ObjectPool<ParticleSystem>> _takeDamagePools = new Dictionary<WeaponType, ObjectPool<ParticleSystem>>();
    private Dictionary<DurabilityController, ParticleSystem> _sparkParticles = new Dictionary<DurabilityController, ParticleSystem>();
    private Dictionary<DurabilityController, ParticleSystem> _smokeParticles = new Dictionary<DurabilityController, ParticleSystem>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ConvertParticleArrayToDictionary();

        InitializeObjectPools();
    }

    // Struct로 짝지은 이펙드들이 데이터상에서 Array에 담김
    // 이를 Dictionary로 전환하는 작업을 Awake에서 진행
    // 최종 목적: Dictionary로 전환하여 탐색속도를 조금 더 빠르게 처리하기 위해서
    private void ConvertParticleArrayToDictionary()
    {
        _fireDictionary = new Dictionary<WeaponType, ParticleSystem>();

        foreach (FireEffect fireEffect in _fireEffectList)
        {
            if (!_fireDictionary.ContainsKey(fireEffect.WeaponType))
            {
                _fireDictionary.Add(fireEffect.WeaponType, fireEffect.FireParticle);
            }
        }

        _takeDamageDictionary = new Dictionary<WeaponType, ParticleSystem>();
        foreach(TakeDamageEffect takeDamageEffect in _takeDamageEffectList)
        {
            if (!_takeDamageDictionary.ContainsKey(takeDamageEffect.WeaponType))
            {
                _takeDamageDictionary.Add(takeDamageEffect.WeaponType, takeDamageEffect.TakeDamageParticle);
            }
        }
    }
    /// <summary>
    /// 공격이펙트와 피격이펙트에 대하여 오브젝트 풀링으로 처리
    /// 오브젝트 풀링을 만든 이유는 공격, 피격시마다 이펙트를 생성시키고 삭제되면 가비지컬렉션이 동작하기때문
    /// </summary>
    private void InitializeObjectPools()
    {
        foreach (var pair in _fireDictionary)
        {
            WeaponType type = pair.Key;
            ParticleSystem prefab = pair.Value;

            _firePools[type] = new ObjectPool<ParticleSystem>(
                createFunc: () => Instantiate(prefab, transform),
                actionOnGet: (effect) => {
                    if (effect != null && effect.gameObject != null)
                    {
                        effect.gameObject.SetActive(true);
                    }
                },
                actionOnRelease: (effect) => {
                    if (effect != null && effect.gameObject != null)
                    {
                        effect.transform.SetParent(transform);
                        effect.gameObject.SetActive(false);
                    }
                },
                actionOnDestroy: (effect) => { if (effect != null) Destroy(effect.gameObject); },
                defaultCapacity: 10, maxSize: 30
            );
        }

        foreach (var pair in _takeDamageDictionary)
        {
            WeaponType type = pair.Key;
            ParticleSystem prefab = pair.Value;

            _takeDamagePools[type] = new ObjectPool<ParticleSystem>(
                createFunc: () => Instantiate(prefab, transform),
                actionOnGet: (effect) => effect.gameObject.SetActive(true),
                actionOnRelease: (effect) => effect.gameObject.SetActive(false),
                actionOnDestroy: (effect) => { if (effect != null) Destroy(effect.gameObject); },
                defaultCapacity: 15, maxSize: 40
            );
        }
    }

    // FireManager의 TryFire이 진행될 때, 무기타입에 따른 파티클 생성
    public ParticleSystem CreateFireEffect(WeaponType type, Transform parent)
    {
        if (_firePools.TryGetValue(type, out ObjectPool<ParticleSystem> pool))
        {
            ParticleSystem fireEffect = null;

            try
            {
                fireEffect = pool.Get();
                if (fireEffect == null || fireEffect.gameObject == null)
                {
                    _fireDictionary.TryGetValue(type, out ParticleSystem prefab);
                    fireEffect = Instantiate(prefab, transform);
                }
            }
            catch
            {
                _fireDictionary.TryGetValue(type, out ParticleSystem prefab);
                fireEffect = Instantiate(prefab, transform);
            }

            if (fireEffect != null)
            {
                fireEffect.transform.SetParent(parent);
                fireEffect.transform.localPosition = Vector3.zero;
                fireEffect.transform.localRotation = Quaternion.identity;

                StartCoroutine(ReleaseParticleCoroutine(pool, fireEffect, fireEffect.main.duration));
            }
            return fireEffect;
        }
        return null;
    }

    // 공격 적중시 무기타입에 따른 파티클 생성용
    // Instantiate(takeTamageParticle, pos, Quaternion.LookRotation(normal)); => 지정한 파티클을 hit.point에 생성시킬 때, hit.normal각도로 회전시켜 생성되도록 처리
    public void CreateTakeDamageEffect(WeaponType type, Vector3 pos, Vector3 normal)
    {
        if (_takeDamagePools.TryGetValue(type, out ObjectPool<ParticleSystem> pool))
        {
            ParticleSystem takeDamageEffect = null;
            try
            {
                takeDamageEffect = pool.Get();
                if (takeDamageEffect == null || takeDamageEffect.gameObject == null)
                {
                    _takeDamageDictionary.TryGetValue(type, out ParticleSystem prefab);
                    takeDamageEffect = Instantiate(prefab, transform);
                }
            }
            catch
            {
                _takeDamageDictionary.TryGetValue(type, out ParticleSystem prefab);
                takeDamageEffect = Instantiate(prefab, transform);
            }

            if (takeDamageEffect != null)
            {
                takeDamageEffect.transform.position = pos;
                takeDamageEffect.transform.rotation = Quaternion.LookRotation(normal);

                StartCoroutine(ReleaseParticleCoroutine(pool, takeDamageEffect, takeDamageEffect.main.duration));
            }
        }
    }
    public void PlaySparkParticle(DurabilityController partDurability)
    {
        if (partDurability == null) return;
        if (_sparkParticlePrefab == null) return;

        float durabilityRatio = partDurability.CurrentDurability / partDurability.MaxDurability;

        if (partDurability.IsDestroyed || durabilityRatio >= 0.4f) return;

        if (_sparkParticles.ContainsKey(partDurability)) return;

        Transform targetPoint = partDurability.ParticlePos;

        ParticleSystem sparkParticle = Instantiate(_sparkParticlePrefab, targetPoint.position, Quaternion.identity);
        sparkParticle.transform.SetParent(targetPoint, true);

        _sparkParticles.Add(partDurability, sparkParticle);
    }

    public void StopSparkParticle(DurabilityController partDurability)
    {
        if (_sparkParticles.TryGetValue(partDurability, out ParticleSystem sparkInstance))
        {
            if (sparkInstance != null)
            {
                sparkInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(sparkInstance.gameObject, 1f);
            }
            _sparkParticles.Remove(partDurability);
        }
    }


    /// <summary>
    ///  코루틴으로 파괴에 대한 연출 처리
    /// </summary>
    public void PlayPartDestroyEffect(DurabilityController durabilityController, Vector3 explodePosition, Transform followTarget)
    {
        StartCoroutine(DestroyAndSmokeCoroutine(durabilityController, explodePosition, followTarget));
    }

    /// <summary>
    /// 파괴 연출처리
    /// 폭발 -> 대기 -> 연기생성으로 진행
    /// 파츠가 사라지거나 내구도가 수리될 경우 파티클 끄기 진행
    /// </summary>
    private IEnumerator DestroyAndSmokeCoroutine(DurabilityController durabilityController, Vector3 explodePosition, Transform followTarget)
    {
        float explosionDuration = 0f;

        if (_destroyParticle != null)
        {
            ParticleSystem explosion = Instantiate(_destroyParticle, explodePosition, Quaternion.identity);
            if (followTarget != null) explosion.transform.SetParent(followTarget, true);

            explosionDuration = explosion.main.duration;
            Destroy(explosion.gameObject, explosionDuration);
        }

        yield return new WaitForSeconds(explosionDuration);

        if (durabilityController == null || durabilityController.CurrentDurability > 0f) yield break;

        if (_smokeParticlePrefab != null && durabilityController.ParticlePos != null)
        {
            ParticleSystem smoke = Instantiate(_smokeParticlePrefab, durabilityController.ParticlePos.position, Quaternion.identity);
            smoke.transform.SetParent(durabilityController.ParticlePos, true);

            _smokeParticles[durabilityController] = smoke;
        }
    }

    public void StopSmokeParticle(DurabilityController durabilityController)
    {
        if (_smokeParticles.TryGetValue(durabilityController, out ParticleSystem smokeInstance))
        {
            if (smokeInstance != null)
            {
                smokeInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(smokeInstance.gameObject, 2f);
            }
            _smokeParticles.Remove(durabilityController);
        }
    }

    private IEnumerator ReleaseParticleCoroutine(ObjectPool<ParticleSystem> targetPool, ParticleSystem particle, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (particle != null && particle.gameObject != null && particle.gameObject.activeSelf)
        {
            try
            {
                targetPool.Release(particle);
            }
            catch
            {
            }
        }
    }
}

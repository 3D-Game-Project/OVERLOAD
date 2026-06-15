using System.Collections.Generic;
using UnityEngine;
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

    // FireManager의 TryFire이 진행될 때, 무기타입에 따른 파티클 생성
    public ParticleSystem CreateFireEffect(WeaponType type, Transform parent)
    {
        if(_fireDictionary.TryGetValue(type, out ParticleSystem fireParticle) && fireParticle != null)
        {
            ParticleSystem fireEffect = Instantiate(fireParticle, parent);
            fireEffect.transform.localPosition = Vector3.zero;
            fireEffect.transform.localRotation = Quaternion.identity;
            return fireEffect;
        }
        return null;
    }

    // 공격 적중시 무기타입에 따른 파티클 생성용
    // Instantiate(takeTamageParticle, pos, Quaternion.LookRotation(normal)); => 지정한 파티클을 hit.point에 생성시킬 때, hit.normal각도로 회전시켜 생성되도록 처리
    public void CreateTakeDamageEffect(WeaponType type, Vector3 pos, Vector3 normal)
    {
        if(_takeDamageDictionary.TryGetValue(type, out ParticleSystem takeTamageParticle) && takeTamageParticle != null)
        {
            Instantiate(takeTamageParticle, pos, Quaternion.LookRotation(normal));
            Debug.Log($"피격 이펙트 생성, 타입: {type}, 파티클: {takeTamageParticle}");
        }
    }

    // 기존의 파츠 내구도 0이 되었을 때, Destroy처리로 임시 구현해두었던 사항에 대한 변경처리
    // 파츠 내구도가 0이 되면 파티클을 생성하도록 구현.
    // 무한정 부위파괴에 대한 이펙트를 나타내는것 보다 5초의 시간동안 파티클이 생성되었다 사라지도록 수정
    public void PlayPartDestroyEffect(Vector3 position, Transform followTarget)
    {
        if (_destroyParticle != null)
        {
            ParticleSystem destroyParticle = Instantiate(_destroyParticle, position, Quaternion.identity);

            if (followTarget != null)
            {
                destroyParticle.transform.SetParent(followTarget, true);
            }

            Destroy(destroyParticle.gameObject, 5f);
        }
    }
}

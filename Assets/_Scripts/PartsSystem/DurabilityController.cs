using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DurabilityController : MonoBehaviour
{
    [SerializeField] private DurabilityType _durabilityType;
    [SerializeField] private float _currentDurability;
    [SerializeField] private float _maxDurability;
    [SerializeField] private PartsData _partsData;
    [SerializeField] private UnitData _unitData;
    [SerializeField] private string _attachedSlotId;

    [Header("스파크, 연기 파티클이 생성될 위치. 무기마다 다름")]
    [SerializeField] private Transform _particlePos;

    [SerializeField]private List<PartsData> _destroyedPartList = new List<PartsData>();

    private bool _isDestroyed = false;
    private DurabilityController _rootCoreController;

    private InventoryPartItem _linkedPartItem;

    public float CurrentDurability => _currentDurability;
    public bool IsDestroyed => _isDestroyed || _currentDurability <= 0f;
    public Transform ParticlePos => _particlePos != null ? _particlePos : transform;
    public DurabilityType DurabilityType => _durabilityType;
    public PartsData PartsData => _partsData;


    public float MaxDurability => _maxDurability;

    [SerializeField] private Animator _animator;
    [SerializeField] private string _deathTrigger = "Death";

    public event Action<float, float> OnDurabilityChanged;
    public event Action OnCoreDestroyed;

    /// <summary>
    /// 시작시 DurabilityType이 Core인지 Part인지 자동으로 파악하고 그거에 맞춰서 DurabilityType 수정
    /// 및 코어의 경우 300수치로 고정설정(이 부분은 나중에 코어를 추가한다면 변경하면됨)
    /// </summary>
    private void Awake()
    {
        
        if(GetComponent<CharacterController>() != null)
        {
            Debug.Log("CharacterController 확인.");
            _durabilityType = DurabilityType.Core;

            if(gameObject.layer == 6)
            {
                _maxDurability = 2000f;
            }

            if(gameObject.layer == 7)
            {
                _maxDurability = 300f;
            }
            _currentDurability = _maxDurability;

        }
        else
        {
            if (GetComponent<Collider>() == null)
            {
                GenerateCollider();
            }

            Debug.Log("Collider 확인.");
            _durabilityType = DurabilityType.Part;
        }
    }

    /// <summary>
    /// 코어일 경우 체력 슬라이더에 이벤트발행.
    /// 파츠일경우 InitializePart함수호출
    /// </summary>
    private void Start()
    {
        if (_durabilityType == DurabilityType.Core)
        {
            OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);

        }
        else
        {
            _rootCoreController = GetRootCoreController();

            if (_linkedPartItem == null && _partsData != null && _currentDurability <= 0)
            {
                InitializePart(_partsData, false);
            }
        }
    }

    // 콜라이더 생성 함수
    // 오브젝트 하위에 있는 모델링 렌더러를 찾기
    // 이후, 렌더러 영역을 기준으로 나머지 추가하여 스케일 크기 설정
    // 최종 모델링사이즈에 추가적으로 사이즈조정(-0.5씩)
    private void GenerateCollider()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            gameObject.AddComponent<BoxCollider>();
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();

        boxCol.center = transform.InverseTransformPoint(bounds.center);

        Vector3 localSize = bounds.size;
        if (transform.lossyScale != Vector3.zero)
        {
            localSize = new Vector3(
                localSize.x / transform.lossyScale.x,
                localSize.y / transform.lossyScale.y,
                localSize.z / transform.lossyScale.z
            );
        }

        boxCol.size = localSize + new Vector3(-0.5f, 0f, -0.5f);
    }

    /// <summary>
    /// 파츠 정보, 몬스터를통해 얻은파츠인지아닌지 확인하여 내구도 설정
    /// </summary>
    /// <param name="partData"></param>
    /// <param name="isDropPart"></param>
    public void InitializePart(PartsData partData, bool isDropPart) 
    {
        _partsData = partData;
        _durabilityType = DurabilityType.Part;
        _maxDurability = partData.MaxDurability;

        if (isDropPart)
        {
            _currentDurability = _maxDurability * 0.8f;
        }
        else
        {
            _currentDurability = _maxDurability;
        }

        _isDestroyed = _currentDurability <= 0f;
    }


    public void InitializeFromInventoryItem(InventoryPartItem partItem)
    {
        if (partItem == null || partItem.PartsData == null)
        {
            Debug.LogWarning("[DurabilityController] 연결할 InventoryPartItem 또는 PartsData가 없습니다.");
            return;
        }

        _linkedPartItem = partItem;
        _partsData = partItem.PartsData;
        _durabilityType = DurabilityType.Part;

        _maxDurability = partItem.MaxDurability;
        _currentDurability = partItem.CurrentDurability;

        _isDestroyed = _currentDurability <= 0f;

        CheckAndStopEffects();
    }

    private void SyncToInventoryItem()
    {
        if (_linkedPartItem == null)
            return;

        _linkedPartItem.SetDurability(_currentDurability);
    }

    /// <summary>
    ///  피격함수. 공격력 - 방어력으로 최종 데미지를 설정하고 1보다 작으면 1이라도 들어가게 수정
    ///  내구도가 0이되면 코어와 파츠에 따른 함수 추가 호출
    /// </summary>
    /// <param name="attackDamage"></param>
    public void TakeDamage(float attackDamage)
    {
        if (_isDestroyed) return;
        float finalDamage = attackDamage;

        float armorDefense = 0f;
        //float armorDefense = GetArmorPartsFromSameLayer();

        if (armorDefense > 0) 
        {
            finalDamage = attackDamage - armorDefense;

            if (finalDamage < 1f) finalDamage = 1f;
            
        }

        _currentDurability -= finalDamage;
        _currentDurability = Mathf.Clamp(_currentDurability, 0f, _maxDurability);

        SyncToInventoryItem();

        if (_durabilityType == DurabilityType.Core)
        {
            OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
        }

        float durabilityRatio = _currentDurability / _maxDurability;

        // durabilityRatio가 40%미만일때 스파크 연출되도록 처리
        if(durabilityRatio < 0.4f)
        {
            if (_durabilityType == DurabilityType.Part) PlaySparkEffect();
        }

        if(_currentDurability <= 0f) DestroyPart();
    }

    // 내구도가 0으로 감소되었을 때, 코어, 파츠별 파괴 분기처리
    // 코어는 애니메이션, 드랍 후 Destroy
    // 파츠는 파괴되는 본인의 데이터를 상위로 전달하여 리스트에 추가
    private void DestroyPart()
    {
        if (_currentDurability > 0f) return;

        _isDestroyed = true;

        if(_durabilityType == DurabilityType.Core)
        {
            StartCoroutine(CoreDestroySequence());
        }
        else
        {
            ControllComponent();

            if(_rootCoreController != null && _partsData != null)
            {
                _rootCoreController.AddDestroyedPartToList(_partsData);
            }

            PlayPartDestroyEffect();
        }

    }

    // 같은 부위에 부착되는 Armor타입을 Layer로 가져오는 방식. 현재는 사용x
    private float GetArmorPartsFromSameLayer()
    {
        CorePartsController coreParts = GetComponentInParent<CorePartsController>();
        
        if(coreParts == null) return 0f;

        AttachmentSlot[] attachedSlots = coreParts.GetComponentsInChildren<AttachmentSlot>();

        foreach (AttachmentSlot slot in attachedSlots) 
        {
            if (slot == null || slot.SlotId != _attachedSlotId) continue;

            if(slot.HasPart && slot.AttachedObject != null)
            {
                MonoBehaviour[] scripts = slot.AttachedObject.GetComponentsInChildren<MonoBehaviour>();

                foreach(MonoBehaviour script in scripts)
                {
                    if(script is IPart part && part.Data != null && part.Data.PartsType == PartsType.Armor)
                    {
                        //if(part.Data is ArmorPartsData armorData)
                        //{
                        //    return armorData.Defense; 
                        //}
                    }
                }
            }
        }
        return 0f;
    }

    // 부위 파괴시 컴포넌트들 false로 제어하여 발생할 수 있는 문제들(대표적으로 사격)차단
    private void ControllComponent()
    {
        if ((TryGetComponent(out Collider col))) col.enabled = false;

        MonoBehaviour[] component = GetComponentsInChildren<MonoBehaviour>(true);
        foreach(var script in component)
        {
            if (script == null || script == this) continue;

            string name = script.GetType().Name;
            if (name == "WeaponGimbalController" || name == "FireManager" || name == "AttackPartController") 
                script.enabled = false;
        }

        Animator[] animators = GetComponentsInChildren<Animator>(true);
        foreach (Animator animator in animators)
        {
            animator.enabled = false;
        }

        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var particle in particles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // 코어가 아닌 파츠가 파괴될 때, 파괴된 정보를 코어에게 전달하여 나중에 드랍리스트에서 제거시키는 용도
    public void AddDestroyedPartToList(PartsData part)
    {
        if(_durabilityType == DurabilityType.Core && !_destroyedPartList.Contains(part))
        {
            _destroyedPartList.Add(part);
        }
    }

    // 최상위 부모 오브젝트 계측에서 코어 타입의 DurabilityController를 추적하는 함수
    private DurabilityController GetRootCoreController()
    {
        DurabilityController[] controllers = GetComponentsInParent<DurabilityController>(true);

        foreach (var ctrl in controllers)
        {
            if (ctrl._durabilityType == DurabilityType.Core)
            {
                return ctrl;
            }
        }

        return null;
    }

    /// <summary>
    ///  방어파츠를 고려하여 생긴 함수. 같은 슬롯아이디를 가졌는데 방어파츠를 가진게 있는지 확인할 때 사용하는 함수
    /// </summary>
    /// <param name="slotId"></param>
    public void SetAssociatedSlotId(string slotId)
    {
        Debug.Log($"SetAssociatedSlotId, {slotId}");
        _attachedSlotId = slotId;
    }


    public void InitializeDroppedPart(PartsData partData, float currentDurability)
    {
        if (partData == null)
        {
            Debug.LogWarning("[DurabilityController] 드랍 파츠 초기화 실패: PartsData 없음");
            return;
        }

        _linkedPartItem = null;

        _partsData = partData;
        _durabilityType = DurabilityType.Part;
        _maxDurability = partData.MaxDurability;
        _currentDurability = Mathf.Clamp(currentDurability, 0f, _maxDurability);

        _isDestroyed = _currentDurability <= 0f;
    }

    /// <summary>
    /// 코어 내구도가 0이 되었을 때, 몬스터의 경우 Agent를 멈추는 처리
    /// 추후 애니메이션이 들어간다면 _deathTrigger를 애니메이션에 파라미터로 전달하여 애니메이션 동작
    /// 2초 지연 후, 파츠드랍함수 실행
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoreDestroySequence()
    {
        if (TryGetComponent(out UnityEngine.AI.NavMeshAgent agent)) { agent.isStopped = true; agent.enabled = false; }
        if (TryGetComponent(out Collider collider)) collider.enabled = false;
        if (TryGetComponent(out CharacterController characterController)) characterController.enabled = false;

        //if (_animator != null && !string.IsNullOrEmpty(_deathTrigger)) _animator.SetTrigger(_deathTrigger);

        
        if (gameObject.layer == 6)
        {
            if (PlayerDeathUI.Instance != null) PlayerDeathUI.Instance.PlayGlitchEffect();

            yield return new WaitForSeconds(1.5f);

            if (EffectManager.instance != null) EffectManager.instance.PlayDeathParticle(transform.position);

            float fadeDuration = 2.0f; 
            if (PlayerDeathUI.Instance != null) PlayerDeathUI.Instance.StartBlackFade(fadeDuration);

            yield break;
        }

        Renderer[] mobRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer ren in mobRenderers) ren.enabled = false;

        if (EffectManager.instance != null) EffectManager.instance.PlayDeathParticle(transform.position);

        yield return new WaitForSeconds(0.5f);

        if (_unitData is EnemyData enemyData)
        {
            DropRuntime dropRuntime = new DropRuntime();

            MonsterLifecycle monsterRoot = GetComponentInParent<MonsterLifecycle>();
            GameObject unitRoot = null;

            if (monsterRoot != null)
                unitRoot = monsterRoot.gameObject; 
            else
                unitRoot = this.gameObject; 

            Vector3 dropPosition = gameObject.transform.position;

            if (unitRoot != null)
            {
                dropRuntime.DropAttachedPartsFromUnit(unitRoot, enemyData, dropPosition);
            }
        }
        
        if (gameObject.layer == 7) OnCoreDestroyed?.Invoke();

        if (TryGetComponent(out BossAI bossAI))
        {
            Destroy(gameObject);
        }
    }

    public void RespawnNearPoint()
    {
        Shop[] shops = UnityEngine.Object.FindObjectsByType<Shop>(FindObjectsSortMode.None);

        Vector3 respawnPos = Vector3.zero;

        if (shops.Length == 0)
        {
            Debug.LogWarning("Shop 미검색됨. 플레이어를 원점(0,0,0)으로 부활시킵니다.");
            respawnPos = Vector3.zero; 
        }
        else
        {
            Shop nearestShop = null;
            float minDistance = float.MaxValue;
            Vector3 deathPosition = transform.position;

            foreach (Shop shop in shops)
            {
                float distance = Vector3.Distance(deathPosition, shop.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestShop = shop;
                }
            }

            Vector2 randomPos = UnityEngine.Random.insideUnitCircle.normalized * 10f;
            Vector3 offset = new Vector3(randomPos.x, 1f, randomPos.y);
            respawnPos = nearestShop.transform.position + offset;

            Debug.Log($"가장 가까운 상점 '{nearestShop.gameObject.name}' 근처에서 부활하였습니다.");
        }

        if (TryGetComponent(out CharacterController cc)) cc.enabled = false;
        if (TryGetComponent(out UnityEngine.AI.NavMeshAgent nav)) nav.enabled = false;

        transform.position = respawnPos;
        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;
        if (TryGetComponent(out Collider col)) col.enabled = true;

        if (nav != null)
        {
            nav.enabled = true;
            nav.Warp(respawnPos); 
            nav.isStopped = false;
        }

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }

        Animator[] allAnimators = GetComponentsInChildren<Animator>(true);
        foreach (Animator anim in allAnimators)
        {
            anim.enabled = true;
        }

        _currentDurability = _maxDurability;
        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
    }

    /// <summary>
    /// 파츠가 교체되거나 수리가 진행되는정도에 따른 연출 제거함수
    /// 0 이상이면 연기 끄기, 40이상이면 스파크도 끄기
    /// </summary>

    private void CheckAndStopEffects()
    {
        if (_currentDurability > 0f)
        {
            _isDestroyed = false; 

            if (EffectManager.instance != null)
            {
                EffectManager.instance.StopSmokeParticle(this);

                float durabilityRatio = _currentDurability / _maxDurability;
                if (durabilityRatio >= 0.4f)
                {
                    EffectManager.instance.StopSparkParticle(this);
                }
            }
        }
    }

    // Pool로 저장한 몬스터가 죽었다가 다시 스폰될 때, 내구도 복원 및 꺼두었던 기능들 복원하는 함수
    public void ResetDurability()
    {
        _isDestroyed = false;
        _destroyedPartList.Clear();

        if (_durabilityType == DurabilityType.Core)
        {
            _currentDurability = _maxDurability;
            OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);

            if (TryGetComponent(out UnityEngine.AI.NavMeshAgent agent)) agent.enabled = true;
            if (TryGetComponent(out Collider collider)) collider.enabled = true;
            if (TryGetComponent(out CharacterController cc)) cc.enabled = true;

            if (_animator != null && !string.IsNullOrEmpty(_deathTrigger))
            {
                _animator.ResetTrigger(_deathTrigger);
                _animator.Rebind(); 
                _animator.Update(0f);
            }

            DurabilityController[] childParts = GetComponentsInChildren<DurabilityController>(true);
            foreach (var part in childParts)
            {
                if (part != this && part.DurabilityType == DurabilityType.Part)
                {
                    part.ResetDurability();
                }
            }
        }
        else 
        {
            if (_partsData != null) _maxDurability = _partsData.MaxDurability;
            _currentDurability = _maxDurability;

            
            if (TryGetComponent(out Collider col)) col.enabled = true;

            MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var script in components)
            {
                if (script == null || script == this) continue;
                string name = script.GetType().Name;
                if (name == "WeaponGimbalController" || name == "FireManager" || name == "AttackPartController")
                    script.enabled = true;
            }

            Animator[] animators = GetComponentsInChildren<Animator>(true);
            foreach (Animator anim in animators) anim.enabled = true;

            CheckAndStopEffects();
        }
    }

    /// <summary>
    /// 파츠의 내구도가 0이 되었을 때 파티클연출이 시작되도록 하는 함수
    /// 싱글톤인 EffectManager(ParticleManager)를 호출
    /// </summary>
    private void PlayPartDestroyEffect()
    {
        if (EffectManager.instance != null)
        {
            Transform followTarget = _rootCoreController != null ? _rootCoreController.transform : transform.root;

            EffectManager.instance.PlayPartDestroyEffect(this, transform.position, followTarget);
        }
    }
    private void PlaySparkEffect()
    {
        if (EffectManager.instance != null)
        {
            EffectManager.instance.PlaySparkParticle(this);
        }
    }

    [ContextMenu("🧪 테스트: 데미지 테스트 (스파크 확인용)")]
    public void TestTakeDamage()
    {
        if (Application.isPlaying)
        {
            float testDmg = _maxDurability * 0.7f;
            Debug.Log($"[테스트] 50 데미지 피격! (현재 체력: {_currentDurability - testDmg})");
            TakeDamage(50f); // 방어력 등 모든 연산이 포함된 공식 피격 루트를 탑니다.
        }
    }

    [ContextMenu("🧪 테스트: 즉시 완전 파괴 (연기 확인용)")]
    public void TestInstantDestroy()
    {
        if (Application.isPlaying)
        {
            Debug.Log("[테스트] 파츠 즉시 파괴 명령!");
            TakeDamage(_currentDurability); // 남은 체력만큼 데미지를 줘서 0으로 만들어버림
        }
    }

    [ContextMenu("🧪 테스트: 100% 완전 수리 (이펙트 꺼짐 확인용)")]
    public void TestFullRepair()
    {
        if (Application.isPlaying)
        {
            _currentDurability = _maxDurability;
            Debug.Log("[테스트] 체력 100% 수리 완료! 연기와 스파크가 꺼져야 합니다.");

            // 아까 만든 수리 감지 함수를 강제 호출하여 이펙트를 끕니다.
            CheckAndStopEffects();
        }
    }
}

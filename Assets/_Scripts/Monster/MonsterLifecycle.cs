using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;
using System.Collections;

public class MonsterLifecycle : MonoBehaviour
{
    public int PrefabIndex { get; set; }

    private NavMeshAgent _agent;
    private BehaviorGraphAgent _behaviorAgent;
    private DurabilityController _coreDurability;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _coreDurability = GetComponent<DurabilityController>();
    }

    private void OnEnable()
    {
        if (_coreDurability != null)
        {
            _coreDurability.OnCoreDestroyed += Die;

            _coreDurability.ResetDurability();
        }
    }

    private void OnDisable()
    {
        if (_coreDurability != null)
        {
            _coreDurability.OnCoreDestroyed -= Die;
        }
    }

    // 매니저가 스폰 직후에 호출하는 함수
    public void WalkToSpawnPoint(Vector3 spawnPointPos, GameObject targetPlayer)
    {
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.SetDestination(spawnPointPos);
        }

        if (_behaviorAgent != null && targetPlayer != null)
        {
            bool success = _behaviorAgent.BlackboardReference.SetVariableValue<GameObject>("Target", targetPlayer);

            if (success)
            {
                Debug.Log($"[🧠 AI 타겟 연결 성공] {_behaviorAgent.gameObject.name}의 'Target'이 플레이어로 설정되었습니다!");
            }
            else
            {
                Debug.LogError($"[🔴 AI 타겟 연결 실패] Blackboard에 'Target'이라는 이름의 GameObject 변수가 정확히 존재하는지 대소문자를 확인하세요!");
            }
        }
    }

    // 몬스터가 사망시 이벤트를 구독받아 Pool다시 반환처리하는 함수
    private void Die()
    {
        if (MonsterSpawnManager.Instance != null)
        {
            MonsterSpawnManager.Instance.ReturnToPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
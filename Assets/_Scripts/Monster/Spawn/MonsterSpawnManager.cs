using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager Instance { get; private set; }

    [Header("스폰 설정")]
    [SerializeField] private Transform _player;
    [SerializeField] private int _maxAliveMonsters = 5;
    [SerializeField] private float _spawnDistance = 70f;

    [SerializeField] private float _safeDistance = 20f;

    [SerializeField] private float _pointCooldownTime = 300f;
    [SerializeField] private float _despawnDistance = 100f;

    [SerializeField] private float _respawnDelayAfterDeath = 10f;

    private float _lastDeathTime = -999f;

    [Header("몬스터 프리팹 리스트 (레벨 순서대로 배치)")]
    [SerializeField] private List<GameObject> _monsterPrefabs;

    private List<MonsterSpawnPoint> _spawnPoints = new List<MonsterSpawnPoint>();
    private List<MonsterLifecycle> _aliveMonsters = new List<MonsterLifecycle>();

    private Dictionary<int, Queue<MonsterLifecycle>> _monsterPool = new Dictionary<int, Queue<MonsterLifecycle>>();
    private Dictionary<MonsterSpawnPoint, float> _pointCooldowns = new Dictionary<MonsterSpawnPoint, float>();

    private float _logTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = GameObject.FindWithTag("Player")?.transform;
            Debug.Log(_player != null ? "[🟡 매니저] 플레이어 자동 찾기 성공!" : "[🔴 매니저] 플레이어를 찾을 수 없습니다!");
        }

        Debug.Log($"[🟡 매니저 준비 완료] 풀링된 몬스터 큐 세팅 끝! 현재 유지 마릿수: {_maxAliveMonsters}");
    }

    private void Update()
    {
        if (_player == null) return;

        HandleDespawn();
        CheckSpawnCondition();

        _logTimer += Time.deltaTime;
        if (_logTimer > 1f)
        {
            if (_spawnPoints.Count == 0) Debug.LogWarning("[🟠 매니저 대기중] 씬에 등록된 스폰 포인트가 0개입니다! (포인트 스크립트를 확인하세요)");
            _logTimer = 0f;
        }
    }

    public void RegisterSpawnPoint(MonsterSpawnPoint point) => _spawnPoints.Add(point);
    public void UnregisterSpawnPoint(MonsterSpawnPoint point) => _spawnPoints.Remove(point);

    private void InitializePool()
    {
        for (int i = 0; i < _monsterPrefabs.Count; i++)
        {
            _monsterPool[i] = new Queue<MonsterLifecycle>();
            for (int j = 0; j < _maxAliveMonsters + 2; j++)
            {
                GameObject obj = Instantiate(_monsterPrefabs[i], transform);
                obj.SetActive(false);

                MonsterLifecycle lifecycle = obj.GetComponent<MonsterLifecycle>();
                if (lifecycle == null) lifecycle = obj.AddComponent<MonsterLifecycle>();

                lifecycle.PrefabIndex = i;
                _monsterPool[i].Enqueue(lifecycle);
            }
        }
    }

    private void HandleDespawn()
    {
        for (int i = _aliveMonsters.Count - 1; i >= 0; i--)
        {
            MonsterLifecycle monster = _aliveMonsters[i];
            if (Vector3.Distance(_player.position, monster.transform.position) > _despawnDistance)
            {
                Debug.Log($"[🔵 디스폰 작동] 몬스터가 플레이어와 {_despawnDistance} 이상 멀어졌습니다. 풀로 반환합니다.");
                ReturnToPool(monster);
            }
        }
    }

    private void CheckSpawnCondition()
    {
        if (_aliveMonsters.Count >= _maxAliveMonsters) return;
        if (_spawnPoints.Count == 0) return;

        if (Time.time - _lastDeathTime < _respawnDelayAfterDeath) return;

        List<MonsterSpawnPoint> validPoints = _spawnPoints
            .Where(p => {
                float dist = Vector3.Distance(_player.position, p.transform.position);
                return dist <= _spawnDistance && dist > _safeDistance;
            })
            .Where(p => !_pointCooldowns.ContainsKey(p) || Time.time - _pointCooldowns[p] >= _pointCooldownTime) // 3. 쿨타임(5분)이 지났는지 확인
            .OrderBy(p => Vector3.Distance(_player.position, p.transform.position))
            .ToList();

        if (validPoints.Count == 0) return;

        MonsterSpawnPoint targetPoint = validPoints[0];
        _pointCooldowns[targetPoint] = Time.time;

        SpawnMonsterToPosition(targetPoint);
    }

    private void SpawnMonsterToPosition(MonsterSpawnPoint point)
    {
        int targetIndex = GetMonsterPrefabIndex();

        if (_monsterPool.TryGetValue(targetIndex, out Queue<MonsterLifecycle> pool) && pool.Count > 0)
        {
            MonsterLifecycle monster = pool.Dequeue();
            _aliveMonsters.Add(monster);

            Renderer[] renderers = monster.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer ren in renderers) ren.enabled = true;

            Collider[] colliders = monster.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders) col.enabled = true;

            Vector3 basePos = point.transform.position;
            Vector2 randomOffset = Random.insideUnitCircle * 4f;
            Vector3 rayStartPos = basePos + new Vector3(randomOffset.x, 0f, randomOffset.y);

            Vector3 spawnPos = rayStartPos;

            if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 50f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                spawnPos = hit.point;
            }
            else
            {
                Debug.LogWarning($"[스폰 경고] {point.gameObject.name} 아래로 바닥을 찾지 못했습니다. 허공에 생성될 수 있습니다.");
            }

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
            {
                spawnPos = navHit.position;
            }

            if (monster.TryGetComponent(out NavMeshAgent agent)) agent.enabled = false;

            monster.transform.position = spawnPos;
            monster.transform.rotation = point.transform.rotation;

            if (agent != null) agent.enabled = true;

            monster.gameObject.SetActive(true);
            monster.WalkToSpawnPoint(point.transform.position, _player.gameObject);
        }
        else
        {
            Debug.LogWarning($"[🔴 스폰 실패] 인덱스 {targetIndex} 몬스터의 풀(Pool)이 비어있습니다!");
        }
    }

    public void ReturnToPool(MonsterLifecycle monster)
    {
        monster.gameObject.SetActive(false);
        _aliveMonsters.Remove(monster);
        _monsterPool[monster.PrefabIndex].Enqueue(monster);

        _lastDeathTime = Time.time;
    }

    private int GetMonsterPrefabIndex()
    {
        if (_monsterPrefabs.Count == 0) return 0;

        FireManager[] fireManagers = _player.GetComponentsInChildren<FireManager>();
        int fmCount = fireManagers.Length;

        if (fmCount >= 3 && _monsterPrefabs.Count > 5)
        {
            int maxIdx = Mathf.Min(8, _monsterPrefabs.Count - 1);
            return Random.Range(5, maxIdx + 1);
        }

        List<int> spawnMonsterList = new List<int> { Random.Range(0, 3) };

        List<int> realSpawnMonsterList = spawnMonsterList.Where(idx => idx < _monsterPrefabs.Count).ToList();
        if (realSpawnMonsterList.Count == 0) return 0;

        return realSpawnMonsterList[Random.Range(0, realSpawnMonsterList.Count)];
    }
}
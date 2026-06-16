using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class InGameSceneManager : MonoBehaviour
{
    [Header("[ 고정 배치된 내비메쉬 서페이스 ]")]
    [SerializeField] private NavMeshSurface _sceneNavMeshSurface;

    [Header("[ 미리 구워둔 내비메쉬 데이터 파일 ]")]
    [SerializeField] private NavMeshData _preBakedNavMeshData;

    [Header("[ 터레인 스폰 위치 ]")]
    [SerializeField] private Vector3 _terrainSpawnPosition = new Vector3(-1720f, -28f, -1862f);

    // 🌟 [기획 반영] 유닛들이 최상위로 탈출하면서 강제 재정렬될 고유 타깃 좌표 세팅 칸!
    [Header("[ 핵심 유닛 스폰 좌표 커스텀 세팅 ]")]
    [SerializeField] private Vector3 _playerTargetPosition = new Vector3(-17f, 5f, -97f); // 네 실제 플레이어 스폰 위치 입력
    [SerializeField] private Vector3 _enemy1TargetPosition = new Vector3(-22f, 0f, -25f); // Enemy_Type1 위치
    [SerializeField] private Vector3 _enemy2TargetPosition = new Vector3(-120f, -1f, -1.5f); // Enemy_Type2 위치

    private List<NavMeshAgent> _spawnedAgents = new List<NavMeshAgent>();

    private void Start()
    {
        BuildAndUnpackInGameWorld();
    }

    private void BuildAndUnpackInGameWorld()
    {

        GameObject terrainPrefab = AssetLoader.Instance.GetLoadedPrefab("TerrainHigh");
        if (terrainPrefab != null)
        {
            GameObject spawnedTerrain = Instantiate(terrainPrefab, _terrainSpawnPosition, Quaternion.identity);
            spawnedTerrain.name = "TerrainHigh";
        }

        if (_sceneNavMeshSurface != null && _preBakedNavMeshData != null)
        {
            _sceneNavMeshSurface.gameObject.SetActive(true);
            _sceneNavMeshSurface.enabled = true;
            _sceneNavMeshSurface.navMeshData = _preBakedNavMeshData;
            Debug.Log("[InGameManager] 내비메쉬 데이터 0초 이식 성공.");
        }

        UnpackLoaderPrefabs();
    }

    private void UnpackLoaderPrefabs()
    {
        GameObject groupPrefab = AssetLoader.Instance.GetLoadedPrefab("InGameLoaderPrefabs");
        if (groupPrefab == null)
        {
            Debug.LogError("[InGameManager] 에셋로더에서 주머니 프리팹(InGameLoaderPrefabs)을 찾을 수 없습니다.");
            return;
        }

        GameObject container = Instantiate(groupPrefab, Vector3.zero, Quaternion.identity);
        NavMeshAgent[] agents = container.GetComponentsInChildren<NavMeshAgent>(true);
        foreach (var agent in agents)
        {
            agent.enabled = false;
            _spawnedAgents.Add(agent);
        }

        int childCount = container.transform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++) children[i] = container.transform.GetChild(i);

        foreach (Transform child in children)
        {
            child.SetParent(null, worldPositionStays: true);

            if (child.name.StartsWith("PlayerRoot"))
            {
                child.position = _playerTargetPosition;
            }
            else if (child.name.StartsWith("Enemy_Type1"))
            {
                child.position = _enemy1TargetPosition;
            }
            else if (child.name.StartsWith("Enemy_Type2"))
            {
                child.position = _enemy2TargetPosition;
            }
        }

        Destroy(container);
        Debug.Log("[InGameManager] 모든 프리팹 최상위 레이어 해방 및 타깃 위치 정렬 완료.");

        EnableAllAgents();
    }

    private void EnableAllAgents()
    {
        foreach (var agent in _spawnedAgents)
        {
            if (agent != null) agent.enabled = true;
        }
        Debug.Log("[InGameManager] 모든 몬스터 정찰 시작! 인스펙터 연결 완벽 보존됨.");
    }
}
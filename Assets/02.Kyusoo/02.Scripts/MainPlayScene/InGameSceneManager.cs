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

    /// <summary>
    /// 인게임 씬이 시작될 때 필요한 터레인 지형, 플레이어 및 에너미 유닛 주머니 프리팹을 스폰. 단, 스폰시 위치를 고정생성시키는중
    /// 추후 고정생성시키는걸 고려하면 Struct로 UnitPosition, PatrolPoint를 같이 추가해서 생성하도록 설정예정
    /// 미리 베이킹된 내비메쉬 데이터를 실시간으로 조립 및 정렬
    /// </summary>
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
        }

        UnpackLoaderPrefabs();
    }

    /// <summary>
    /// Unit, Menu 등의 통합 묶음 프리팹(InGameLoaderPrefabs)을 인스턴스화한 후 내부 구조를 해체하는 함수.
    /// 자식으로 묶여있던 모든 플레이어 및 몬스터들을 하이어라키 최상위 레벨(Root)로 독립처리. Canvas로 들어가야하는것들은 Canvas하위에 생성되도록 고려가 필요 
    /// 이 과정에서 AI 컴포넌트의 오작동을 막기 위해 몬스터 에이전트들을 임시 비활성화.
    /// </summary>
    private void UnpackLoaderPrefabs()
    {
        GameObject groupPrefab = AssetLoader.Instance.GetLoadedPrefab("InGameLoaderPrefabs");
        if (groupPrefab == null)
        {
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
        EnableAllAgents();
    }

    /// <summary>
    /// 배치가 완료된 몬스터들의 NavMeshAgent 컴포넌트를 활성화처리
    /// </summary>
    private void EnableAllAgents()
    {
        foreach (var agent in _spawnedAgents)
        {
            if (agent != null) agent.enabled = true;
        }
    }
}
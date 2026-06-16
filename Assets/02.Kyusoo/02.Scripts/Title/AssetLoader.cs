using System.Collections;
using System.Collections.Generic; // Dictionary 사용을 위해 필수
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    public static AssetLoader Instance { get; private set; }

    [Header("[ 프로젝트 에셋 보관소 ]")]
    // 🌟 유니티 인스펙터에서 관리할 진짜 프리팹 에셋 리스트!
    [SerializeField] private List<GameObject> _sourcePrefabs;

    // 런타임에 에셋 이름으로 프리팹 원본을 빠르게 꺼내 쓰기 위한 주소록 딕셔너리
    private Dictionary<string, GameObject> _prefabLookUp = new Dictionary<string, GameObject>();

    public float Progress { get; private set; } = 0f;
    public bool IsDone { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 🌟 시작하자마자 리스트에 꽂힌 프리팹들을 이름 기반 딕셔너리로 자동 빌드
            InitPrefabLookUp();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitPrefabLookUp()
    {
        if (_sourcePrefabs == null) return;

        foreach (var prefab in _sourcePrefabs)
        {
            if (prefab == null) continue;

            if (!_prefabLookUp.ContainsKey(prefab.name))
            {
                _prefabLookUp.Add(prefab.name, prefab);
            }
        }
    }

    // 🌟 [중요 추가] 인게임 매니저가 "TerrainHigh 프리팹 원본 좀 줘!" 할 때 꺼내주는 함수
    public GameObject GetLoadedPrefab(string assetName)
    {
        if (_prefabLookUp.TryGetValue(assetName, out GameObject prefab))
        {
            return prefab;
        }

        Debug.LogError($"[AssetLoader] '{assetName}' 이름의 프리팹을 보관소에서 찾을 수 없습니다!");
        return null;
    }

    public void StartLoadingAssets(string[] assetNames)
    {
        Progress = 0f;
        IsDone = false;
        StartCoroutine(LoadAssetsCoroutine(assetNames));
    }

    private IEnumerator LoadAssetsCoroutine(string[] assetNames)
    {
        if (assetNames == null || assetNames.Length == 0)
        {
            Progress = 1f;
            IsDone = true;
            yield break;
        }

        int totalCount = assetNames.Length;

        for (int i = 0; i < totalCount; i++)
        {
            string currentAsset = assetNames[i];
            Debug.Log($"[AssetLoader] {currentAsset} 데이터를 메모리에 로드 중...");

            // -------------------------------------------------------------
            // [실제 적용 시점] 나중에 진짜 어드레서블을 붙이면 
            // _prefabLookUp에 로드된 자원을 런타임에 추가하는 코드가 됨!
            // -------------------------------------------------------------
            yield return new WaitForSeconds(0.4f);

            Progress = (float)(i + 1) / totalCount;
        }

        Debug.Log("[AssetLoader] Terrain과 ParticleManager 로딩 완벽 완료!");
        IsDone = true;
    }
}
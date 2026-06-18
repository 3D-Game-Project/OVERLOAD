//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement; 
//using TMPro;
//using DG.Tweening;

//public class LoadingUI : MonoBehaviour
//{
//    [Header("[로딩 UI]")]
//    [SerializeField] private Slider _loadingSlider;
//    [SerializeField] private TextMeshProUGUI _textStatus;
//    [SerializeField] private TextMeshProUGUI _textPressAnyKey;

//    [Header("[ 로딩 텍스트 연출 ]")]
//    [SerializeField] private float _waveSpeed = 4f;   
//    [SerializeField] private float _waveHeight = 8f;   
//    [SerializeField] private float _waveSpacing = 0.5f;

//    [SerializeField]
//    private string _nextSceneName = "SecondBuildScene";

//    private AsyncOperation _asyncOperation;
//    private bool _isLoadingComplete = false;

//    private string[] _loadTargetAssets = new string[] {"Terrain", "InGameLoaderPrefabs"};

//    [SerializeField] private float _minimumLoadingTime = 5f;
//    [SerializeField] private float _gaugeSmoothSpeed = 1.5f;

//    private void OnEnable()
//    {
//        _isLoadingComplete = false;
//        if (_loadingSlider != null) _loadingSlider.value = 0f;
//        if (_textPressAnyKey != null)
//        {
//            _textPressAnyKey.gameObject.SetActive(false);
//            _textPressAnyKey.color = new Color(_textPressAnyKey.color.r, _textPressAnyKey.color.g, _textPressAnyKey.color.b, 0f);
//        }

//        StartCoroutine(LoadTextWaveCoroutine());

//        StartCoroutine(LoadSceneSequence());
//    }

//    private void Update()
//    {
//        if (_isLoadingComplete && Input.anyKeyDown)
//        {
//            if (_asyncOperation != null)
//            {
//                _asyncOperation.allowSceneActivation = true;
//            }
//        }
//    }

//    private void UpdateStatusText(float progress)
//    {
//        string newText = "";

//        if (progress <= 0.2f) newText = "Map Loading...";
//        else if (progress <= 0.4f) newText = "Enemy Loading...";
//        else if (progress <= 0.6f) newText = "Player Setting...";
//        else if (progress <= 0.8f) newText = "Weapon System Booting...";
//        else if (progress < 1.0f) newText = "Finalizing Data...";
//        else newText = "Loading Complete!";

//        if (_textStatus != null && _textStatus.text != newText)
//        {
//            _textStatus.text = newText;
//        }
//    }

//    private IEnumerator LoadTextWaveCoroutine()
//    {
//        while (!_isLoadingComplete)
//        {
//            if (_textStatus == null) yield return null;

//            _textStatus.ForceMeshUpdate();
//            TMP_TextInfo textInfo = _textStatus.textInfo;
//            int characterCount = textInfo.characterCount;

//            if (characterCount == 0)
//            {
//                yield return null;
//                continue;
//            }

//            for (int i = 0; i < characterCount; i++)
//            {
//                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

//                if (!charInfo.isVisible) continue;

//                int materialIndex = charInfo.materialReferenceIndex;
//                int vertexIndex = charInfo.vertexIndex;
//                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

//                float yOffset = Mathf.Sin(Time.time * _waveSpeed + (i * _waveSpacing)) * _waveHeight;

//                vertices[vertexIndex + 0].y += yOffset; 
//                vertices[vertexIndex + 1].y += yOffset; 
//                vertices[vertexIndex + 2].y += yOffset; 
//                vertices[vertexIndex + 3].y += yOffset; 

//            }

//            for (int i = 0; i < textInfo.meshInfo.Length; i++)
//            {
//                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
//                _textStatus.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
//            }

//            yield return null;
//        }
//    }

//    private IEnumerator LoadSceneSequence()
//    {
//        yield return null;
//        yield return null;

//        float startTime = Time.time;

//        _asyncOperation = SceneManager.LoadSceneAsync(_nextSceneName);
//        _asyncOperation.allowSceneActivation = false;

//        if (AssetLoader.Instance != null)
//        {
//            AssetLoader.Instance.StartLoadingAssets(_loadTargetAssets);
//        }

//        float currentDisplayProgress = 0f;

//        while (true)
//        {
//            float sceneProgress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
//            float assetProgress = AssetLoader.Instance != null ? AssetLoader.Instance.Progress : 0f;
//            float actualTargetProgress = (sceneProgress + assetProgress) * 0.5f;

//            float elapsedTime = Time.time - startTime;
//            if (elapsedTime < _minimumLoadingTime)
//            {
//                actualTargetProgress = Mathf.Min(actualTargetProgress, 0.9f);
//            }

//            currentDisplayProgress = Mathf.MoveTowards(
//                currentDisplayProgress,
//                actualTargetProgress,
//                Time.deltaTime * _gaugeSmoothSpeed
//            );

//            if (_loadingSlider != null) _loadingSlider.value = currentDisplayProgress;
//            UpdateStatusText(currentDisplayProgress);


//            if (sceneProgress >= 1f &&
//                (AssetLoader.Instance == null || AssetLoader.Instance.IsDone) &&
//                elapsedTime >= _minimumLoadingTime &&
//                currentDisplayProgress >= 0.9f)
//            {
//                break;
//            }

//            yield return null;
//        }

//        while (currentDisplayProgress < 1.0f)
//        {
//            currentDisplayProgress = Mathf.MoveTowards(currentDisplayProgress, 1.0f, Time.deltaTime * _gaugeSmoothSpeed * 2f);
//            if (_loadingSlider != null) _loadingSlider.value = currentDisplayProgress;
//            yield return null;
//        }

//        _isLoadingComplete = true;
//        UpdateStatusText(1.0f);
//        if (_loadingSlider != null) _loadingSlider.value = 1f;

//        if (_textPressAnyKey != null)
//        {
//            _textPressAnyKey.gameObject.SetActive(true);
//            _textPressAnyKey.DOFade(1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
//        }
//    }

//    private void OnDestroy()
//    {
//        if (_textPressAnyKey != null) _textPressAnyKey.DOKill();
//    }
//}


using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class LoadingUI : MonoBehaviour
{
    [Header("[로딩 UI]")]
    [SerializeField] private Slider _loadingSlider;
    [SerializeField] private TextMeshProUGUI _textStatus;
    [SerializeField] private TextMeshProUGUI _textPressAnyKey;

    [Header("[ 로딩 텍스트 연출 ]")]
    [SerializeField] private float _waveSpeed = 4f;
    [SerializeField] private float _waveHeight = 8f;
    [SerializeField] private float _waveSpacing = 0.5f;

    [SerializeField] private string _nextSceneName = "SecondBuildScene";

    private AsyncOperation _asyncOperation;
    private bool _isLoadingComplete = false;

    // 🌟 [임시 가상 전환 가이드라인]
    // 20% 단위로 끊어서 상태를 표출하고 1초씩 지연시킬 연출 맵 데이터 세팅
    private struct LoadingPhase
    {
        public float TargetProgress;
        public string StatusText;
    }

    private LoadingPhase[] _loadingPhases = new LoadingPhase[]
    {
        new LoadingPhase { TargetProgress = 0.2f, StatusText = "Map Loading..." },
        new LoadingPhase { TargetProgress = 0.4f, StatusText = "Enemy Loading..." },
        new LoadingPhase { TargetProgress = 0.6f, StatusText = "Player Setting..." },
        new LoadingPhase { TargetProgress = 0.8f, StatusText = "Weapon System Booting..." },
        new LoadingPhase { TargetProgress = 1.0f, StatusText = "Finalizing Data..." }
    };

    private void OnEnable()
    {
        _isLoadingComplete = false;
        if (_loadingSlider != null) _loadingSlider.value = 0f;
        if (_textPressAnyKey != null)
        {
            _textPressAnyKey.gameObject.SetActive(false);
            _textPressAnyKey.color = new Color(_textPressAnyKey.color.r, _textPressAnyKey.color.g, _textPressAnyKey.color.b, 0f);
        }

        StartCoroutine(LoadTextWaveCoroutine());
        StartCoroutine(LoadSceneSequence());
    }

    private void Update()
    {
        if (_isLoadingComplete && Input.anyKeyDown)
        {
            if (_asyncOperation != null)
            {
                _asyncOperation.allowSceneActivation = true;
            }
        }
    }

    // 텍스트 강제 갱신용 서브루틴
    private void SetStatusText(string text)
    {
        if (_textStatus != null)
        {
            _textStatus.text = text;
        }
    }

    private IEnumerator LoadTextWaveCoroutine()
    {
        while (!_isLoadingComplete)
        {
            if (_textStatus == null) yield return null;

            _textStatus.ForceMeshUpdate();
            TMP_TextInfo textInfo = _textStatus.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount == 0)
            {
                yield return null;
                continue;
            }

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                float yOffset = Mathf.Sin(Time.time * _waveSpeed + (i * _waveSpacing)) * _waveHeight;

                vertices[vertexIndex + 0].y += yOffset;
                vertices[vertexIndex + 1].y += yOffset;
                vertices[vertexIndex + 2].y += yOffset;
                vertices[vertexIndex + 3].y += yOffset;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textStatus.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return null;
        }
    }

    private IEnumerator LoadSceneSequence()
    {
        yield return null;
        yield return null;

        // 1. 유니티 백그라운드 씬 로드는 뒤에서 조용히 실행 (프리팹 꼬임 우회)
        _asyncOperation = SceneManager.LoadSceneAsync(_nextSceneName);
        _asyncOperation.allowSceneActivation = false;

        // 🌟 [가상 가동 전환 부]
        // 20% 단위로 텍스트를 명확하게 바꾸고, 그 상태에서 딱 1초씩 코루틴 정밀 대기를 태웁니다.
        // 슬라이더 바 게이지 역시 툭툭 끊기지 않고 부드럽게 해당 페이즈 목표치까지 보간 이동합니다.
        float currentDisplayProgress = 0f;

        for (int i = 0; i < _loadingPhases.Length; i++)
        {
            LoadingPhase currentPhase = _loadingPhases[i];
            SetStatusText(currentPhase.StatusText);

            float phaseStartTime = Time.time;
            float startProgress = currentDisplayProgress;
            float endProgress = currentPhase.TargetProgress;

            // 90% 이전 구간(마지막 가상 정리 직전)까지는 무조건 1초 동안 슬라이더가 점진적으로 차오름
            while (Time.time - phaseStartTime < 1.0f)
            {
                float t = (Time.time - phaseStartTime) / 1.0f;
                currentDisplayProgress = Mathf.Lerp(startProgress, endProgress, t);

                // 실제 유니티 씬 로드가 끝나기 전(0.9f 제한) 상태와 싱크 가림막
                if (endProgress >= 1.0f)
                {
                    currentDisplayProgress = Mathf.Min(currentDisplayProgress, 0.99f);
                }

                if (_loadingSlider != null) _loadingSlider.value = currentDisplayProgress;
                yield return null;
            }

            // 각 페이즈가 끝날 때 완벽 수치 보정 마감
            currentDisplayProgress = endProgress;
            if (_loadingSlider != null) _loadingSlider.value = currentDisplayProgress;
        }

        // 2. 가상 부팅 연출 완료 확인 루프 (실제 유니티 씬 로드도 완료되었는지 체크)
        while (_asyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        // 3. 100% 완전 완벽 정착 마감 처리
        _isLoadingComplete = true;
        SetStatusText("Loading Complete!");
        if (_loadingSlider != null) _loadingSlider.value = 1f;

        // Press Any Key 텍스트 유도 작동
        if (_textPressAnyKey != null)
        {
            _textPressAnyKey.gameObject.SetActive(true);
            _textPressAnyKey.DOKill();
            _textPressAnyKey.DOFade(1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }

    private void OnDestroy()
    {
        if (_textPressAnyKey != null) _textPressAnyKey.DOKill();
    }
}
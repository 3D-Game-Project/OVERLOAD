using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private string _nextSceneName = "SecondBuildScene";

    private AsyncOperation _asyncOperation;
    private bool _isLoadingComplete = false;

    private string[] _loadTargetAssets = new string[] {"Terrain", "InGameLoaderPrefabs"};

    [SerializeField] private float _minimumLoadingTime = 5f;
    [SerializeField] private float _gaugeSmoothSpeed = 1.5f;

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

    private void UpdateStatusText(float progress)
    {
        string newText = "";

        if (progress <= 0.2f) newText = "Map Loading...";
        else if (progress <= 0.4f) newText = "Enemy Loading...";
        else if (progress <= 0.6f) newText = "Player Setting...";
        else if (progress <= 0.8f) newText = "Weapon System Booting...";
        else if (progress < 1.0f) newText = "Finalizing Data...";
        else newText = "Loading Complete!";

        if (_textStatus != null && _textStatus.text != newText)
        {
            _textStatus.text = newText;
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

        float startTime = Time.time;

        _asyncOperation = SceneManager.LoadSceneAsync(_nextSceneName);
        _asyncOperation.allowSceneActivation = false;

        if (AssetLoader.Instance != null)
        {
            AssetLoader.Instance.StartLoadingAssets(_loadTargetAssets);
        }

        float currentDisplayProgress = 0f;

        while (true)
        {
            float sceneProgress = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            float assetProgress = AssetLoader.Instance != null ? AssetLoader.Instance.Progress : 0f;
            float actualTargetProgress = (sceneProgress + assetProgress) * 0.5f;

            float elapsedTime = Time.time - startTime;
            if (elapsedTime < _minimumLoadingTime)
            {
                actualTargetProgress = Mathf.Min(actualTargetProgress, 0.9f);
            }

            currentDisplayProgress = Mathf.MoveTowards(
                currentDisplayProgress,
                actualTargetProgress,
                Time.deltaTime * _gaugeSmoothSpeed
            );

            if (_loadingSlider != null) _loadingSlider.value = currentDisplayProgress;
            UpdateStatusText(currentDisplayProgress);

            
            if (sceneProgress >= 1f &&
                (AssetLoader.Instance == null || AssetLoader.Instance.IsDone) &&
                elapsedTime >= _minimumLoadingTime &&
                currentDisplayProgress >= 0.9f)
            {
                break;
            }

            yield return null;
        }

        while (currentDisplayProgress < 1.0f)
        {
            currentDisplayProgress = Mathf.MoveTowards(currentDisplayProgress, 1.0f, Time.deltaTime * _gaugeSmoothSpeed * 2f);
            if (_loadingSlider != null) _loadingSlider.value = currentDisplayProgress;
            yield return null;
        }

        _isLoadingComplete = true;
        UpdateStatusText(1.0f);
        if (_loadingSlider != null) _loadingSlider.value = 1f;

        if (_textPressAnyKey != null)
        {
            _textPressAnyKey.gameObject.SetActive(true);
            _textPressAnyKey.DOFade(1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }

    private void OnDestroy()
    {
        if (_textPressAnyKey != null) _textPressAnyKey.DOKill();
    }
}

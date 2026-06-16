using GifImporter;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using GifImporter;

[System.Serializable]
public struct GuidePage
{
    // 챕터별 타이틀, 이미지, 설명 교체 용도
    public string ChapterTitle;
    public Gif GuideGif;
    [TextArea(3, 5)] public string Description;
}

public class GuideUI : MonoBehaviour
{
    [SerializeField] private GuidePage[] _guidePages;

    [Header("GuidePages와 연결할 데이터들")]
    [SerializeField] private TextMeshProUGUI _chapterTitle;
    [SerializeField] private TextMeshProUGUI _chapterDesc;
    [SerializeField] private TextMeshProUGUI _pageIndex;
    [SerializeField] private Image _chapterImage;

    [Header("GIF 플레이어 컴포넌트")]
    [SerializeField] private GifPlayer _gifPlayer;

    [Header("하단 영역 챕터넘기기 버튼 + 상단 Close")]
    [SerializeField] private Button _previous;
    [SerializeField] private Button _next;
    [SerializeField] private Button _close;

    private int _currentIdex = 0;

    private void Awake()
    {
        if (_previous != null) _previous.onClick.AddListener(OnClickPrevious);
        if (_next != null) _next.onClick.AddListener(OnClickNext);
        if (_close != null) _close.onClick.AddListener(OnClickClose);
    }

    private void OnEnable()
    {
        _currentIdex = 0;
        UpdatePageUI();
    }

    private void UpdatePageUI()
    {
        if (_guidePages == null || _guidePages.Length == 0) return;

        GuidePage currentPage = _guidePages[_currentIdex];

        if (_chapterTitle != null) _chapterTitle.text = currentPage.ChapterTitle;
        if (_chapterDesc != null) _chapterDesc.text = currentPage.Description;

        if (currentPage.GuideGif != null && _gifPlayer != null)
        {
            if (_chapterImage != null) _chapterImage.gameObject.SetActive(true);

            _gifPlayer.enabled = true;
            _gifPlayer.Gif = currentPage.GuideGif;
        }
        else
        {
            if (_gifPlayer != null) _gifPlayer.enabled = false; 
            
        }

        if (_pageIndex != null)
        {
            _pageIndex.text = $"{_currentIdex + 1} / {_guidePages.Length}";
        }

        if (_previous != null) _previous.interactable = (_currentIdex > 0);
        if (_next != null) _next.interactable = (_currentIdex < _guidePages.Length - 1);
    }

    private void OnClickPrevious()
    {
        if (_currentIdex > 0)
        {
            _currentIdex--;
            UpdatePageUI();
        }
    }

    private void OnClickNext()
    {
        if (_currentIdex < _guidePages.Length - 1)
        {
            _currentIdex++;
            UpdatePageUI();
        }
    }

    private void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}

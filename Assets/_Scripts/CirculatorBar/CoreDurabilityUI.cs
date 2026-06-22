using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoreDurabilityUI : MonoBehaviour
{
    [SerializeField] private DurabilityController _targetCore;
    [SerializeField] private GameObject _uiRoot; 
    [SerializeField] private Image _fillImage;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.red;
    [SerializeField] private Color _warningColor = Color.yellow;

    private Tween _fillTween;
    private Tween _blinkTween;
    private bool _isFirstUpdate = true;

    private void OnEnable()
    {
        _isFirstUpdate = true;
        if (_targetCore != null) _targetCore.OnDurabilityChanged += UpdateGauge;
        if (_fillImage != null) _fillImage.color = _normalColor;
    }

    private void OnDisable()
    {
        if (_targetCore != null) _targetCore.OnDurabilityChanged -= UpdateGauge;
        _fillTween?.Kill();
        _blinkTween?.Kill();
    }

    private void UpdateGauge(float currentDurability, float maxDurability)
    {
        if (_fillImage == null) return;

        if (_isFirstUpdate && currentDurability > 0f)
        {
            if (_uiRoot != null) _uiRoot.SetActive(true);
            _isFirstUpdate = false;
        }

        float targetRatio = currentDurability / maxDurability;

        _fillTween?.Kill();
        _fillTween = _fillImage.DOFillAmount(targetRatio, 0.25f).SetEase(Ease.OutCubic);

        if (targetRatio <= 0f)
        {
            _blinkTween?.Kill();
            _blinkTween = null;
            if (_uiRoot != null) _uiRoot.SetActive(false);
            return;
        }

        if (targetRatio < 0.3f)
        {
            if (_blinkTween == null || !_blinkTween.IsActive())
            {
                _blinkTween?.Kill();
                _blinkTween = _fillImage.DOColor(_warningColor, 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }
        else
        {
            if (_blinkTween != null && _blinkTween.IsActive())
            {
                _blinkTween?.Kill();
                _blinkTween = null;
                _fillImage.DOColor(_normalColor, 0.3f);
            }
        }
    }
}
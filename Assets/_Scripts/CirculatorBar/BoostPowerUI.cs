using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BoostPowerUI : MonoBehaviour
{
    // 🌟 사용하시는 실제 부스트 컨트롤러 스크립트 이름으로 변경하세요!
    [SerializeField] private CoreEnergyController _coreEnergyController;

    [SerializeField] private GameObject _uiRoot; // BoostPower 빈 부모 오브젝트 연결
    [SerializeField] private Image _fillImage;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = new Color(0f, 0.8f, 1f);
    [SerializeField] private Color _warningColor = Color.yellow;
    [SerializeField] private Color _exhaustedColor = Color.red;

    private Tween _fillTween; // 규격 통일
    private Tween _blinkTween;
    private bool _isFirstUpdate = true;

    private void OnEnable()
    {
        _isFirstUpdate = true;

        if (_coreEnergyController != null)
        {
            _coreEnergyController.OnEnergyChanged += UpdateBoostBar;
            _coreEnergyController.OnEnergyExhausted += ExhaustBoostBar;
        }

        if (_fillImage != null)
        {
            _fillImage.color = _normalColor;
            _fillImage.DOFade(1f, 0f);
        }
    }

    private void OnDisable()
    {
        if (_coreEnergyController != null)
        {
            _coreEnergyController.OnEnergyChanged -= UpdateBoostBar;
            _coreEnergyController.OnEnergyExhausted -= ExhaustBoostBar;
        }

        _fillTween?.Kill();
        _blinkTween?.Kill();
    }

    private void UpdateBoostBar(float currentBoost, float maxBoost)
    {
        if (_fillImage == null) return;

        // 최초 1회 업데이트 시 UI 켜기
        if (_isFirstUpdate && currentBoost > 0f)
        {
            if (_uiRoot != null) _uiRoot.SetActive(true);
            _isFirstUpdate = false;
        }

        float targetRatio = currentBoost / maxBoost;

        _fillTween?.Kill();
        // 🌟 정석적인 DOFillAmount 사용
        _fillTween = _fillImage.DOFillAmount(targetRatio, 0.2f).SetEase(Ease.OutCubic);

        if (targetRatio > 0f && targetRatio < 0.3f)
        {
            if (_blinkTween == null || !_blinkTween.IsActive())
            {
                _blinkTween?.Kill();
                _blinkTween = _fillImage.DOColor(_warningColor, 0.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else if (targetRatio >= 0.3f)
        {
            if (_blinkTween != null && _blinkTween.IsActive())
            {
                _blinkTween?.Kill();
                _blinkTween = null;
                _fillImage.DOColor(_normalColor, 0.3f);
            }
        }
    }

    private void ExhaustBoostBar(bool isExhausted)
    {
        if (_fillImage == null) return;

        if (isExhausted)
        {
            _fillImage.color = _exhaustedColor;

            _blinkTween?.Kill();
            _blinkTween = _fillImage.DOFade(0.3f, 0.25f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            _blinkTween?.Kill();
            _blinkTween = null;

            _fillImage.DOFade(1f, 0.15f);
            _fillImage.DOColor(_normalColor, 0.3f);
        }
    }
}
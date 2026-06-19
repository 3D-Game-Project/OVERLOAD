using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OverheatUI : MonoBehaviour
{
    [SerializeField] private OverheatController _overheatController;

    [SerializeField] private Slider _energyBarSlider;
    [SerializeField] private Image _fillImage;

    [SerializeField] private Color _normalColor = new Color(0f, 0.8f, 1f);
    [SerializeField] private Color _overheatColor = Color.red;

    private Tween _sliderTween;
    private Tween _blinkTween;

    private void OnEnable()
    {
        if (_overheatController != null)
        {
            _overheatController.OnEnergyChanged += UpdateEnergyBar;
            _overheatController.OnOverHeated += OverheatEnergyBar;
        }

        if (_fillImage != null) _fillImage.color = _normalColor;
    }

    private void OnDisable()
    {
        if (_overheatController != null)
        {
            _overheatController.OnEnergyChanged -= UpdateEnergyBar;
            _overheatController.OnOverHeated -= OverheatEnergyBar;
        }

        _sliderTween?.Kill();
        _blinkTween?.Kill();
    }

    // 에너지 변화에 따른 UI연출
    private void UpdateEnergyBar(float currentEnergy, float maxEnergy)
    {
        if (_energyBarSlider == null) return;

        float targetRatio = currentEnergy / maxEnergy;

        _sliderTween?.Kill();

        _sliderTween = _energyBarSlider.DOValue(targetRatio, 0.2f).SetEase(Ease.OutCubic);
    }

    // 과열되었을 때, 에너지가 차오르면 깜빡이면서 차오르도록 연출
    private void OverheatEnergyBar(bool isOverheated)
    {
        if (_fillImage == null) return;

        if (isOverheated)
        {
            _fillImage.color = _overheatColor;

            _blinkTween?.Kill(); 
            _blinkTween = _fillImage.DOFade(0.3f, 0.25f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else
        {
            _blinkTween?.Kill();

            _fillImage.DOFade(1f, 0.15f);
            _fillImage.DOColor(_normalColor, 0.3f);
        }
    }
}

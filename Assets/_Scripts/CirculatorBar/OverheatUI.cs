using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OverheatUI : MonoBehaviour
{
    [SerializeField] private OverheatController _overheatController;

    [SerializeField] private GameObject _uiRoot; 
    [SerializeField] private Image _fillImage;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = new Color(0f, 0.8f, 1f);
    [SerializeField] private Color _overheatColor = Color.red;

    private Tween _fillTween; 
    private Tween _blinkTween;
    private bool _isFirstUpdate = true;

    private void OnEnable()
    {
        _isFirstUpdate = true;

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

        _fillTween?.Kill();
        _blinkTween?.Kill();
    }

    // 에너지 변화에 따른 UI 연출
    // 0에서부터 1로 점차 차오르게 처리하기
    private void UpdateEnergyBar(float currentEnergy, float maxEnergy)
    {
        if (_fillImage == null) return;

        if (_isFirstUpdate)
        {
            if (_uiRoot != null) _uiRoot.SetActive(true);
            _isFirstUpdate = false;
        }

        
        float targetRatio = 1f - (currentEnergy / maxEnergy);

        _fillTween?.Kill();

        _fillTween = _fillImage.DOFillAmount(targetRatio, 0.2f).SetEase(Ease.OutCubic);
    }

    // 과열되었을 때, 깜빡이면서 차오르도록 연출
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
            _blinkTween = null;

            _fillImage.DOFade(1f, 0.15f);
            _fillImage.DOColor(_normalColor, 0.3f);
        }
    }
}
using DG.Tweening;
using UnityEngine;

public class AimStateController : MonoBehaviour
{
    [Header("Camera Controller")]
    [SerializeField] private PlayerCameraController _cameraController;

    [Header("Alpha Settings")]
    [SerializeField] private float _defaultAlpha = 0.3f;
    [SerializeField] private float _aimingAlpha = 0.7f;

    private CanvasGroup _canvasGroup;
    private Tween _alphaTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (_cameraController != null)
        {
            _cameraController.OnAimStateChanged += UpdateGroupAlpha;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = _defaultAlpha;
        }
    }

    private void OnDisable()
    {
        if (_cameraController != null)
        {
            _cameraController.OnAimStateChanged -= UpdateGroupAlpha;
        }

        _alphaTween?.Kill();
    }

    private void UpdateGroupAlpha(bool isAiming)
    {
        if (_canvasGroup == null) return;

        float targetAlpha = isAiming ? _aimingAlpha : _defaultAlpha;

        _alphaTween?.Kill();
        _alphaTween = _canvasGroup.DOFade(targetAlpha, 0.2f).SetEase(Ease.OutCubic);
    }
}
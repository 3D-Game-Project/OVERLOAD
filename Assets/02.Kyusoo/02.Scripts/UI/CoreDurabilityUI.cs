using UnityEngine;
using UnityEngine.UI; 
using DG.Tweening;

public class CoreDurabilityUI : MonoBehaviour
{
    [SerializeField] private DurabilityController _targetCore;

    [SerializeField] private Image _background;
    [SerializeField] private Image _gauge;

    [SerializeField] private Vector3 _playerPos = new Vector3(-400f, 100f, 0f);
    [SerializeField] private Vector3 _enemyPos = new Vector3(0f, 75f, 0f);

    [SerializeField] private Color _warningColor = Color.yellow;

    [SerializeField] private Color _normalColor = Color.red;

    private Tweener _blinkTweener;
    private bool _isFirstUpdate = true;

    private void OnEnable()
    {
        _isFirstUpdate = true; 

        if (_targetCore != null)
        {
            _targetCore.OnDurabilityChanged += UpdateGauge;
        }
    }

    private void OnDisable()
    {
        if (_targetCore != null)
        {
            _targetCore.OnDurabilityChanged -= UpdateGauge;
        }

        if (_gauge != null)
        {
            _gauge.DOKill();
        }
    }

    private void LateUpdate()
    {
        if (_targetCore != null && Camera.main != null)
        {
            Vector3 worldPos = _targetCore.transform.position;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0f)
            {
                _background.gameObject.SetActive(false);
                _gauge.gameObject.SetActive(false);
                return;
            }

            if(gameObject.transform.root.gameObject.layer == 6)
            {
                screenPos.x += _playerPos.x;
                screenPos.y += _playerPos.y;
            }

            if(gameObject.transform.root.gameObject.layer == 7)
            {
                screenPos.x += _enemyPos.x;
                screenPos.y += _enemyPos.y;
            }
           

            transform.position = screenPos;
        }
    }

    private void UpdateGauge(float currentDurability, float maxDurability)
    {
        if (_gauge == null) return;

        if (_isFirstUpdate && currentDurability > 0f)
        {
            _background.gameObject.SetActive(true);
            _gauge.gameObject.SetActive(true);
            _isFirstUpdate = false; 
        }

        float targetFillAmount = currentDurability / maxDurability;


        DOTween.Kill(_gauge, typeof(Image).GetField("fillAmount"));
        _gauge.DOFillAmount(targetFillAmount, 0.25f).SetEase(Ease.OutQuad);

        _gauge.DOComplete(true);

        if (targetFillAmount <= 0f)
        {
            if (_blinkTweener != null && _blinkTweener.IsActive())
            {
                _blinkTweener.Kill();
                _blinkTweener = null;
            }

            _background.gameObject.SetActive(false);
            _gauge.gameObject.SetActive(false);

            return; 
        }

        // 내구도 30% 미만일 때 동작하는 깜빡이 연출입니다.
        // 부스트게이지도 일정수치밑으로 내려가면 이런 연출이 있으면 좋겠다해서 추가해보았습니다.
        if (targetFillAmount < 0.3f && targetFillAmount > 0f)
        {
            if (_blinkTweener == null || !_blinkTweener.IsActive())
            {
                _blinkTweener = _gauge.DOColor(_warningColor, 0.1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetTarget(_gauge);

            }
        }
        // 다시 채워졌을 때
        else
        {
            if (_blinkTweener != null && _blinkTweener.IsActive())
            {
                _blinkTweener.Kill();
                _blinkTweener = null;

                _gauge.color = _normalColor;
            }
        }

    }
}

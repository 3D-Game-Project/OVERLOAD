using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityRowUI : MonoBehaviour
{
    [Header("Part")]
    [SerializeField] private Image _partIcon;
    [SerializeField] private TMP_Text _partNameText;

    [Header("Durability")]
    [SerializeField] private Image _gaugeImage;
    [SerializeField] private TMP_Text _durabilityRatioText;

    public void Setup(
        Sprite icon,
        string partName,
        float currentDurability,
        float maxDurability)
    {
        if (_partIcon != null)
        {
            _partIcon.sprite = icon;
            _partIcon.enabled = icon != null;
            _partIcon.preserveAspect = true;
        }

        if (_partNameText != null)
            _partNameText.text = partName;

        float safeMax = Mathf.Max(0f, maxDurability);
        float safeCurrent = Mathf.Clamp(
            currentDurability,
            0f,
            safeMax);

        if (_durabilityRatioText != null)
        {
            _durabilityRatioText.text =
                $"{safeCurrent:0} / {safeMax:0}";
        }

        float ratio =
            safeMax > 0f ? safeCurrent / safeMax : 0f;

        if (_gaugeImage != null)
        {
            _gaugeImage.DOKill();
            _gaugeImage.fillAmount = 0f;

            _gaugeImage
                .DOFillAmount(ratio, 0.3f)
                .SetEase(Ease.OutCubic);
        }
    }

    private void OnDisable()
    {
        if (_gaugeImage != null)
            _gaugeImage.DOKill();
    }
}
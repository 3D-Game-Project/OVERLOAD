using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _totalDamageText;
    [SerializeField] private TextMeshProUGUI _activePartsCountText;

    [SerializeField] private Transform _contentContainer;

    [SerializeField] private GameObject _durabilityRowPrefab;

    private List<GameObject> _rows = new List<GameObject>();

    private void OnEnable()
    {
        RefreshWeaponSection();
        RefreshDurabilitySection();
    }

    private void RefreshWeaponSection()
    {
        GameObject playerRoot = GameObject.FindWithTag("Player");
        if (playerRoot == null) return;

        float finalAttackPower = 0f;
        int activeWeaponsCount = 0;

        PlayerDefaultAttack defaultDamage = playerRoot.GetComponent<PlayerDefaultAttack>();
        if (defaultDamage != null)
        {
            finalAttackPower += defaultDamage.Damage;
        }

        FireManager[] equippedWeapons = playerRoot.GetComponentsInChildren<FireManager>(true);
        activeWeaponsCount = equippedWeapons.Length;

        foreach (FireManager weapon in equippedWeapons)
        {
            if (weapon != null && weapon.AttackPartsData != null)
            {
                finalAttackPower += weapon.AttackPartsData.Damage;
            }
        }

        if (_totalDamageText != null)
        {
            _totalDamageText.text = $"{finalAttackPower} DMG";
        }

        if (_activePartsCountText != null)
        {
            _activePartsCountText.text = $"{activeWeaponsCount} Parts Active";
        }
    }

    private void RefreshDurabilitySection()
    {
        if (_contentContainer == null || _durabilityRowPrefab == null) return;

        foreach (var row in _rows)
        {
            if (row != null) Destroy(row);
        }
        _rows.Clear();

        GameObject playerRoot = GameObject.FindWithTag("Player");
        if (playerRoot == null) return;

        DurabilityController[] equippedParts = playerRoot.GetComponentsInChildren<DurabilityController>(true);

        foreach (DurabilityController equippedPart in equippedParts)
        {
            GameObject durabilityRow = Instantiate(_durabilityRowPrefab, _contentContainer);
            _rows.Add(durabilityRow);

            durabilityRow.SetActive(true);

            TextMeshProUGUI partNameText = durabilityRow.transform.Find("PartName")?.GetComponent<TextMeshProUGUI>();
            Image gaugeImage = durabilityRow.transform.Find("Slider/Gauge")?.GetComponent<Image>();
            TextMeshProUGUI valueText = durabilityRow.transform.Find("DurabilityRatio")?.GetComponent<TextMeshProUGUI>();

            if (partNameText != null)
            {
                string rawName = equippedPart.gameObject.name;
                string processedName = rawName;

                if (equippedPart.gameObject == playerRoot)
                {
                    processedName = "Core";
                }
                else if (rawName.StartsWith("Weapon_"))
                {
                    processedName = rawName.Replace("Weapon_", "");
                }

                if (processedName.Contains("(Clone)"))
                {
                    processedName = processedName.Replace("(Clone)", "").Trim();
                }

                partNameText.text = processedName;
            }

            float current = equippedPart.CurrentDurability;
            float max = equippedPart.MaxDurability;

            if (valueText != null)
            {
                valueText.text = $"{current} / {max}";
            }

            float ratio = Mathf.Clamp01(current / max);

            if (gaugeImage != null)
            {
                gaugeImage.fillAmount = 0f;

                gaugeImage.DOFillAmount(ratio, 0.3f).SetEase(Ease.OutCubic);
            }
        }
    }
}

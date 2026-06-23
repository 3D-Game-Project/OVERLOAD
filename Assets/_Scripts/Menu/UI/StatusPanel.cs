using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _totalDamageText;
    [SerializeField] private TextMeshProUGUI _activePartsCountText;

    [SerializeField] private Transform _contentContainer;

    [SerializeField] private GameObject _durabilityRowPrefab;

    [SerializeField] private Sprite _coreIcon;

    private List<GameObject> _rows = new List<GameObject>();

    private void OnEnable()
    {
        RefreshWeaponSection();
        RefreshDurabilitySection();
    }

    /// <summary>
    ///  장착된 무기 파츠 고유의 공격력과 플레이어의 기본공격력을 합산한 정보를 제공
    ///  무기파츠를 얼마나 장착하고 있는지도 파악
    /// </summary>
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

    /// <summary>
    /// DurabilityController를 가지고있는 모든 파츠(코어 포함)의 내구도 현황과 파츠이름, 내구도 비율을 설정하여 표시
    /// 장착된 파츠의 핵심정보만 표시하는 방식
    /// 무기파츠의 경우 Weapon_, (Clone)등의 정보가 붙는데 불필요하다고 판단하여 제거처리진행
    /// 추후 개선한다면 부스트나 다리모듈도 마찬가지로 불필요한 텍스트 제거할 예정
    /// </summary>
    private void RefreshDurabilitySection()
    {
        if (_contentContainer == null ||
            _durabilityRowPrefab == null)
        {
            return;
        }

        foreach (GameObject row in _rows)
        {
            if (row != null)
                Destroy(row);
        }

        _rows.Clear();

        GameObject playerRoot = GameObject.FindWithTag("Player");

        if (playerRoot == null)
            return;

        DurabilityController[] equippedParts =
            playerRoot.GetComponentsInChildren<DurabilityController>(true);

        foreach (DurabilityController durability in equippedParts)
        {
            if (durability == null)
                continue;

            GameObject rowObject = Instantiate(
                _durabilityRowPrefab,
                _contentContainer);

            _rows.Add(rowObject);
            rowObject.SetActive(true);

            DurabilityRowUI rowUI =
                rowObject.GetComponent<DurabilityRowUI>();

            if (rowUI == null)
            {
                Debug.LogWarning(
                    $"[StatusPanel] {rowObject.name}에 " +
                    "DurabilityRowUI가 없습니다.");

                continue;
            }

            bool isCore =
                durability.DurabilityType == DurabilityType.Core;

            PartsData partData = durability.PartsData;

            Sprite icon;
            string partName;

            if (isCore)
            {
                icon = _coreIcon;
                partName = "Core";
            }
            else if (partData != null)
            {
                icon = partData.PartsIcon;
                partName = partData.PartsName;
            }
            else
            {
                icon = null;
                partName = durability.gameObject.name
                    .Replace("(Clone)", "")
                    .Trim();
            }

            rowUI.Setup(
                icon,
                partName,
                durability.CurrentDurability,
                durability.MaxDurability);
        }
    }
}

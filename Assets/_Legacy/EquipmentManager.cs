using System.Collections.Generic;
using UnityEngine;

public class CharacterEquipment : MonoBehaviour
{
    [Header("Layer Setup")]
    [SerializeField] private LayerMask _myTargetLayer;

    [Header("All Equipment Slots")]
    [SerializeField] private List<EquipmentSlot> _equipmentSlots = new List<EquipmentSlot>();

    public void EquipPart(PartsData newPartsData, string targetSlotName)
    {
        if (newPartsData == null || newPartsData.PartsPrefab == null) return;

        EquipmentSlot targetSlot = _equipmentSlots.Find(slot => slot.SlotName == targetSlotName);

        if (targetSlot.Anchor == null || targetSlot.EquipedType != newPartsData.PartsType)
        {
            Debug.LogWarning($"[{targetSlotName}] 슬롯이 없거나 {newPartsData.PartsType} 타입을 장착할 수 없습니다.");
            return;
        }

        foreach (Transform child in targetSlot.Anchor) Destroy(child.gameObject);

        GameObject newPartsObj = Instantiate(newPartsData.PartsPrefab, targetSlot.Anchor);
        newPartsObj.transform.localPosition = Vector3.zero;
        newPartsObj.transform.localRotation = Quaternion.identity;

        InitializePartComponent(newPartsObj, newPartsData);

        if (TryGetComponent(out EnemyBehaviorBridge enemyBridge)) enemyBridge.UpdateEquippedWeapons();
    }


    private void InitializePartComponent(GameObject partsObj, PartsData data)
    {
        // 공통: 모든 부품은 적 탄환에 닿아 꺼지는 걸 막기 위해 주인의 레이어를 따르게 합니다.
        partsObj.layer = gameObject.layer;
        foreach (Transform child in partsObj.transform) child.gameObject.layer = gameObject.layer;

        // 무기 파츠일 경우 (FireManager가 붙어있음)
        if (partsObj.TryGetComponent(out FireManager fireManager))
        {
            fireManager.SetWeaponOwner(_myTargetLayer, gameObject.layer);
        }

        // 방어구 파츠일 경우
        /*
        if (partsObj.TryGetComponent(out ArmorParts armor))
        {
            armor.SetArmorOwner(data.SomeStat); 
        }
        */
    }
}
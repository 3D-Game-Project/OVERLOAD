using UnityEngine;
using System.Collections.Generic;

public class DropRuntime
{
    private int _maxDropCount;

    // 사망시 파츠 드랍을 결정하는 함수
    // EnemyData의 DropParts 리스트에서 각 파츠별로 20%의 드랍확률을 적용.
    // 여러 파츠중 _maxDropCount개수만큼 이미 드랍리스트에 포함되었다면 나머지는 자동 드랍되지않게 처리
    // 추가로 randomIndex에 파츠의 레어리티를 고려하여 Switch문을 활용하여 드랍 확률을 조정하는 것을 고려
    // 최종 _dropList에 드랍될 파츠가 존재한다면 해당 파츠들을 Instantiate하여 드랍하는 함수 호출
    public void DropParts(EnemyData _enemyData, Vector3 spawnPosition, List<PartsData> destroyedParts)
    {
        if (_enemyData == null || _enemyData.DropParts == null)
        {
            Debug.LogError("EnemyData에 DropParts가 존재하지않음.");
            return;
        }

        List<PartsData> availablePool = new List<PartsData>(_enemyData.DropParts);

        if (destroyedParts != null && destroyedParts.Count > 0)
        {
            foreach (PartsData brokenPart in destroyedParts)
            {
                if (availablePool.Contains(brokenPart))
                {
                    availablePool.Remove(brokenPart);
                }
            }
        }

        List<PartsData> _dropList = new List<PartsData>();
        _maxDropCount = Random.Range(1, 3);

        for (int i = 0; i < availablePool.Count; i++)
        {
            if (_dropList.Count == _maxDropCount) break;

            _dropList.Add(availablePool[i]);
        }

        if (_dropList.Count > 0)
        {
            CreateDropParts(_dropList, spawnPosition);
        }

        // DropCurrencyItems로 이전
        //if (_enemyData.DropItems != null && _enemyData.DropItems.Count > 0)
        //{
        //    foreach (DropItem dropData in _enemyData.DropItems)
        //    {
        //        if (dropData == null || dropData.ItemPrefab == null) continue;

        //        float currencyRoll = Random.Range(0f, 100f);
        //        if (currencyRoll <= dropData.DropChance)
        //        {
        //            CreateDropCurrency(dropData, spawnPosition);
        //        }
        //    }
        //}
        DropCurrencyItems(_enemyData, spawnPosition);
    }

    private void DropCurrencyItems(EnemyData enemyData, Vector3 spawnPosition)
    {
        if (enemyData == null)
            return;

        if (enemyData.DropItems == null || enemyData.DropItems.Count <= 0)
            return;

        foreach (DropItem dropData in enemyData.DropItems)
        {
            if (dropData == null || dropData.ItemPrefab == null)
                continue;

            float currencyRoll = Random.Range(0f, 100f);

            if (currencyRoll <= dropData.DropChance)
            {
                CreateDropCurrency(dropData, spawnPosition);
            }
        }
    }

    // Ground layer가 붙은 바닥위치확인하기
    private Vector3 GetSpawnDropPos(Vector3 targetPosition)
    {
        Vector3 rayStart = targetPosition + new Vector3(0f, 3f, 0f);
        float rayDistance = 20f; 

        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayerMask))
        {
            return hit.point;
        }

        return targetPosition;
    }


    // 필드에 생성시킬 재화와 생성위치를 받아 프리팹 생성하는 함수
    private void CreateDropCurrency(DropItem dropData, Vector3 centerPosition)
    {
        GameObject currencyObj = Object.Instantiate(dropData.ItemPrefab);

        Currency dropCurrency = currencyObj.AddComponent<Currency>();
        dropCurrency.Initialize(dropData);

        BoxCollider collider = currencyObj.GetComponent<BoxCollider>();
        if (collider == null) collider = currencyObj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(1.5f, 1.5f, 1.5f);

        currencyObj.transform.localScale = Vector3.one * 1.5f;

        Vector3 randomOffset = Random.insideUnitSphere * 5f;
        randomOffset.y = 0f;
        Vector3 scatterPosition = centerPosition + randomOffset;

        Vector3 finalGroundPos = GetSpawnDropPos(scatterPosition);

        currencyObj.transform.position = finalGroundPos + new Vector3(0f, 0.4f, 0f);
    }


    // 필드에 생성시킬 파츠와 생성위치를 받아 프리팹을 생성하는 함수
    // 위치의 경우 몬스터의 위치를 기준으로 1만큼 랜덤한 위치에 생성되도록 처리
    // Collider를 적용하는 이유는 드랍처리를 진행할 때, OnTriggerEnter를 진행하기 위해 추가
    // Collider의 최초 크기가 작은것을 고려하여 size center를 조정
    //private void CreateDropParts(List<PartsData> _dropList, Vector3 spawnPosition)
    //{
    //    Debug.Log($"드랍 파츠 아이템 생성");
    //    for (int i = 0; i < _dropList.Count; i++)
    //    {
    //        GameObject dropPart = Object.Instantiate(_dropList[i].PartsPrefab);
    //        dropPart.AddComponent<BoxCollider>();
    //        BoxCollider collider = dropPart.GetComponent<BoxCollider>();
    //        if (collider != null)
    //        {
    //            collider.isTrigger = true;
    //            collider.center = new Vector3(-0.3f, 0, 0.6f);
    //            collider.size = new Vector3(1f, 1f, 3f);
    //        }
    //        dropPart.transform.localScale = Vector3.one;

    //        Vector3 randomOffset = Random.insideUnitSphere * 5f;
    //        randomOffset.y = 0f;
    //        Vector3 scatterPosition = spawnPosition + randomOffset;

    //        Vector3 finalGroundPos = GetSpawnDropPos(scatterPosition);

    //        dropPart.transform.position = finalGroundPos + new Vector3(0f, 0.6f, 0f);
    //    }
    //}

    private void CreateDropParts(List<PartsData> _dropList, Vector3 spawnPosition)
    {
        Debug.Log("드랍 파츠 아이템 생성");

        for (int i = 0; i < _dropList.Count; i++)
        {
            PartsData partsData = _dropList[i];

            if (partsData == null)
                continue;

            CreateDropPart(
                partsData,
                spawnPosition,
                partsData.MaxDurability
            );
        }
    }

    public void DropInventoryPart(InventoryPartItem partItem, Vector3 spawnPosition)
    {
        if (partItem == null || partItem.PartsData == null)
        {
            Debug.LogWarning("[DropRuntime] 드랍할 InventoryPartItem 또는 PartsData가 없습니다.");
            return;
        }

        CreateDropPart(
            partItem.PartsData,
            spawnPosition,
            partItem.CurrentDurability
        );
    }

    private void CreateDropPart(PartsData partsData, Vector3 centerPosition, float currentDurability)
    {
        if (partsData == null || partsData.PartsPrefab == null)
        {
            Debug.LogWarning("[DropRuntime] 드랍할 PartsData 또는 PartsPrefab이 없습니다.");
            return;
        }

        GameObject dropPart = Object.Instantiate(partsData.PartsPrefab);

        BoxCollider collider = dropPart.GetComponent<BoxCollider>();
        if (collider == null)
            collider = dropPart.AddComponent<BoxCollider>();

        collider.isTrigger = true;
        collider.center = new Vector3(-0.3f, 0f, 0.6f);
        collider.size = new Vector3(1f, 1f, 3f);

        dropPart.transform.localScale = Vector3.one;

        DurabilityController durability =
            dropPart.GetComponent<DurabilityController>();

        if (durability == null)
            durability = dropPart.GetComponentInChildren<DurabilityController>();

        if (durability != null)
        {
            durability.InitializeDroppedPart(partsData, currentDurability);
        }

        DroppedPartItem droppedPartItem = dropPart.GetComponent<DroppedPartItem>();

        if (droppedPartItem == null)
            droppedPartItem = dropPart.AddComponent<DroppedPartItem>();

        droppedPartItem.Initialize(partsData, currentDurability);

        Vector3 randomOffset = Random.insideUnitSphere * 2f;
        randomOffset.y = 0f;

        Vector3 scatterPosition = centerPosition + randomOffset;
        Vector3 finalGroundPos = GetSpawnDropPos(scatterPosition);

        dropPart.transform.position = finalGroundPos + new Vector3(0f, 0.6f, 0f);

        Debug.Log(
            $"[DropRuntime] 인벤토리 파츠 드랍: {partsData.PartsName} / Durability: {currentDurability}"
        );
    }

    /// <summary>
    /// 장착된 걸 기준으로 parts를 드랍
    /// </summary>
    public void DropAttachedPartsFromUnit(GameObject unitRoot, EnemyData enemyData, Vector3 spawnPosition)
    {
        if (unitRoot == null)
            return;

        // 드랍할 파츠 리스트
        List<GameObject> droppedObjects = new List<GameObject>();
        int droppedPartCount = 0;

        AttachmentSlot[] slots = unitRoot.GetComponentsInChildren<AttachmentSlot>(true);

        foreach(AttachmentSlot slot in slots)
        {
            if (slot == null)
                return;

            if (!slot.HasPart)
                continue;

            GameObject attachedObject = slot.AttachedObject;

            if (attachedObject == null)
                continue;

            // 같은 파츠 오브젝트가 중복 슬롯에서 잡히는 상황 방지용
            if (droppedObjects.Contains(attachedObject))
                continue;


            DurabilityController durability =
                attachedObject.GetComponent<DurabilityController>();

            if (durability == null)
                durability = attachedObject.GetComponentInChildren<DurabilityController>(true);

            if (durability == null)
                continue;

            if (durability.DurabilityType != DurabilityType.Part)
                continue;

            if (durability.PartsData == null)
                continue;

            // 파괴된 파츠는 드랍하지 않음
            if (durability.IsDestroyed || durability.CurrentDurability <= 0f)
                continue;

            CreateDropPart(
                durability.PartsData,
                spawnPosition,
                durability.CurrentDurability
            );

            droppedObjects.Add(attachedObject);
            droppedPartCount++;

            Debug.Log(
                $"[DropRuntime] 실제 장착 파츠 드랍: " +
                $"{durability.PartsData.PartsName} / " +
                $"{durability.CurrentDurability:0} / {durability.MaxDurability:0}"
            );
        }

        // 실제 장착 파츠를 하나도 찾지 못하면 기존 EnemyData.DropParts 사용
        if (droppedPartCount <= 0 && enemyData != null)
        {
            Debug.LogWarning("[DropRuntime] 실제 장착 파츠를 찾지 못해 EnemyData.DropParts를 fallback으로 사용합니다.");
            DropParts(enemyData, spawnPosition, null);
            return;
        }

        // 재화 드랍은 기존유지
        DropCurrencyItems(enemyData, spawnPosition);
    }
}

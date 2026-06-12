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


        if (_enemyData.DropItems != null && _enemyData.DropItems.Count > 0)
        {
            foreach (DropItem dropData in _enemyData.DropItems)
            {
                if (dropData == null || dropData.ItemPrefab == null) continue;

                float currencyRoll = Random.Range(0f, 100f);
                if (currencyRoll <= dropData.DropChance)
                {
                    CreateDropCurrency(dropData, spawnPosition);
                }
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
    private void CreateDropParts(List<PartsData> _dropList, Vector3 spawnPosition)
    {
        Debug.Log($"드랍 파츠 아이템 생성");
        for (int i = 0; i < _dropList.Count; i++)
        {
            GameObject dropPart = Object.Instantiate(_dropList[i].PartsPrefab);
            dropPart.AddComponent<BoxCollider>();
            BoxCollider collider = dropPart.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.center = new Vector3(-0.3f, 0, 0.6f);
                collider.size = new Vector3(1f, 1f, 3f);
            }
            dropPart.transform.localScale = Vector3.one;

            Vector3 randomOffset = Random.insideUnitSphere * 5f;
            randomOffset.y = 0f;
            Vector3 scatterPosition = spawnPosition + randomOffset;

            Vector3 finalGroundPos = GetSpawnDropPos(scatterPosition);

            dropPart.transform.position = finalGroundPos + new Vector3(0f, 0.6f, 0f);
        }
    }
}

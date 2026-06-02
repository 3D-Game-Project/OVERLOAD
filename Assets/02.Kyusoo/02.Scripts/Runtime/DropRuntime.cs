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
    public void DropParts(EnemyData _enemyData, Vector3 spawnPosition)
    {
        Debug.Log($"사망 후 드랍 파츠 확률 함수 실행");
        if (_enemyData == null || _enemyData.DropParts == null)
        {
            Debug.LogError("EnemyData에 DropParts가 존재하지않음.");
            return;
        }

        List<PartsData> _dropList = new List<PartsData>();
        _maxDropCount = Random.Range(2, 3); 

        for (int i = 0; i < _enemyData.DropParts.Count; i++)
        {
            if (_dropList.Count == _maxDropCount) break;

            _dropList.Add(_enemyData.DropParts[i]);

            //int randomIndex = Random.Range(1, 11);

            //if(randomIndex >= 9)
            //{
            //    _dropList.Add(_enemyData.DropParts[i]);
            //}
        }

        if(_dropList.Count > 0)
        {
            CreateDropParts(_dropList, spawnPosition);
        }
    }

    private void CreateDropParts(List<PartsData> _dropList, Vector3 spawnPosition)
    {
        Debug.Log($"드랍 파츠 아이템 생성");
        for(int i = 0; i < _dropList.Count; i++)
        {
            GameObject dropPart = Object.Instantiate(_dropList[i].PartsPrefab);
            dropPart.transform.localScale = Vector3.one * 0.5f;

            Vector3 randomOffset = Random.insideUnitSphere * 1f;
            randomOffset.y = 0f;
            dropPart.transform.position = spawnPosition + randomOffset;
        }
    }

}

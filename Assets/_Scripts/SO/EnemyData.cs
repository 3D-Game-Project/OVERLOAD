using UnityEngine;

[CreateAssetMenu(fileName = "New_Enemy_Data", menuName = "Data/Enemy")]
public class EnemyData : UnitData
{
    [SerializeField] private GameObject _enemyPrefab;
    // [SerializeField] private EnemyType _enemyType;
    // [SerializeField] private List<PartsData> _dropParts;
    // [SerializeField] private List<DropItem> _dropItems;
}

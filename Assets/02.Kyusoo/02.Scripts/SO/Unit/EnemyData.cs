using UnityEngine;

[CreateAssetMenu(fileName = "New_Enemy_Data", menuName = "Data/Enemy")]
public class EnemyData : UnitData
{
    [SerializeField] private GameObject _enemyPrefab;
     [SerializeField] private EnemyType _enemyType;
     //[SerializeField] private List<PartsData> _dropParts;
     //[SerializeField] private List<DropItem> _dropItems;

    [Header("Movement")]
    public float PatrolSpeed = 1.8f;
    public float ChaseSpeed = 3.8f;
    public float RotationSpeed = 720f;

    [Header("Detection")]
    public float DetectRadius = 10f;
    public float ViewAngle = 120f;
    public LayerMask TargetLayer;
    public LayerMask ObstacleLayer;

    //[Header("Combat")]
    //public float AttackRange = 2.1f;
    //public float AttackCooldown = 1.2f;
    //public float AttackStopDistanceRate = 0.85f;

    [Header("Patrol")]
    public float WaitAtPatrolPoint = 1.0f;
}

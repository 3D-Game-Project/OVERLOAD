using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New_Enemy_Data", menuName = "Data/Enemy")]
public class EnemyData : UnitData
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private EnemyType _enemyType;
    [SerializeField] private List<PartsData> _dropParts;
    [SerializeField] private List<DropItem> _dropItems;

    public List<PartsData> DropParts => _dropParts;
    public List<DropItem> DropItems => _dropItems;

    [Header("Movement")]
    public float PatrolSpeed = 4f;
    public float ChaseSpeed = 5f;
    public float RotationSpeed = 720f;

    [Header("Detection")]
    public float DetectRadius = 20f;
    public float ViewAngle = 120f;
    public LayerMask TargetLayer;
    public LayerMask ObstacleLayer;

    [Header("Patrol")]
    public float WaitAtPatrolPoint = 1.0f;
}
using UnityEngine;

[CreateAssetMenu(fileName = "New_Repair_Kit", menuName = "Data/Item/Repair Kit")]
public class RepairKitData : ItemData
{
    [Header("Repair")]
    [SerializeField] private float _repairAmount = 100f;

    public float RepairAmount => _repairAmount;
}
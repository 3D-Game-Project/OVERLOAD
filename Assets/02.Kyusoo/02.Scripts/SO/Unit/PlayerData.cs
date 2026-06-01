using UnityEngine;

[CreateAssetMenu(fileName = "New_Player_Data", menuName = "Data/Player")]
public class PlayerData : UnitData
{
    [SerializeField] private GameObject _playerPrefab;
}
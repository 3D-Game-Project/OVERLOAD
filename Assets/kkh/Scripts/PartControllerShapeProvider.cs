using UnityEngine;

public class PartControllerShapeProvider : MonoBehaviour
{
    [Header("Character Controller Shape")]
    [SerializeField] private CharacterControllerShape _shape;

    public CharacterControllerShape Shape => _shape;
}
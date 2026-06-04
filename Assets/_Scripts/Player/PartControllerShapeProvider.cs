using UnityEngine;

// 임시
// 다리모듈에 따라 characterController의 크기가 달라져야함
// 크기에 맞게 다리 모듈에 대한 크기 확인용
// shapeController에서 해당 값을 가져가서 적용
public class PartControllerShapeProvider : MonoBehaviour
{
    [Header("Character Controller Shape")]
    [SerializeField] private CharacterControllerShape _shape;

    public CharacterControllerShape Shape => _shape;
}
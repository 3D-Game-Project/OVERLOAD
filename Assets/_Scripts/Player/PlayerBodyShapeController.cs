using UnityEngine;

[System.Serializable]
public struct CharacterControllerShape
{
    public float height;
    public float radius;
    public Vector3 center;
    public float stepOffset;
}

public class PlayerBodyShapeController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    [Header("Default Core Shape")]
    [SerializeField] private CharacterControllerShape _coreOnlyShape;

    [Header("Test")]
    [SerializeField] private PartControllerShapeProvider _testShapeProvider;
    [SerializeField] private bool _applyTestShapeOnStart = true;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (_applyTestShapeOnStart && _testShapeProvider != null)
        {
            ApplyPartShape(_testShapeProvider);
        }
        else
        {
            ApplyCoreOnlyShape();
        }
    }

    public void ApplyCoreOnlyShape()
    {
        ApplyShape(_coreOnlyShape);
    }

    public void ApplyPartShape(PartControllerShapeProvider shapeProvider)
    {
        if (shapeProvider == null)
        {
            ApplyCoreOnlyShape();
            return;
        }

        ApplyShape(shapeProvider.Shape);
    }

    private void ApplyShape(CharacterControllerShape shape)
    {
        _characterController.height = shape.height;
        _characterController.radius = shape.radius;
        _characterController.center = shape.center;
        _characterController.stepOffset = shape.stepOffset;
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    public bool IsGrounded => _characterController.isGrounded;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ApplyGravity();

        Vector3 finalVelocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
        _characterController.Move(finalVelocity * Time.deltaTime);
    }

    public void SetHorizontalVelocity(Vector3 velocity)
    {
        velocity.y = 0f;
        _horizontalVelocity = velocity;
    }

    public void AddHorizontalVelocity(Vector3 velocity)
    {
        velocity.y = 0f;
        _horizontalVelocity += velocity;
    }

    public void SetVerticalVelocity(float velocity)
    {
        _verticalVelocity = velocity;
    }

    public void AddVerticalVelocity(float velocity)
    {
        _verticalVelocity += velocity;
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity <0f)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }
}

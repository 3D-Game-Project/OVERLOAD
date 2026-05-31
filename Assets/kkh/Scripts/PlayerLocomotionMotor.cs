using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotionMotor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;
    [SerializeField] private float _maxFallSpeed = -25f;

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    private bool _ignoreGroundSnapThisFrame;

    public bool IsGrounded => _characterController.isGrounded;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        ApplyGravity();

        Vector3 finalVelocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
        _characterController.Move(finalVelocity * Time.deltaTime);

        _ignoreGroundSnapThisFrame = false;
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

    public void IgnoreGroundSnapThisFrame()
    {
        _ignoreGroundSnapThisFrame = true;
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f && !_ignoreGroundSnapThisFrame)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;

        if (_verticalVelocity < _maxFallSpeed)
            _verticalVelocity = _maxFallSpeed;
    }
}
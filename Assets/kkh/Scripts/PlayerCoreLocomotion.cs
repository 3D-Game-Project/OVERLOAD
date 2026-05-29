using UnityEngine;

public class PlayerCoreLocomotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLocomotionMotor _motor;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _visualRoot;

    [Header("Core Movement Settings")]
    [SerializeField] private float _maxMoveSpeed = 2.5f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _decceleration = 8f;
    [SerializeField] private float _rotationSpeed = 8f;

    [Header("Thruster Settings")]
    [SerializeField] private float _hoverHeight = 2f;
    [SerializeField] private float _hoverForce = 20f;
    [SerializeField] private float _maxRiseSpeed = 5f;
    [SerializeField] private float _groundCheckDistance = 3f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Visual")]
    [SerializeField] private float _tiltAmount = 8f;
    [SerializeField] private float _tiltSmooth = 8f;

    private float _currentOutput;


}

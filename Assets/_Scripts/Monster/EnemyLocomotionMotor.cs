using UnityEngine;
using UnityEngine.AI;
public class EnemyLocomotionMotor : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private NavMeshAgent _agent;

    [Header("Gravity (중력 설정)")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundedForce = -2f;
    [SerializeField] private float _maxFallSpeed = -25f;

    private float _verticalVelocity;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();

        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (_agent != null)
        {
            _agent.updatePosition = false;
            _agent.updateRotation = true;
        }
    }

    private void Update()
    {
        if (_characterController == null || !_characterController.enabled || _agent == null) return;
        if (!_agent.isOnNavMesh) return;

        ApplyGravity();

        Vector3 agentVelocity = _agent.desiredVelocity;

        Vector3 finalVelocity = new Vector3(agentVelocity.x, _verticalVelocity, agentVelocity.z);

        _characterController.Move(finalVelocity * Time.deltaTime);

        _agent.nextPosition = transform.position;
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundedForce;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;

        if (_verticalVelocity < _maxFallSpeed)
        {
            _verticalVelocity = _maxFallSpeed;
        }
    }
}
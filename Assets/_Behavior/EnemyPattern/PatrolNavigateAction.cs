using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "[Self] patrols between [PatrolRadius] at [PatrolSpeed] speed", category: "Action", id: "29a7d7b31f380c4ddc4a250ae3b5441d")]
public partial class PatrolNavigateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [Header("정찰 설정")]
    [SerializeReference] public BlackboardVariable<float> PatrolRadius;
    [SerializeReference] public BlackboardVariable<float> PatrolSpeed;

    [Header("시간 및 가감속 설정 (인스펙터에서 수정 가능)")]
    public float MoveDuration = 2.5f; 
    public float WaitDuration = 1.0f; 
    public float Acceleration = 10f;  
    public float Deceleration = 15f;  

    private NavMeshAgent _agent;
    private float _arrivalDistance = 1.0f;

    private enum PatrolState { Moving, Stopping, Waiting }
    private PatrolState _currentState;

    private float _stateTimer;    
    private float _currentSpeed;  

    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null) return Status.Failure;

        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.updateRotation = true;

            _currentState = PatrolState.Moving;
            _stateTimer = 0f;
            _currentSpeed = 0f;
            _agent.speed = 0f; 

            SetNewRandomDestination();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null || !_agent.gameObject.activeInHierarchy || !_agent.enabled || !_agent.isOnNavMesh)
        {
            return Status.Running;
        }

        float targetSpeed = PatrolSpeed != null ? PatrolSpeed.Value : 5f;

        switch (_currentState)
        {
            case PatrolState.Moving:
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, Acceleration * Time.deltaTime);
                _agent.speed = _currentSpeed;

                _stateTimer += Time.deltaTime;

                if (_stateTimer >= MoveDuration || (!_agent.pathPending && _agent.remainingDistance <= _arrivalDistance))
                {
                    _currentState = PatrolState.Stopping;
                }
                break;

            case PatrolState.Stopping:
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, Deceleration * Time.deltaTime);
                _agent.speed = _currentSpeed;

                if (_currentSpeed <= 0.01f)
                {
                    _currentSpeed = 0f;
                    _agent.speed = 0f;
                    _agent.ResetPath(); 

                    _currentState = PatrolState.Waiting;
                    _stateTimer = 0f; 
                }
                break;

            case PatrolState.Waiting:
                _stateTimer += Time.deltaTime;

                if (_stateTimer >= WaitDuration)
                {
                    _currentState = PatrolState.Moving;
                    _stateTimer = 0f; 

                    SetNewRandomDestination();
                }
                break;
        }

        return Status.Running;
    }

    private void SetNewRandomDestination()
    {
        float radius = PatrolRadius != null ? PatrolRadius.Value : 20f;

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += Self.Value.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }
}
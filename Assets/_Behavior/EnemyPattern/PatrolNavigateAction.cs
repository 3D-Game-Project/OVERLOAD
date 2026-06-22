using System;
using System.Collections.Generic;
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

    private NavMeshAgent _agent;

    private float _arrivalDistance = 1.0f;

    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null) return Status.Failure;

        if (PatrolSpeed != null) _agent.speed = PatrolSpeed.Value;

        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.ResetPath();

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

        if (!_agent.pathPending && _agent.remainingDistance <= _arrivalDistance)
        {
            SetNewRandomDestination();
        }

        return Status.Running;
    }

    private void SetNewRandomDestination()
    {
        float radius = PatrolRadius != null ? PatrolRadius.Value : 15f;

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += Self.Value.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }
}


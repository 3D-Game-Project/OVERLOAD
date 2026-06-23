using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack Target", story: "[Self] attacks [Target]", category: "Action", id: "61c0ad711652e40298cdfcc98c6b6e15")]
public partial class AttackTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<string> AttackTrigger;

    private NavMeshAgent _agent;
    private FireManager[] _fireManagers;
    private WeaponGimbalController[] _gimbals;

    private DurabilityController _coreDurability;

    private float _lastAnimTime;

    private Transform _currentTargetPart;
    private float _targetChangeTimer = 0f;
    private float _targetChangeInterval = 1f;

    // 공격이 시작되면 agent의 움직임을 멈추고 FireManager가 연결된 모든 파츠를 찾기
    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;

        if(Target?.Value == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");

            if (foundPlayer != null)
            {
                Target.Value = foundPlayer;
            }
            else
            {
                return Status.Failure;
            }
        }

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            _agent.ResetPath();
        }

        _coreDurability = Self.Value.GetComponent<DurabilityController>();

        _fireManagers = Self.Value.GetComponentsInChildren<FireManager>(false);
        _gimbals = Self.Value.GetComponentsInChildren<WeaponGimbalController>(false);

        _targetChangeTimer = 0f;

        return Status.Running;
    }

    // 플레이어의 위치를 찾아서 플레이어의 위치로 회전하기
    // 이후 좌우, 상하의 각도 구하기
    // 플레이어의 높이가 몬스터의 Gimbal과 계산할때 60이상일경우 사격중지
    // 그외(좌우 15도 상하 60이내)일 경우 사격실행 
    protected override Status OnUpdate()
    {
        if (Self?.Value == null || Target?.Value == null) return Status.Failure;

        if (_coreDurability != null && (_coreDurability.IsDestroyed || _coreDurability.CurrentDurability <= 0f))
        {
            return Status.Failure;
        }

        if (_fireManagers == null || _fireManagers.Length == 0)
        {
            _fireManagers = Self.Value.GetComponentsInChildren<FireManager>(false);
            _gimbals = Self.Value.GetComponentsInChildren<WeaponGimbalController>(false);
            if (_fireManagers.Length == 0) return Status.Failure;
        }

        UpdateTargetPart();

        Vector3 targetPos = _currentTargetPart != null ? _currentTargetPart.position : Target.Value.transform.position;
        Vector3 myPos = Self.Value.transform.position;
        Vector3 dirToTarget = targetPos - myPos;


        Vector3 dirXZ = new Vector3(dirToTarget.x, 0f, dirToTarget.z);

        if (dirXZ.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirXZ);
            Self.Value.transform.rotation = Quaternion.Slerp(Self.Value.transform.rotation, lookRot, Time.deltaTime * 5f);
        }

        
        float horizontalAngle = Vector3.Angle(Self.Value.transform.forward, dirXZ);
        float verticalAngle = Vector3.Angle(dirXZ, dirToTarget);

        float maxAllowedPitch = 60f;
        if (_gimbals != null && _gimbals.Length > 0 && _gimbals[0] != null)
        {
            maxAllowedPitch = _gimbals[0].MaxVerticalAngle;
        }

        bool isHorizontalAligned = horizontalAngle <= 15f;

        // 플레이어의 높이가 Gimbal의 최대 Vertical(60)을 벗어났을 때, 쏘지않고 대기할것인지 확인
        bool isVerticalLimit = true;

        bool isVerticalAligned = isVerticalLimit ? (verticalAngle <= maxAllowedPitch) : true;

        if (isHorizontalAligned && isVerticalAligned)
        {
            FireAndAnimate(targetPos);
        }
        
        return Status.Running;
    }

    private void UpdateTargetPart()
    {
        _targetChangeTimer -= Time.deltaTime;

        
        if (_currentTargetPart == null || !_currentTargetPart.gameObject.activeInHierarchy || _targetChangeTimer <= 0f)
        {
            
            DurabilityController[] allParts = Target.Value.GetComponentsInChildren<DurabilityController>(false);

            List<Transform> aliveParts = new List<Transform>();

            
            foreach (DurabilityController part in allParts)
            {
                if (!part.IsDestroyed && part.CurrentDurability > 0f)
                {
                    aliveParts.Add(part.transform);
                }
            }

            if (aliveParts.Count > 0)
            {
                int randomIdx = UnityEngine.Random.Range(0, aliveParts.Count);
                _currentTargetPart = aliveParts[randomIdx];
            }
            else
            {
                _currentTargetPart = Target.Value.transform;
            }
            Debug.Log($"공격중인 파츠 확인하기: {_currentTargetPart.gameObject}");
            _targetChangeTimer = _targetChangeInterval;
        }
    }

    private void FireAndAnimate(Vector3 targetPoint)
    {
        bool isWeaponReady = false;
        float cooldownForAnim = 0.5f; // 만약을 대비한 기본 쿨타임

        foreach (FireManager firemanager in _fireManagers)
        {
            if (firemanager == null || !firemanager.enabled || firemanager.WeaponRuntime == null) continue;


            firemanager.TryFire(targetPoint, _currentTargetPart);

            isWeaponReady = true;
            if (firemanager.AttackPartsData != null)
            {
                cooldownForAnim = firemanager.AttackPartsData.FireCooldown;
            }
        }

        if (isWeaponReady && Time.time >= _lastAnimTime + cooldownForAnim)
        {
            _lastAnimTime = Time.time;
            TriggerAnimation();
        }
    }

    private void TriggerAnimation()
    {
        if (Animator?.Value != null && !string.IsNullOrEmpty(AttackTrigger?.Value))
        {
            Animator.Value.SetTrigger(AttackTrigger.Value);
        }
    }
}


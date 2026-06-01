using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Sync Config From Self", story: "[Self] updates config", category: "Action", id: "fcaa6c4ac53db183fc8867d4638f03ee")]
public partial class SyncConfigFromSelfAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<float> AttackRange;

    [SerializeReference] public BlackboardVariable<float> DetectRadius;

    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;

    [SerializeReference] public BlackboardVariable<float> PatrolSpeed;

    [SerializeReference] public BlackboardVariable<float> AttackCooldown;

    protected override Status OnUpdate()
    {
        if (Self?.Value == null) return Status.Failure;

        EnemyBehaviorBridge bridge = Self.Value.GetComponent<EnemyBehaviorBridge>();
        if (bridge == null || !bridge.HasConfig) return Status.Failure;

        EnemyData config = bridge.Config;

        // 가져온 고정 데이터(config)의 값들을 BlackboardVariable의 '.Value'에 덮어씌웁니다.
        //Behavior Tree 내부의 다른 노드들이 이 갱신된 값들을 꺼내어 쓸 수 있음.
        AttackRange.Value = bridge.GetMinAttackRange();
        DetectRadius.Value = config.DetectRadius;
        ChaseSpeed.Value = config.ChaseSpeed;
        PatrolSpeed.Value = config.PatrolSpeed;
        AttackCooldown.Value = 0f;

        return Status.Success;
    }
}


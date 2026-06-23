using System;
using Unity.AppUI.UI;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check Distance", story: "Is [Target] within [DetectRadius] from [Self]", category: "Conditions", id: "3439c9d9c4c2422680818caa276096be")]
public partial class CheckDistanceCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> DetectRadius;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    // 높낮이(Y축)고려를 제외한 상태에서 X,Z에 대한 계산 후, 탐지범위내로 들어오면 True반환
    public override bool IsTrue()
    {
        if (Self == null || Self.Value == null)
        {
            return false;
        }
        if (Target == null || Target.Value == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");

            if (foundPlayer != null)
            {
                Target.Value = foundPlayer;
            }
            else
            {
                return false;
            }
        }

        float distance = Vector3.Distance(Self.Value.transform.position, Target.Value.transform.position);
        float radius = DetectRadius != null ? DetectRadius.Value : 0f;

        bool isWithinRadius = distance <= radius;


        return isWithinRadius;
    }
}

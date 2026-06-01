using UnityEngine;

// 땅 감지 센서
// sphereCast를 통해 감지
public class GroundSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _originOverride;     // 없으면 characterController기준으로 바닥 감지 (다리가 여러개인 모듈같은 경우에 필요할 수 있음)
    [SerializeField] private Transform _ignoreRoot;         // 자기 자신 감지 안하도록 추가

    // spherecast 세팅용 변수
    [Header("Cast Settings")]
    [SerializeField] private float _sphereRadius = 0.25f;
    [SerializeField] private float _castDistance = 10f;
    [SerializeField] private float _skinOffset = 0.05f;
    [SerializeField] private LayerMask _groundLayer = ~0;   // 우선은 everything으로 설정

    // 바닥 감지 -> groundinfo에 담기 -> 찾았으면 true 반환
    public bool TryGetGround(out GroundInfo groundInfo)
    {
        // 감지 시작점 찾기 (originoverride가 있으면 거기 기준)
        Vector3 origin = GetCastOrigin();

        // 한 지점만 감지하는 raycast가 아닌 spherecast이용
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            _sphereRadius,
            Vector3.down,
            _castDistance + _skinOffset,
            _groundLayer,
            QueryTriggerInteraction.Ignore
        );

        // 땅 정보 초기화 시켜두기
        groundInfo = new GroundInfo
        {
            HasGround = false,
            Distance = Mathf.Infinity,
            Point = Vector3.zero,
            Normal = Vector3.up
        };

        float closestDistance = Mathf.Infinity;

        // spherecast가 닿은 것 중에서 가장 가까운 거리 찾기
        // for문을 통해 가장 가까운 hit을 땅 정보에 업데이트
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (_ignoreRoot != null && hit.collider.transform.IsChildOf(_ignoreRoot))
                continue;

            float correctedDistance = Mathf.Max(0f, hit.distance - _skinOffset);

            if (correctedDistance < closestDistance)
            {
                closestDistance = correctedDistance;

                groundInfo = new GroundInfo
                {
                    HasGround = true,
                    Distance = correctedDistance,
                    Point = hit.point,
                    Normal = hit.normal
                };
            }
        }

        return groundInfo.HasGround;
    }

    // SphereCast 시작점 설정용
    private Vector3 GetCastOrigin()
    {
        // originoverride가 있으면 거기를 기준점으로
        // 살짝 올린 상태로 시작해서 바닥과 겹치지않게 미리 조정
        if (_originOverride != null)
            return _originOverride.position + Vector3.up * (_sphereRadius + _skinOffset);

        // override가 없는 경우 (그리고 charactercontroller가 있는 경우)
        if (_characterController != null)
        {
            // character controller의 가운데를 우선 잡기
            Vector3 center = transform.TransformPoint(_characterController.center);

            // 캡슐 모양이기에 오차를 빼줌
            float bottomOffset =
                (_characterController.height * 0.5f) -
                _characterController.radius;

            Vector3 bottom = center - Vector3.up * bottomOffset;

            return bottom + Vector3.up * (_sphereRadius + _skinOffset);
        }

        // 둘다 없는 경우 그냥 이 오브젝트를 기준으로 사용
        return transform.position + Vector3.up * (_sphereRadius + _skinOffset);
    }
}
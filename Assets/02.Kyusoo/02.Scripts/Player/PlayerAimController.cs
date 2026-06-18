using UnityEngine;
using UnityEngine.UI;

public class PlayerAimController : MonoBehaviour, IAimProvider
{
    [Header("조준 설정")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _maxAimDistance = 100f;
    [SerializeField] private LayerMask _aimLayerMask;

    [Header("근접 조준 보정관련 설정")]
    [SerializeField] private float _closeFadeDistance = 5f;
    [SerializeField] private float _virtualDistance = 25f;

    [Header("UI 연동")]
    [SerializeField] private Image _crosshairImage;


    public Vector3 CurrentAimPoint { get; private set; }

    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateAimPoint();
    }

    /// <summary>
    /// 화면 중앙(크로스헤어) 기준의 물리 레이캐스트를 발사하여 실제 인게임 무기들이 조준해야 할 좌표(CurrentAimPoint)를 실시간으로 계산
    /// 1. 지형 및 적 충돌 성공 시: 충돌 거리를 에임 거리에 할당하되, 플레이어와 너무 가까운 충돌체인 경우 총구가 급격하게 꺾여 왜곡되는 것을 방지하기 위해 가상 거리(_virtualDistance)로 부드럽게 영점 설정.
    /// 2. 충돌 실패(허공 사격) 시: 크로스헤어와 무기 총구 사이의 좌우 탄착군 영점이 과도하게 벌어지는 영점 이탈 현상을 차단하고 크로스헤어로 중앙 사격을 유지하기 위해, 가상 영점 20m로 구성.
    /// </summary>
    private void UpdateAimPoint()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);

        float finalRayDistance = _maxAimDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, _maxAimDistance, ~_aimLayerMask))
        {
            float distanceToPlayer = Vector3.Distance(transform.position, hit.point);

            if (distanceToPlayer < _closeFadeDistance)
            {
                float distanceRatio = Mathf.Clamp01(distanceToPlayer / _closeFadeDistance);

                finalRayDistance = Mathf.Lerp(_virtualDistance, hit.distance, distanceRatio);
            }
            else
            {
                finalRayDistance = hit.distance;
            }
        }
        else
        {
            finalRayDistance = 20f;
        }

        CurrentAimPoint = ray.origin + ray.direction * finalRayDistance;
    }

    public Vector3 GetAimPoint()
    {
        return CurrentAimPoint;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (_mainCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_mainCamera.transform.position, CurrentAimPoint);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(CurrentAimPoint, 0.3f);
    }
}
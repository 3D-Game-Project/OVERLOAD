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
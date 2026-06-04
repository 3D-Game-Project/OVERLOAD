using UnityEngine;
using UnityEngine.UI;

public class PlayerAimController : MonoBehaviour, IAimProvider
{
    [Header("조준 설정")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _maxAimDistance = 100f;
    [SerializeField] private LayerMask _aimLayerMask;

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

        if (Physics.Raycast(ray, out RaycastHit hit, _maxAimDistance, ~_aimLayerMask))
        {
            CurrentAimPoint = hit.point;
        }
        else
        {
            CurrentAimPoint = ray.origin + ray.direction * 25f;
        }
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
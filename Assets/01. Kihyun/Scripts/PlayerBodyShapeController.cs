using UnityEngine;

public class PlayerBodyShapeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _visualRoot;

    [Header("Shape Settings")]
    [SerializeField] private float _heightPadding = 0.05f;
    [SerializeField] private float _minHeight = 0.5f;

    [Header("Radius Settings")]
    [SerializeField] private bool _autoRadius = false;
    [SerializeField] private float _fixedRadius = 0.45f;
    [SerializeField] private float _radiusPadding = 0.05f;
    [SerializeField] private float _minRadius = 0.2f;
    [SerializeField] private float _maxRadius = 1.0f;

    [Header("Center Settings")]
    [SerializeField] private bool _useVisualCenterXZ = false;

    [Header("Ground Snap")]
    [SerializeField] private bool _snapToGroundAfterApply = true;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundOffset = 0.03f;
    [SerializeField] private float _snapRayStartHeight = 3f;
    [SerializeField] private float _snapRayDistance = 10f;

    private void Awake()
    {
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();
    }

    public void RebuildShapeFromVisuals()
    {
        if (_characterController == null)
            return;

        if (_visualRoot == null)
            return;

        if (!TryCalculateLocalVisualBounds(out Bounds localBounds))
            return;

        ApplyCharacterControllerShape(localBounds);

        if (_snapToGroundAfterApply)
        {
            SnapControllerBottomToGround();
        }
    }

    private void ApplyCharacterControllerShape(Bounds localBounds)
    {
        float height = Mathf.Max(
            _minHeight,
            localBounds.size.y + _heightPadding
        );

        float radius = _fixedRadius;

        if (_autoRadius)
        {
            float visualRadius =
                Mathf.Max(localBounds.size.x, localBounds.size.z) * 0.5f;

            radius = visualRadius + _radiusPadding;
        }

        radius = Mathf.Clamp(radius, _minRadius, _maxRadius);
        radius = Mathf.Min(radius, height * 0.5f - 0.01f);

        Vector3 center = localBounds.center;

        if (!_useVisualCenterXZ)
        {
            center.x = 0f;
            center.z = 0f;
        }

        // 컨트롤러의 바닥을 실제 비주얼 바닥에 맞춤
        center.y = localBounds.min.y + height * 0.5f;

        _characterController.height = height;
        _characterController.radius = radius;
        _characterController.center = center;
    }

    private bool TryCalculateLocalVisualBounds(out Bounds localBounds)
    {
        Renderer[] renderers =
            _visualRoot.GetComponentsInChildren<Renderer>(false);

        bool hasBounds = false;
        localBounds = new Bounds();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!renderer.enabled)
                continue;

            Bounds worldBounds = renderer.bounds;

            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(min.x, min.y, min.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(min.x, min.y, max.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(min.x, max.y, min.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(min.x, max.y, max.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(max.x, min.y, min.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(max.x, min.y, max.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(max.x, max.y, min.z));

            EncapsulateWorldPoint(ref localBounds, ref hasBounds,
                new Vector3(max.x, max.y, max.z));
        }

        return hasBounds;
    }

    private void EncapsulateWorldPoint(
        ref Bounds bounds,
        ref bool hasBounds,
        Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

        if (!hasBounds)
        {
            bounds = new Bounds(localPoint, Vector3.zero);
            hasBounds = true;
            return;
        }

        bounds.Encapsulate(localPoint);
    }

    private void SnapControllerBottomToGround()
    {
        Vector3 rayOrigin =
            transform.position + Vector3.up * _snapRayStartHeight;

        if (!Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                _snapRayDistance,
                _groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        float controllerBottomY = GetControllerBottomWorldY();
        float targetBottomY = hit.point.y + _groundOffset;
        float deltaY = targetBottomY - controllerBottomY;

        if (Mathf.Abs(deltaY) < 0.001f)
            return;

        bool wasEnabled = _characterController.enabled;

        _characterController.enabled = false;
        transform.position += Vector3.up * deltaY;
        _characterController.enabled = wasEnabled;
    }

    private float GetControllerBottomWorldY()
    {
        return transform.position.y +
               _characterController.center.y -
               _characterController.height * 0.5f;
    }
}
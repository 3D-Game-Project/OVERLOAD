using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _originOverride;
    [SerializeField] private Transform _ignoreRoot;

    [Header("Cast Settings")]
    [SerializeField] private float _sphereRadius = 0.25f;
    [SerializeField] private float _castDistance = 10f;
    [SerializeField] private float _skinOffset = 0.05f;
    [SerializeField] private LayerMask _groundLayer = ~0;

    public bool TryGetGround(out GroundInfo groundInfo)
    {
        Vector3 origin = GetCastOrigin();

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            _sphereRadius,
            Vector3.down,
            _castDistance + _skinOffset,
            _groundLayer,
            QueryTriggerInteraction.Ignore
        );

        groundInfo = new GroundInfo
        {
            HasGround = false,
            Distance = Mathf.Infinity,
            Point = Vector3.zero,
            Normal = Vector3.up
        };

        float closestDistance = Mathf.Infinity;

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

    private Vector3 GetCastOrigin()
    {
        if (_originOverride != null)
            return _originOverride.position + Vector3.up * (_sphereRadius + _skinOffset);

        if (_characterController != null)
        {
            Vector3 center = transform.TransformPoint(_characterController.center);

            float bottomOffset =
                (_characterController.height * 0.5f) -
                _characterController.radius;

            Vector3 bottom = center - Vector3.up * bottomOffset;

            return bottom + Vector3.up * (_sphereRadius + _skinOffset);
        }

        return transform.position + Vector3.up * (_sphereRadius + _skinOffset);
    }
}
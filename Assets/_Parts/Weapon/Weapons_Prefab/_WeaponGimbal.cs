using UnityEngine;

public class WeaponGimbalController : MonoBehaviour
{
    [Header("Rotation Bounds (파츠별 고유 각도 제한)")]
    [Range(0f, 180f)][SerializeField] private float maxHorizontalAngle = 45f; 
    [Range(0f, 180f)][SerializeField] private float maxVerticalAngle = 60f;
    public float MaxVerticalAngle => maxVerticalAngle;

    [Header("Rotation Speed")]
    [SerializeField] private float rotationSpeed = 360f;

    [SerializeField] private float smoothDamping = 15f;
    private Quaternion currentTargetLocalRot = Quaternion.identity;

    private IAimProvider aimProvider;
    private Quaternion defaultLocalRotation;

    private void Awake()
    {
        defaultLocalRotation = transform.localRotation;
        currentTargetLocalRot = defaultLocalRotation;
    }

    private void Start()
    {
        aimProvider = GetComponentInParent<IAimProvider>();

        if (aimProvider == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 부모 기체에서 IAimProvider를 찾을 수 없어 자율 조준이 비활성화됩니다.");
        }
    }

    private void Update()
    {
        if (aimProvider == null)
        {
            ResetToDefault();
            return;
        }

        if (aimProvider is IAimStateProvider stateProvider && !stateProvider.IsAiming)
        {
            ResetToDefault();
            return;
        }


        Vector3 targetWorldPoint = aimProvider.GetAimPoint();

        ExecuteGimbalRotation(targetWorldPoint);
    }

    private void ExecuteGimbalRotation(Vector3 targetWorldPoint)
    {
        if (transform.parent == null) return;

        Vector3 localTargetPos = transform.parent.InverseTransformPoint(targetWorldPoint);

        Vector3 localWeaponPos = transform.parent.InverseTransformPoint(transform.position);
        Vector3 directionToTarget = localTargetPos - localWeaponPos;

        if (directionToTarget.z < 2.0f)
        {
            directionToTarget.z = 5.0f;
        }

        Quaternion targetLocalRot = Quaternion.LookRotation(directionToTarget, Vector3.up);
        Vector3 targetEuler = targetLocalRot.eulerAngles;

        float pitch = NormalizeAngle(targetEuler.x);

        pitch = Mathf.Clamp(pitch, -maxVerticalAngle, maxVerticalAngle);

        Quaternion clampedLocalRot = Quaternion.Euler(pitch, 0f, 0f);

        currentTargetLocalRot = Quaternion.Slerp(
            currentTargetLocalRot,
            clampedLocalRot,
            smoothDamping * Time.deltaTime
        );

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            currentTargetLocalRot,
            rotationSpeed * Time.deltaTime
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    public void ResetToDefault()
    {
        currentTargetLocalRot = Quaternion.Slerp(currentTargetLocalRot, defaultLocalRotation, smoothDamping * Time.deltaTime);
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            currentTargetLocalRot,
            rotationSpeed * Time.deltaTime
        );
    }
}
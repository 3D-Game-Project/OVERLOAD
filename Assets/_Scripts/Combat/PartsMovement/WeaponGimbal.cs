using UnityEngine;

public class WeaponGimbalController : MonoBehaviour
{
    [Header("Rotation Bounds (파츠별 고유 각도 제한)")]
    [Range(0f, 180f)][SerializeField] private float maxHorizontalAngle = 45f; 
    [Range(0f, 180f)][SerializeField] private float maxVerticalAngle = 60f;   

    [Header("Rotation Speed")]
    [SerializeField] private float rotationSpeed = 360f; 

    private IAimProvider aimProvider;
    private Quaternion defaultLocalRotation;

    private void Awake()
    {
        defaultLocalRotation = transform.localRotation;
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

        Vector3 targetWorldPoint = aimProvider.GetAimPoint();

        ExecuteGimbalRotation(targetWorldPoint);
    }

    private void ExecuteGimbalRotation(Vector3 targetWorldPoint)
    {
        if (transform.parent == null) return;

        Vector3 localTargetPos = transform.parent.InverseTransformPoint(targetWorldPoint);
        if (localTargetPos == Vector3.zero) return;

        Quaternion targetLocalRot = Quaternion.LookRotation(localTargetPos, Vector3.up);
        Vector3 targetEuler = targetLocalRot.eulerAngles;

        //float yaw = NormalizeAngle(targetEuler.y);
        float pitch = NormalizeAngle(targetEuler.x);

        //yaw = Mathf.Clamp(yaw, -maxHorizontalAngle, maxHorizontalAngle);
        pitch = Mathf.Clamp(pitch, -maxVerticalAngle, maxVerticalAngle);

        Quaternion clampedLocalRot = Quaternion.Euler(pitch, 0f, 0f);

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            clampedLocalRot,
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
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            defaultLocalRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
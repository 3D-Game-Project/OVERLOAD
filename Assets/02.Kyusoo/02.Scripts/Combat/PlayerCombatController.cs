using UnityEngine;

public class PlayerCombatController : MonoBehaviour,IAimProvider
{
    [SerializeField] private UnitData _unitData;
    [SerializeField ]private Vector3 _currentAimPoint;
    [SerializeField] private LayerMask targetAndObstacleLayer;

    public UnitRuntime UnitRuntime { get; private set; }

    private void Awake()
    {
        if (_unitData != null)
        {
            UnitRuntime = new UnitRuntime(_unitData);
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetAndObstacleLayer))
        {
            _currentAimPoint = hit.point;
        }
        else
        {
            _currentAimPoint = ray.origin + ray.direction * 100f;
        }
    }

    private void OnEnable()
    {
        if (UnitRuntime == null) return;
        UnitRuntime.OnHealthChanged += HealthChanged;
        UnitRuntime.OnDeath += Death;
    }

    private void OnDisable()
    {
        if (UnitRuntime == null) return;
        UnitRuntime.OnHealthChanged -= HealthChanged;
        UnitRuntime.OnDeath -= Death;
    }

    // 해당 부분 검토 필요 => 플레이어가 회복하였을 때 애니메이션을 추가할것인지?
    // playerController.RuntimeStatus.TakeDamage(10); 로 직접 호출하면 됨!

    //private void HealthChanged(int currentHp, int maxHp)
    //{
    //}

    public void TakeDamage(int damage)
    {
        UnitRuntime.TakeDamage(damage);
    }

    private void Heal(int healAmount)
    {
        UnitRuntime.Heal(healAmount);
    }

    private void HealthChanged(int currentHp, int maxHp)
    {
        Debug.Log(currentHp);
    }
    private void Death()
    {
        Invoke("PlayDeathAnimation", 2f);
    }

    private void PlayDeathAnimation()
    {
        // 사망 애니메이션 재생
        Destroy(gameObject);
    }

    public Vector3 GetAimPoint()
    {
        return _currentAimPoint;
    }
}

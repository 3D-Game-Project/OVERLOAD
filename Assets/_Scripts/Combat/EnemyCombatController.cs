using UnityEngine;

public class EnemyCombatController : MonoBehaviour
{
    [SerializeField] private UnitData _unitData;

    public UnitRuntime UnitRuntime { get; private set; }

    private void Awake()
    {
        if (_unitData != null)
        {
            UnitRuntime = new UnitRuntime(_unitData);
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

    public void TakeDamage(int damage)
    {
        if (UnitRuntime == null) return;
        Debug.Log("몬스터 피격중");
        UnitRuntime.TakeDamage(damage);

    }

    public void HealthChanged(int currentHp, int maxHp)
    {
        Debug.Log($"몬스터 피격 {currentHp}");
    }

    private void Death()
    {
        Invoke("PlayDeathAnimation", 2f);

        Destroy(gameObject, 2f);

    }

    private void PlayDeathAnimation()
    {
        // 사망 애니메이션 재생

        DropRuntime dropRuntime = new DropRuntime();
        Debug.Log($"사망 후 드랍처리");
        dropRuntime.DropParts(UnitRuntime.UnitData as EnemyData, transform.position, null);
    }
}

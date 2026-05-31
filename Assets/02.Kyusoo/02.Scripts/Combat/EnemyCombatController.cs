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
        UnitRuntime.OnDeath += Death;
    }

    private void OnDisable()
    {
        if (UnitRuntime == null) return;
        UnitRuntime.OnDeath -= Death;
    }

    public void TakeDamage(int damage)
    {
        if (UnitRuntime == null) return;
        UnitRuntime.TakeDamage(damage);
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
}

using System;


public class UnitRuntime
{
    public UnitData UnitData { get; private set; }
    public int CurrentHP { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    // 체력 변경 이벤트, 공격받았을 때 이벤트, 사망했을 때 이벤트
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    public UnitRuntime(UnitData unitData)
    {
        UnitData = unitData;
        CurrentHP = unitData.MaxHp;
        Attack = SetAttack();
        Defense = SetDefense();
    }

    // 장착된 파츠에 따라 공격력을 설정
    // 파츠 정보가 없을 경우 공격력 0 설정
    // 부착한 파츠중 무기가 있을 경우에만 공격력 설정, 무기가 없으면 공격력 0
    // 최초 시작시 기본 무기를 장착한 상태로 시작할 거기 때문에 공격력 0이 되는 경우 오류로 간주
    // 기본 무기의 공격력은 기본 방어력 5를 감안하여 10 정도로 설정
    public int SetAttack() // 받아올 정보 PartsData
    {
        // if(
        return 0;
    }

    // 장착된 파츠에 따라 방어력을 설정
    // 파츠 정보가 없을 경우 기본 방어력 5 설정
    // 부착한 파츠중 방어구가 있을 경우에만 방어력 설정, 방어구가 없으면 기본 방어력 5
    public int SetDefense() // 받아올 정보 PartsData
    {
        
        return 0;
    }

    // 체력 회복
    // CurrentHP가 MaxHp를 초과하지 않도록 설정
    // healAmount가 0 이하일 경우 회복하지 않도록 설정
    // 회복 후 체력 변경 이벤트 발생 => PlayerHealthBar에서 체력바 업데이트, Player에 회복에 따른 애니메이션 추가(미정)
    public void Heal(int healAmount)
    {
        if(healAmount <= 0) return;

        CurrentHP = Math.Min(UnitData.MaxHp, CurrentHP + healAmount);
        
        OnHealthChanged?.Invoke(CurrentHP, UnitData.MaxHp);
    }

    // 피격
    // damage가 0 이하일 경우 피격하지 않도록 설정
    // 피격 후 체력 변경 이벤트 발생 ==> 체력바 업데이트
    // 피격 후 OnHit 이벤트 발생 ==> 피격 애니메이션 추가
    public void TakeDamage(int damage)
    {
        if(damage <= 0) return;

        // 공격력 - 방어력 만큼의 피해 적용
        int finalDamage = Math.Max(0, damage - Defense);

        CurrentHP -= finalDamage;

        if(CurrentHP <= 0)
        {
            Die();
        }
        else
        {
            OnHealthChanged?.Invoke(CurrentHP, UnitData.MaxHp);
        }
    }

    // 사망, 이벤트 발행하여 사망 애니메이션 추가
    public void Die()
    {
        OnDeath?.Invoke();
    }
}

using UnityEngine;

// 코어 출력 전용 클래스
public class CorePowerController : MonoBehaviour
{
    [Header("Power")]
    [SerializeField] private float maxPower = 100f;
    [SerializeField] private float currentPower = 100f;
    [SerializeField] private float recoverPowerPerSecond = 15f;

    public float CurrentPower => currentPower;
    public float MaxPower => maxPower;
    public float PowerRatio => currentPower / maxPower;

    private void Update()
    {
        RecoverPower();
    }

    // 코어 출력 회복
    // 시간마다 일정정도 지속적 회복
    private void RecoverPower()
    {
        currentPower += recoverPowerPerSecond * Time.deltaTime;
        currentPower = Mathf.Min(currentPower, maxPower);
    }

    // public으로 외부에서 출력을 사용할 일이 있으면 가져가서 사용
    // 직관적으로 amount만큼 사용됨
    public bool TryUsePower(float amount)
    {
        if (amount <= 0f)
            return true;

        if (currentPower < amount)
            return false;

        currentPower -= amount;
        return true;
    }
}
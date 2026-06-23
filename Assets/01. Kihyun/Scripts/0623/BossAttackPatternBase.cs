using System.Collections;
using UnityEngine;

// 각 패턴은 이 클래스를 상속받아 구현
// 패턴에 필요한 파츠 유무 확인
// 패턴 딜레이 코루틴
public abstract class BossAttackPatternBase : MonoBehaviour
{
    public abstract BossAttackPatternType PatternType { get; }

    // 일반 이동 패턴과 동시에 실행할 수 있는지
    public abstract bool CanRunWhileMoving { get; }

    public bool IsRunning { get; private set; }

    protected bool IsCancellationRequested { get; private set; }

    public bool CanExecute(Transform target)
    {
        if (!isActiveAndEnabled)
            return false;

        if (IsRunning)
            return false;

        if (target == null)
            return false;

        return CheckRequirements(target);
    }

    internal IEnumerator Execute(Transform target)
    {
        if (!CanExecute(target))
            yield break;

        IsRunning = true;
        IsCancellationRequested = false;

        OnPatternStarted(target);

        yield return ExecutePattern(target);

        bool wasCancelled = IsCancellationRequested;

        OnPatternEnded(wasCancelled);

        IsRunning = false;
        IsCancellationRequested = false;
    }

    public virtual void Cancel()
    {
        if (!IsRunning)
            return;

        IsCancellationRequested = true;
    }

    protected void RequestCancel()
    {
        IsCancellationRequested = true;
    }

    protected bool ShouldStop(Transform target)
    {
        if (target == null)
        {
            IsCancellationRequested = true;
        }

        return IsCancellationRequested;
    }

    protected abstract bool CheckRequirements(Transform target);

    protected abstract IEnumerator ExecutePattern(Transform target);

    protected virtual void OnPatternStarted(Transform target)
    {
    }

    protected virtual void OnPatternEnded(bool wasCancelled)
    {
    }
}
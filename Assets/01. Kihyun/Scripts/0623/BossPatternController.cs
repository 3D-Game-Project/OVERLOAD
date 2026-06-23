using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatternController : MonoBehaviour
{
    [Header("Attack Patterns")]
    [SerializeField]
    private List<BossAttackPatternBase> _attackPatterns = new();

    private BossAttackPatternBase _currentAttackPattern;

    public bool IsAttackRunning =>
        _currentAttackPattern != null &&
        _currentAttackPattern.IsRunning;

    public BossAttackPatternType? CurrentAttackType =>
        _currentAttackPattern != null
            ? _currentAttackPattern.PatternType
            : null;

    private void Awake()
    {
        BossAttackPatternBase[] foundPatterns = GetComponentsInChildren<BossAttackPatternBase>(true);

        foreach (BossAttackPatternBase pattern in foundPatterns)
        {
            if (pattern == null)
                continue;

            if (!_attackPatterns.Contains(pattern))
            {
                _attackPatterns.Add(pattern);
            }
        }

        ValidatePatterns();
    }

    public bool CanExecuteAttack(
        BossAttackPatternType patternType,
        Transform target)
    {
        if (_currentAttackPattern != null)
            return false;

        BossAttackPatternBase pattern =
            FindPattern(patternType);

        return pattern != null &&
               pattern.CanExecute(target);
    }

    public bool CanRunWhileMoving(
        BossAttackPatternType patternType)
    {
        BossAttackPatternBase pattern =
            FindPattern(patternType);

        return pattern != null &&
               pattern.CanRunWhileMoving;
    }


    // yield return _patternController.ExecuteAttackPattern(패턴 타입, 플레이어)
    // 이런 식으로 보스 패턴 호출 가능
    public IEnumerator ExecuteAttackPattern(
        BossAttackPatternType patternType,
        Transform target)
    {
        if (_currentAttackPattern != null)
        {
            Debug.LogWarning(
                "이미 공격 패턴이 실행 중입니다."
            );

            yield break;
        }

        BossAttackPatternBase pattern =
            FindPattern(patternType);

        if (pattern == null)
        {
            Debug.LogWarning(
                $"{patternType} 패턴을 찾을 수 없습니다."
            );

            yield break;
        }

        if (!pattern.CanExecute(target))
            yield break;

        _currentAttackPattern = pattern;

        yield return pattern.Execute(target);

        if (_currentAttackPattern == pattern)
        {
            _currentAttackPattern = null;
        }
    }

    public void CancelCurrentAttack()
    {
        _currentAttackPattern?.Cancel();
    }

    private BossAttackPatternBase FindPattern(
        BossAttackPatternType patternType)
    {
        foreach (BossAttackPatternBase pattern
                 in _attackPatterns)
        {
            if (pattern == null)
                continue;

            if (pattern.PatternType == patternType)
                return pattern;
        }

        return null;
    }

    private void ValidatePatterns()
    {
        HashSet<BossAttackPatternType> types = new();

        foreach (BossAttackPatternBase pattern
                 in _attackPatterns)
        {
            if (pattern == null)
                continue;

            if (!types.Add(pattern.PatternType))
            {
                Debug.LogWarning(
                    $"중복 보스 패턴: {pattern.PatternType}"
                );
            }
        }
    }
}
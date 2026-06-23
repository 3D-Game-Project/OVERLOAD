using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossPatternDebugTester : MonoBehaviour
{
    [SerializeField] private BossPatternController _patternController;
    [SerializeField] private BossAimController _aimController;
    [SerializeField] private Transform _target;

    private Coroutine _testRoutine;

    private void Awake()
    {
        if (_patternController == null)
        {
            _patternController =
                GetComponent<BossPatternController>();
        }

        if (_aimController == null)
        {
            _aimController =
                GetComponent<BossAimController>();
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // 조준 테스트
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            _aimController?.StartAiming(_target);
        }

        if (Keyboard.current.yKey.wasReleasedThisFrame)
        {
            _aimController?.StopAiming();
        }

        // 충격파 테스트
        if (Keyboard.current.tKey.wasPressedThisFrame &&
            _testRoutine == null)
        {
            _testRoutine = StartCoroutine(
                TestShockwave()
            );
        }

        // 총기 공격 테스트
        if (Keyboard.current.gKey.wasPressedThisFrame &&
            _testRoutine == null)
        {
            _testRoutine = StartCoroutine(
                TestGunAttack()
            );
        }

        // 돌진 공격 테스트
        if (Keyboard.current.cKey.wasPressedThisFrame &&
              _testRoutine == null)
        {
            _testRoutine = StartCoroutine(
                TestCharge()
            );
        }

        if (Keyboard.current.mKey.wasPressedThisFrame &&
            _testRoutine == null)
        {
            _testRoutine = StartCoroutine(
                TestHomingMissile()
            );
        }
    }

    private bool TryFindTarget()
    {
        if (_target != null)
            return true;

        GameObject player =
            GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning(
                "돌진 테스트 대상 Player를 찾지 못했습니다."
            );

            return false;
        }

        _target = player.transform;
        return true;
    }

    private IEnumerator TestShockwave()
    {
        yield return _patternController.ExecuteAttackPattern(BossAttackPatternType.Shockwave, _target);

        _testRoutine = null;

        Debug.Log("충격파 패턴 종료");
    }

    private IEnumerator TestGunAttack()
    {
        yield return _patternController.ExecuteAttackPattern(BossAttackPatternType.Gun, _target);

        _testRoutine = null;

        Debug.Log("총기 공격 패턴 종료");
    }

    private IEnumerator TestCharge()
    {
        if (!TryFindTarget())
        {
            _testRoutine = null;
            yield break;
        }

        if (!_patternController.CanExecuteAttack(
                BossAttackPatternType.Charge,
                _target))
        {
            Debug.LogWarning(
                "돌진 실행 조건을 만족하지 못했습니다. " +
                "부스터 장착 및 작동 상태를 확인하세요."
            );

            _testRoutine = null;
            yield break;
        }

        yield return _patternController.ExecuteAttackPattern(
            BossAttackPatternType.Charge,
            _target
        );

        _testRoutine = null;

        Debug.Log("돌진 패턴 종료");
    }

    private IEnumerator TestHomingMissile()
    {
        if (!TryFindTarget())
        {
            _testRoutine = null;
            yield break;
        }

        if (!_patternController.CanExecuteAttack(
                BossAttackPatternType.HomingMissile,
                _target))
        {
            Debug.LogWarning(
                "유도 미사일 실행 조건을 만족하지 못했습니다."
            );

            _testRoutine = null;
            yield break;
        }

        yield return _patternController.ExecuteAttackPattern(
            BossAttackPatternType.HomingMissile,
            _target
        );

        _testRoutine = null;

        Debug.Log("유도 미사일 패턴 종료");
    }
}
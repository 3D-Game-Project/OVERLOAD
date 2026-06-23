using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Transform _player;

    private BossMovementController _movementController;
    private DurabilityController _durability;

    [Header("거리 기준 설정")]
    [SerializeField] private float _approachRange = 20f;
    [SerializeField] private float _closeRange = 10f;

    [Header("패턴별 최대 유지 시간")]
    [SerializeField] private float _approachMaxDuration = 3f;
    [SerializeField] private float _guidedAttackDuration = 2.5f;
    [SerializeField] private float _aoeAttackDuration = 2.5f;
    [SerializeField] private float _allOutAttackDuration = 3f;
    [SerializeField] private float _dashDuration = 2f;
    [SerializeField] private float _retreatDuration = 2f;
    [SerializeField] private float _shockwaveDuration = 2f;

    private void Awake()
    {
        _movementController = GetComponent<BossMovementController>();
        _durability = GetComponent<DurabilityController>();
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = GameObject.FindWithTag("Player")?.transform;
        }

        StartCoroutine(BossAILoop());
    }

    /// <summary>
    /// 보스에 판단처리
    /// 판단 1. 체력이 50% 이상인가? 이상이면 유도 공격, 장판 공격 보다 접근 우선
    /// 판단 2. 미만이면 유도 공격, 장판 공격이 접근 보다 우선
    /// </summary>
    private IEnumerator BossAILoop()
    {
        while (true)
        {
            if (_player == null)
            {
                _player = GameObject.FindWithTag("Player")?.transform;
                continue;
            }

            float distance = Vector3.Distance(transform.position, _player.position);

            if (distance > 100f)
            {
                if (_movementController != null) _movementController.StopMovement();

                yield return new WaitForSeconds(1f);
                continue; 
            }

            float hpRatio = 1f;
            if (_durability != null && _durability.MaxDurability > 0)
            {
                hpRatio = _durability.CurrentDurability / _durability.MaxDurability;
            }

            string patternResult = "";
            IEnumerator selectedPattern = null;

            // 거리가 20f보다 먼가? (접근 & 유도 공격 & 장판 공격)
            if (distance > _approachRange)
            {
                int randomValue = Random.Range(0, 100);

                // 조건 2. 체력이 50이상인가?(접근과 공격에 대한 우선순위 설정을 위해)
                if (hpRatio >= 0.5f)
                {
                    if (randomValue < 60)
                    {
                        patternResult = "접근 (HP 50% 이상, 확률 60%)";
                        selectedPattern = ApproachPattern();
                    }
                    else if (randomValue < 80)
                    {
                        patternResult = "유도 공격 (HP 50% 이상, 확률 20%)";
                        selectedPattern = GuidedAttackPattern();
                    }
                    else
                    {
                        patternResult = "장판 공격 (HP 50% 이상, 확률 20%)";
                        selectedPattern = AoeAttackPattern();
                    }
                }
                else // 체력이 50 미만
                {
                    if (randomValue < 30)
                    {
                        patternResult = "접근 (HP 50% 미만, 확률 30%)";
                        selectedPattern = ApproachPattern();
                    }
                    else if (randomValue < 65)
                    {
                        patternResult = "유도 공격 (HP 50% 미만, 확률 35%)";
                        selectedPattern = GuidedAttackPattern();
                    }
                    else
                    {
                        patternResult = "장판 공격 (HP 50% 미만, 확률 35%)";
                        selectedPattern = AoeAttackPattern();
                    }
                }
            }
            // 거리가 10 초과 20이하인가? (이동 공격 & 돌진)
            else if (distance <= _approachRange && distance > _closeRange)
            {
                if (Random.Range(0, 2) == 0)
                {
                    patternResult = "무빙 사격 (거리 10~20)";
                    selectedPattern = AllOutAttackPattern();
                }
                else
                {
                    patternResult = "돌진 (거리 10~20)";
                    selectedPattern = DashPattern();
                }
            }
            // 거리가 10 이하인가? (후진 이동 공격 & 충격파
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    patternResult = "후퇴 사격 (거리 10 이하)";
                    selectedPattern = RetreatPattern();
                }
                else
                {
                    patternResult = "충격파 (거리 10 이하)";
                    selectedPattern = ShockwavePattern();
                }
            }
            Debug.Log($"[BossAI] 실행 패턴: {patternResult} | 현재 거리: {distance:F1} | 남은 체력: {hpRatio * 100:F0}%");

            if (selectedPattern != null)
            {
                yield return StartCoroutine(selectedPattern);
            }

            float randomCooldown = Random.Range(2f, 4f);
            yield return new WaitForSeconds(randomCooldown);
        }
    }

    // 접근 패턴에 대한 처리
    // 목표지점 전달하여 움직임 처리
    // 3초의 시간동안 움직이지만 만약 빨리 도착했다면 해당 처리 즉시 종료 및 움직임 중지처리
    private IEnumerator ApproachPattern()
    {
        float timer = 0f;
        while (timer < _approachMaxDuration)
        {
            if (_player != null && _movementController != null)
            {
                _movementController.MoveToTarget(_player.position);
                _movementController.LookAtTarget(_player.position);
            }

            float currentDist = Vector3.Distance(transform.position, _player.position);
            if (currentDist < _approachRange) break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (_movementController != null) _movementController.StopMovement();
    }

    // 후퇴하며 사격 (20f까지 뒤로 빠지기)
    private IEnumerator RetreatPattern()
    {
        // 여기서 사격 처리(애니메이션, 발사로직 등)

        float timer = 0f;
        while (timer < _retreatDuration)
        {
            if (_player != null && _movementController != null)
            {
                Vector3 retreatDir = (transform.position - _player.position).normalized;
                retreatDir.y = 0f;
                _movementController.MoveToTarget(transform.position + retreatDir * 5f);
                _movementController.LookAtTarget(_player.position);
                // 사격 호출
            }

            float currentDist = Vector3.Distance(transform.position, _player.position);
            if (currentDist >= _approachRange) break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (_movementController != null) _movementController.StopMovement();
    }

    // 원거리 범위/유도 공격
    private IEnumerator GuidedAttackPattern()
    {
        float timer = 0f;
        while (timer < _guidedAttackDuration)
        {
            if (_player != null)
            {
                if (_movementController != null) _movementController.LookAtTarget(_player.position);
                // 유도공격 호출
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // 장판 공격
    private IEnumerator AoeAttackPattern()
    {
        float timer = 0f;
        while (timer < _aoeAttackDuration)
        {
            if (_player != null && _movementController != null)
            {
                _movementController.LookAtTarget(_player.position);
                // 장판 공격
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // [패턴] 총 공격 (무빙 사격)
    private IEnumerator AllOutAttackPattern()
    {
        float timer = 0f;

        Vector3 lastPlayerPos = _player != null ? _player.position : transform.position;

        while (timer < _allOutAttackDuration)
        {
            if (_player != null && _movementController != null)
            {
                _movementController.LookAtTarget(_player.position);

                Vector3 playerMoveDelta = _player.position - lastPlayerPos;
                playerMoveDelta.y = 0f; 

                if (playerMoveDelta.sqrMagnitude > 0.001f)
                {
                    float dot = Vector3.Dot(playerMoveDelta.normalized, transform.right);

                    Vector3 strafeDir = Vector3.zero;

                    if (dot > 0.1f)
                    {
                        strafeDir = transform.right;
                    }
                    else if (dot < -0.1f)
                    {
                        strafeDir = -transform.right;
                    }

                    if (strafeDir != Vector3.zero)
                    {
                        _movementController.MoveToTarget(transform.position + strafeDir * 5f);
                    }
                }
                else
                {
                    _movementController.StopMovement();
                }

                lastPlayerPos = _player.position;

                // TODO: 공격 호출
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (_movementController != null) _movementController.StopMovement();
    }

    // 돌진
    private IEnumerator DashPattern()
    {
        float timer = 0f;
        // _movementController.SetSpeed()로 돌진 속도 증가

        while (timer < _dashDuration)
        {
            if (_player != null && _movementController != null)
            {
                _movementController.LookAtTarget(_player.position); 

                // 돌진 처리

            }
            timer += Time.deltaTime;
            yield return null;
        }

        // _movementController.ResetSpeed()로 속도 복구
        if (_movementController != null) _movementController.StopMovement();
    }

    // 충격파 패턴
    private IEnumerator ShockwavePattern()
    {
        float timer = 0f;
        while (timer < _shockwaveDuration)
        {
            if (_player != null)
            {
                if (_movementController != null) _movementController.LookAtTarget(_player.position);
                // TODO: 충격파 공격
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
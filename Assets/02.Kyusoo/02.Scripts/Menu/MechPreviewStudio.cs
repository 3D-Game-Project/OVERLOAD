using UnityEngine;

public class MechPreviewStudio : MonoBehaviour
{
    public static MechPreviewStudio Instance { get; private set; }

    [SerializeField] private Transform _spawnPoint; 

    private GameObject _previewInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 정면 뷰 모델 세팅에 필요한 작업처리
    // 1. 발판 위치(_spawnPoint)에 플레이어 로봇을 복사본으로 생성
    // 2. 복사본 로봇이 PlayerInputHandler에 의해 움직이거나 공격처리하는걸 차단
    public void SetupPreviewModel(GameObject playerPrefab)
    {
        CleanUpPreview();

        if (playerPrefab == null) return;

        _previewInstance = Instantiate(playerPrefab, _spawnPoint.position, _spawnPoint.rotation * Quaternion.Euler(0f, 180f, 0f), transform);


        if (_previewInstance.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (_previewInstance.TryGetComponent(out PlayerLocomotionMotor motor))
        {
            motor.enabled = false;
        }

        if (_previewInstance.TryGetComponent(out FireManager fireManager))
        {
            fireManager.enabled = false;
        }
        if (_previewInstance.TryGetComponent(out PlayerInputHandler inputHandler))
        {
            inputHandler.enabled = false;
        }

    }

    // 방어코드
    // 이전에 생성하였던 모델이 존재한다면 제거 처리
    public void CleanUpPreview()
    {
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
        }
    }
}

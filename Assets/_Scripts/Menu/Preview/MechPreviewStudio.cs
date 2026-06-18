using System;
using UnityEngine;

public class MechPreviewStudio : MonoBehaviour
{
    public static MechPreviewStudio Instance { get; private set; }

    [SerializeField] private Transform _spawnPoint; 

    private GameObject _previewInstance;
    private PartEquipActionController _liveActionController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
    }

    private void OnDestroy()
    {
 
    }

    // 파츠 조립 코루틴이 끝나 성공(true)을 반환할 때만 새로고침을 수행.
    private void HandleLiveEquipFinished(bool success)
    {
        if (success)
        {
            RefreshPreview();
        }
    }

    public void RefreshPreview()
    {
        GameObject playerRootObj = GameObject.Find("PlayerRoot");
        Debug.Log(playerRootObj + "발견");
        if (playerRootObj != null)
        {
            SetupPreviewModel(playerRootObj); 
        }
    }

    // 정면 뷰 모델 세팅에 필요한 작업처리
    // 1. 발판 위치(_spawnPoint)에 플레이어 로봇을 복사본으로 생성
    // 2. 복사본 로봇이 PlayerInputHandler에 의해 움직이거나 공격처리하는걸 차단
    // dummyCore.enabled = false를 통해 CoreParts의 Start()가 동작되지 못하도록 제어
    public void SetupPreviewModel(GameObject playerPrefab)
    {
        CleanUpPreview();

        if (playerPrefab == null) return;

        _previewInstance = Instantiate(playerPrefab, _spawnPoint.position, _spawnPoint.rotation * Quaternion.Euler(0f, -135f, 0f), transform);

        if (_previewInstance.TryGetComponent(out CorePartsController corePartsComp))
        {
            corePartsComp.ResetYawRootsForPreview();
            corePartsComp.enabled = false;
        }

        MonoBehaviour[] allComponents = _previewInstance.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var component in allComponents)
        {
            if (component == null) continue;

            if (component.GetType().Name == "WeaponGimbalController")
            {
                component.enabled = false;

                component.transform.localRotation = Quaternion.identity;

            }
        }

        Animator[] animators = _previewInstance.GetComponentsInChildren<Animator>(true);
        foreach (var anim in animators)
        {
            if (anim != null) anim.enabled = false;
        }

        if (_previewInstance.TryGetComponent(out CorePartsController dummyCore)) dummyCore.enabled = false;
        if (_previewInstance.TryGetComponent(out PartEquipActionController dummyAction)) dummyAction.enabled = false;

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

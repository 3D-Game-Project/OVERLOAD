using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // 시네머신 3.x 필수

public class PlayerCameraController : MonoBehaviour
{
    [Header("시네머신 카메라 연결")]
    [SerializeField] private CinemachineCamera _normalCamera; 
    [SerializeField] private CinemachineCamera _aimCamera;    

    [Header("UI 연결")]
    [SerializeField] private GameObject _crosshairUI;         

    private void Start()
    {
        // 게임 시작 시 기본 상태로 초기화 (평상시 카메라가 더 높은 우선순위)
        if (_normalCamera != null) _normalCamera.Priority = 10;
        if (_aimCamera != null) _aimCamera.Priority = 5;

        if (_crosshairUI != null) _crosshairUI.SetActive(false);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        bool isAiming = Mouse.current.rightButton.isPressed;

        if (isAiming)
        {
            if (_aimCamera != null) _aimCamera.Priority = 20;

            if (_crosshairUI != null) _crosshairUI.SetActive(true);
        }
        else
        {
            if (_aimCamera != null) _aimCamera.Priority = 5;

            if (_crosshairUI != null) _crosshairUI.SetActive(false);
        }
    }
}
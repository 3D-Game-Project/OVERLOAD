using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetRotator : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform _followTarget;
    [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Mouse Look")]
    [SerializeField] private float _mouseSensitivity = 0.12f;
    [SerializeField] private float _minPitch = -30f;
    [SerializeField] private float _maxPitch = 60f;

    private float _yaw;
    private float _pitch;

    private void Start()
    {
        Vector3 euler = transform.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (_followTarget != null)
        {
            transform.position = _followTarget.position + _targetOffset;
        }

        // 마우스가 보일 때 (현재 게임 내에서는 상점이 열렸을 때만 마우스가 보이기 때문에) 회전 미처리
        if (Cursor.visible) 
            return;

        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        _yaw += mouseDelta.x * _mouseSensitivity;
        _pitch -= mouseDelta.y * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
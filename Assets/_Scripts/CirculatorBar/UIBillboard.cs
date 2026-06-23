using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private Camera _mainCamera;
    private Canvas _canvas;

    private void Start()
    {
        GameObject camObject = GameObject.Find("MainCamera");
        if (camObject != null)
        {
            _mainCamera = camObject.GetComponent<Camera>();
        }

        _canvas = GetComponent<Canvas>();

        if (_canvas != null && _canvas.renderMode == RenderMode.WorldSpace)
        {
            _canvas.worldCamera = _mainCamera;
        }
    }


    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        transform.forward = _mainCamera.transform.forward;
    }
}
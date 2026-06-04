using UnityEngine;

namespace LaserEffects
{
    public class GunAimController : MonoBehaviour
    {
        [Header("Mouse Settings")]
        public float sensitivity = 2f;

        [Header("Rotation Limits")]
        public float minX = -20f;   // Up / Down
        public float maxX = 20f;

        public float minY = -30f;   // Left / Right
        public float maxY = 30f;

        float rotX;
        float rotY;

        void Start()
        {
            Vector3 angles = transform.localEulerAngles;
            rotX = NormalizeAngle(angles.x);
            rotY = NormalizeAngle(angles.y);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            rotY += mouseX;
            rotX -= mouseY;

            rotX = Mathf.Clamp(rotX, minX, maxX);
            rotY = Mathf.Clamp(rotY, minY, maxY);

            transform.localRotation = Quaternion.Euler(rotX, rotY, 0f);
        }

        float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}

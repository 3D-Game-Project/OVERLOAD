using UnityEngine;

public class ItemFloatingEffect : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 4f;

    private float _startY;

    private void Start()
    {
        _startY = transform.position.y;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float newY = _startY + (Mathf.Sin(Time.time * bounceSpeed) * bounceHeight);

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    private void Start()
    {
        if (MonsterSpawnManager.Instance != null)
        {
            MonsterSpawnManager.Instance.RegisterSpawnPoint(this);
        }
    }

    private void OnDisable()
    {
        if (MonsterSpawnManager.Instance != null)
        {
            MonsterSpawnManager.Instance.UnregisterSpawnPoint(this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }
}
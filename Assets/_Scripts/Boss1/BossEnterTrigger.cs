using UnityEngine;

public class BossEnterTrigger : MonoBehaviour
{
    [Header("¿ÃµøΩ√≈≥ ∫∏Ω∫∑Î ¡¬«•")]
    [SerializeField] private Vector3 _bossRoomPosition = new Vector3(700f, 10f, 12f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TeleportPlayer(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = _bossRoomPosition;
            cc.enabled = true;
        }

    }
}

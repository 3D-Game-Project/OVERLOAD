using UnityEngine;

public class Shop : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MenuController menu = FindFirstObjectByType<MenuController>();
            if (menu != null)
            {
                menu.IsShop = true;
                Debug.Log("상점 진입");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 상점 영역을 완전히 벗어났을 때
        if (other.CompareTag("Player"))
        {
            MenuController menu = FindFirstObjectByType<MenuController>();
            if (menu != null)
            {
                menu.IsShop = false;
                menu.CloseMenu();
                Debug.Log("상점 이탈");
            }
        }
    }
}

using UnityEngine;

public class PathController : MonoBehaviour
{
    public GameObject downHall;      // Koridorun collider'ı
    public GameObject downStairs;   // Merdivenin collider'ı
    public Transform player;         // Darion'un konumu

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Eğer tıklanan yer karakterin aşağısındaysa (Merdivene inmek istiyorsa)
            if (mousePos.y < player.position.y)
            {
                SwitchToStairs();
            }
            // Eğer karakterin yukarısına/hizasına tıklanıyorsa (Koridorda kalmak istiyorsa)
            else
            {
                SwitchToHall();
            }
        }
    }

    void SwitchToStairs()
    {
        downHall.SetActive(false);
        downStairs.SetActive(true);
    }

    void SwitchToHall()
    {
        downHall.SetActive(true);
        downStairs.SetActive(false);
    }
}
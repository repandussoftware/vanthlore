using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public float speed = 12f;
    private bool isInitialized = false;
    private int direction = 1; // 1: Sağ, -1: Sol

    public void Setup(bool facingRight)
    {
        // 1. Yön Değişkenini Belirle
        direction = facingRight ? 1 : -1;

        // 2. Görsel Aynalama (VFXManager zaten yapıyor ama burada da sağlama alabiliriz)
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * direction; // Mevcut scale'i yönle çarpıyoruz
        transform.localScale = newScale;

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // 'Space.World' ekleyerek Unity'nin kafa karışıklığını gideriyoruz.
        // Hızımızı 'direction' ile çarparak gerçek dünya yönünü mühürlüyoruz.
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime, Space.World);
    }

}
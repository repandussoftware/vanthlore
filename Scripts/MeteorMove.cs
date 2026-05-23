using UnityEngine;

public class MeteorMove : MonoBehaviour
{
    private float speed;
    private bool isInitialized = false;

    public void Setup(float customSpeed, float customScale)
    {
        speed = customSpeed;
        transform.localScale = Vector3.one * customScale;
        
        // Meteorun rotasyonunu sıfırlıyoruz ki hep dik dursun asdas
        transform.rotation = Quaternion.identity; 
        
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Sadece aşağı hareket, takla atmak yok! cam gibi!
        transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
    }
}
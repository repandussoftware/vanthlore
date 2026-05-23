using UnityEngine;

public class OuroborosSpinner : MonoBehaviour
{
    [Header("Dönüş Ayarları")]
    [Tooltip("Pozitif (+) sola (Counter-Clockwise), Negatif (-) sağa (Clockwise) döndürür canım.")]
    [SerializeField] private float rotationSpeed = -120f; // SerializeField ekledik ki Inspector'dan görebilesin asdas

    void Update()
    {
        // UI objeleri RectTransform kullandığı için en sağlıklı dönüş yolu budur.
        // Time.unscaledDeltaTime kullanırsak, oyun durduğunda (Pause) bile spinner dönmeye devam eder cam gibi!
        transform.Rotate(0, 0, rotationSpeed * Time.unscaledDeltaTime);
    }
}
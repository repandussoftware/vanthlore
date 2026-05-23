using UnityEngine;
using UnityEngine.UI;
using TMPro; // Eğer yazın TextMeshPro ise bunu ekle
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance; 

    private CanvasGroup restingGroup;
    public float fadeSpeed = 1.0f; // Kararma hızı

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        // Ana objedeki Canvas Group'u alıyoruz
        restingGroup = GetComponent<CanvasGroup>();
        
        // Başlangıçta her şey şeffaf ve tıklanamaz olsun
        restingGroup.alpha = 0f;
        restingGroup.blocksRaycasts = false;
    }

    // Ekranı Karartma ve Yazıyı Gösterme
    public IEnumerator FadeToBlack()
    {
        Debug.Log("Uyku başlıyor, ekran kararıyor...");
        restingGroup.blocksRaycasts = true; // Kararma anında arkaya tıklanmasın

        while (restingGroup.alpha < 1f)
        {
            restingGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        restingGroup.alpha = 1f;
        Debug.Log("Ekran siyah ve yazı görünüyor.");
    }

    // Ekranı Aydınlatma ve Yazıyı Gizleme
    public IEnumerator FadeToClear()
    {
        Debug.Log("Darion uyanıyor...");
        while (restingGroup.alpha > 0f)
        {
            restingGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        restingGroup.alpha = 0f;
        restingGroup.blocksRaycasts = false; // Oyun başlayınca tıklamalar açılsın
    }
}
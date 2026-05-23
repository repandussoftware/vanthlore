using UnityEngine;
using System.Collections.Generic;

public class HelenRandomSpawner : MonoBehaviour
{
    [Header("Helen Objesi")]
    public GameObject helen; // Sahnedeki Helen objesini buraya sürükle

    [Header("Olası Uyku Noktaları")]
    public List<Transform> spawnPoints; // Helen'in uyuyabileceği boş objeler (Yatak, masa, yer vb.)

    void Start()
    {
        if (helen != null && spawnPoints.Count > 0)
        {
            SpawnHelenAtRandomPoint();
        }
    }

    void SpawnHelenAtRandomPoint()
    {
        // Listeden rastgele bir index seçiyoruz
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform selectedPoint = spawnPoints[randomIndex];

        // Helen'i seçilen noktaya taşıyoruz
        helen.transform.position = selectedPoint.position;

        // Eğer noktaların bakış yönü (rotation) önemliyse bunu da ekleyebiliriz:
        helen.transform.rotation = selectedPoint.rotation;

        Debug.Log("Helen bugün şurada uyuyor: " + selectedPoint.name);
    }
}
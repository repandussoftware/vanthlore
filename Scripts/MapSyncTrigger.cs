using UnityEngine;

public class MapSyncTrigger : MonoBehaviour
{
    [Header("Haritadaki Node İsmi")]
    public string targetNodeName; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // KRİTİK NOKTA: 'Include' diyerek kapalı objeler içinde aramayı sağlıyoruz.
            var mapController = Object.FindAnyObjectByType<MapMovementController>(FindObjectsInactive.Include);
            
            if (mapController != null)
            {
                mapController.SyncNodeFromWorld(targetNodeName);
            }
            else
            {
                Debug.LogWarning("MapMovementController sahnede bulunamadı! UIManager'ın içinde olduğundan emin ol.");
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    // Bu düğümden gidilebilecek diğer düğümler
    public MapNode leftNode;
    public MapNode rightNode;

    // Görsel olarak editörde yolları görmek için (Opsiyonel)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (leftNode != null) Gizmos.DrawLine(transform.position, leftNode.transform.position);
        if (rightNode != null) Gizmos.DrawLine(transform.position, rightNode.transform.position);
    }
}
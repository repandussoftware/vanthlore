using UnityEngine;

public class InfiniteFloor : MonoBehaviour
{
    public Transform cameraTransform; // Main Camera'yı buraya sürükle
    public float backgroundSize; // Bir Sprite'ın tam genişliği (Örn: 20.48f)
    private Transform[] layers;
    private int leftIndex;
    private int rightIndex;
    private float viewZone = 20f; // Ekrandan çıkma payı

    void Start() {
        // Floor objesi içindeki 3 Sprite'ı listeye alıyoruz
        layers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            layers[i] = transform.GetChild(i);

        leftIndex = 0;
        rightIndex = layers.Length - 1;
    }

    void Update() {
        // Darion sağa giderken kamera sağdaki parçaya yaklaştığında:
        if (cameraTransform.position.x > (layers[rightIndex].position.x - viewZone))
            ScrollRight();
        
        // Darion sola giderken (gerekirse):
        if (cameraTransform.position.x < (layers[leftIndex].position.x + viewZone))
            ScrollLeft();
    }

    private void ScrollRight() {
        int lastLeft = leftIndex;
        layers[leftIndex].position = new Vector3(layers[rightIndex].position.x + backgroundSize, layers[leftIndex].position.y, 0);
        rightIndex = lastLeft;
        leftIndex++;
        if (leftIndex == layers.Length) leftIndex = 0;
    }

    private void ScrollLeft() {
        int lastRight = rightIndex;
        layers[rightIndex].position = new Vector3(layers[leftIndex].position.x - backgroundSize, layers[rightIndex].position.y, 0);
        leftIndex = lastRight;
        rightIndex--;
        if (rightIndex < 0) rightIndex = layers.Length - 1;
    }
}
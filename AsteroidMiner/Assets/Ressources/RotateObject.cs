using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public bool randomRotation = false; // Rastgele döndürme
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // Dönüş ekseni (varsayılan olarak Y ekseni)
    public float rotationSpeed = 1f; // Dönüş hızı (derece/saniye)

    void Start()
    {
        if(randomRotation)
        {
            // Rastgele bir dönüş hızı belirle
            rotationSpeed = Random.Range(0f, 4f) - 2f;
            rotationAxis = new Vector3(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
            gameObject.transform.rotation = Random.rotation;
        }
    }

    void Update()
    {
        // Nesneyi belirli bir eksen etrafında sabit bir hızla döndür
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
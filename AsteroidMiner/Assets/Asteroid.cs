using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private float resource = 1000f; // Asteroidin kaynağı
    private float initialResource = 1000f; // Başlangıç kaynağı

    public void Initialize(float startingResource)
    {
        resource = startingResource;
        initialResource = startingResource;
        UpdateScale();
    }

    void UpdateScale()
    {
        // Ölçek kaynak miktarına göre ayarlanır
        float scale = resource / initialResource;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public float MineResource(float amount)
    {
        if (resource < amount)
        {
            Destroy(gameObject); // Kaynak sıfırsa asteroid yok edilir
            return resource;
        }

        resource -= amount;
        UpdateScale();
        return amount;
    }
}
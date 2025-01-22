using UnityEngine;

public class Asteroid : MonoBehaviour
{
    private Reserves initialReserve = new(); // Başlangıç kaynağı

    private Reserves currentReserve = new();

    public string Testname;

    public void Initialize(Reserves startingResource)
    {
        currentReserve.Add(startingResource);
        initialReserve.Add(startingResource);

        UpdateScale();
    }

    void UpdateScale()
    {
        // Ölçek kaynak miktarına göre ayarlanır
        float radius = (float)Mathf.Pow((3 * currentReserve.TotalOre) / (4 * Mathf.PI), 1.0f / 3.0f);
        
        //float scale = Mathf.Log(currentReserve.TotalOre);
        transform.localScale = new Vector3(radius, radius, radius);
    }

    public Reserves MineResource(float amount)
    {
        var minedOre = currentReserve.MineChunkOfOre(amount);

        if (currentReserve.TotalOre < amount)
        {
            Destroy(gameObject); // Kaynak sıfırsa asteroid yok edilir
            return minedOre;
        }
        
        UpdateScale();
        return minedOre;
    }
}
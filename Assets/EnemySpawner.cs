using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject bomberShipPrefab;
    public Transform spawnPoint;
    public float waveInterval = 10f;

    private int currentWave = 1;
    private float timeSinceLastWave = 0f;

    void Update()
    {
        timeSinceLastWave += Time.deltaTime;

        if (timeSinceLastWave >= waveInterval)
        {
            SpawnWave();
            timeSinceLastWave = 0f;
            currentWave++;
        }
    }

    void SpawnWave()
    {
        for (int i = 0; i < 1; i++)
        {
            var newShip = Instantiate(bomberShipPrefab, spawnPoint.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)), spawnPoint.rotation);
            newShip.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }
    }
}
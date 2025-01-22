using System.Collections.Generic;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    [Header("Asteroid Ayarları")]
    public GameObject asteroidPrefab; // Asteroid prefabı
    public Transform spawnPoint; // Asteroidlerin etrafında oluşturulacağı merkez
    public int asteroidCount = 10; // Oluşturulacak asteroid sayısı
    public float spawnRadius = 50f; // Asteroidlerin oluşturulacağı çemberin yarıçapı
    public float minDistance = 3f; // Asteroidler arasındaki minimum mesafe

    private List<GameObject> asteroids = new List<GameObject>();

    void Start()
    {
        SpawnAsteroids();
    }

    void SpawnAsteroids()
    {
        int spawned = 0;

        while (spawned < asteroidCount)
        {
            Vector3 randomPosition = GetRandomPosition();

            // Yeni asteroidin diğerlerine olan mesafesini kontrol et
            bool validPosition = true;
            foreach (var asteroid in asteroids)
            {
                if (Vector3.Distance(randomPosition, asteroid.transform.position) < minDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            // Geçerli pozisyon bulduysak asteroid oluştur
            if (validPosition)
            {
                GameObject newAsteroid = Instantiate(asteroidPrefab, randomPosition, Quaternion.identity);
                newAsteroid.transform.parent = transform;

                // Kaynak bileşenini ayarla
                Asteroid asteroidScript = newAsteroid.GetComponent<Asteroid>();
                Reserves initialReserves = new()
                {
                    Iron = 1000
                };

                asteroidScript.Testname = "initialized asteroid";

                asteroidScript.Initialize(initialReserves); // Başlangıç kaynağı 1000

                asteroids.Add(newAsteroid);
                spawned++;
            }
        }
    }

    Vector3 GetRandomPosition()
    {
        // Çemberin içinde rastgele bir pozisyon hesapla
        float angle = Random.Range(0f, Mathf.PI * 2f); // 0-360 derece
        float radius = Random.Range(0f, spawnRadius); // Çemberin yarıçapı içinde rastgele mesafe
        Vector3 position = new Vector3(
            Mathf.Cos(angle) * radius,
            spawnPoint.transform.position.y, // Yüksekliği sabit tut (2D düzlemde çember)
            Mathf.Sin(angle) * radius
        );
        return spawnPoint.position + position;
    }
}
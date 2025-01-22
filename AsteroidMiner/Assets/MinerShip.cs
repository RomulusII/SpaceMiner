using System.Collections;
using UnityEngine;

public class MinerShip : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float maxSpeed = 5f; // Maksimum hız
    public float acceleration = 2f; // İvmelenme hızı
    public float deceleration = 2f; // Yavaşlama hızı
    public float turnSpeed = 100f; // Dönüş hızı (derece/saniye)

    [Header("Y Eksenini Düzeltme")]
    public float yAxisCorrectionSpeed = 1f; // Y eksenine sıfıra dönüş hızı
    public float yAxisCorrectionThreshold = 0.1f; // Y ekseninde düzeltme başlatma eşiği

    [Header("Maden Toplama Ayarları")]
    public float oreCapacity = 50f; // Maksimum kapasite
    public float orePerSecond = 5f; // Madencilik hızı

    private Reserves currentReserve = new();

    [Header("Hedefler")]
    private bool isMining = false;

    public float maxMiningDistance = 2f; // Madencilik mesafesi
    public float dropRange = 10f; // DropPoint'e ulaşmak için gereken maksimum mesafe

    [Header("Çarpışma ve Engel Kaçınma")]
    public float avoidanceDistance = 5f; // Çarpışma tespiti mesafesi
    public float avoidanceTurnSpeed = 2f; // Engel kaçınma dönüş hızı
    public LayerMask obstacleLayer; // Engel olarak algılanacak katmanlar

    private Rigidbody rb;
    public Transform currentTarget; // Şu anki hedef

    private MeterBar cargoMeter;
    void Start()
    {
        // Kargo çubuğunu kontrol etmek için CargoMeter referansı alın
        cargoMeter = GetComponent<MeterBar>();

        rb = GetComponent<Rigidbody>();
        SelectClosestAsteroid(); // İlk hedefi seç
    }

    void Update()
    {
        if (currentTarget != null)
        {
            MoveTowardsTarget(); // Hedefe doğru hareket et

            // Eğer hedef asteroid ise ve yeterince yaklaşmışsa madenciliğe başla
            if (!isMining && currentTarget.CompareTag("Asteroid") && IsCloseToAsteroid(currentTarget, maxMiningDistance))
            {
                StartMining();
            }

            // Eğer hedef DropPoint ise ve yeterince yaklaşmışsa madeni boşalt
            if (currentTarget.CompareTag("DropPoint") && IsCloseToTarget(maxMiningDistance))
            {
                DepositOre();
            }
        } else
        {
            if (!SelectClosestAsteroid())
            {
                FindClosestDropPoint();
            }
        }

        CorrectYAxisPosition(); // Y eksenini düzelt
        cargoMeter.SetValue(currentReserve.TotalOre, oreCapacity);
    }

    private void MoveTowardsTarget()
    {
        // Hedefe olan yönü hesapla (sadece XZ düzleminde)

        Vector3 directionToTarget = currentTarget.position - transform.position;
        directionToTarget.y = 0; // Yükseklik farkını yok say

        Quaternion targetRotation;

        //// Engel algıla ve kaçınma yönünü bul
        //if (DetectObstacle(out Vector3 avoidanceDirection))
        //{

        //    // Engel algılandıysa kaçınma yönüne dön
        //    targetRotation = Quaternion.LookRotation(avoidanceDirection);
        //}
        //else
        //{
        //    // Engel yoksa hedefe bakış rotasyonu hesapla
        //    targetRotation = Quaternion.LookRotation(directionToTarget);
        //}

        targetRotation = Quaternion.LookRotation(directionToTarget);

        // Eğer hedefe dönülecek bir yön varsa
        if (directionToTarget != Vector3.zero && !IsCloseToTarget(maxMiningDistance))
        {
            // En kısa dönüş yönüne dön
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (!isMining)
        {
            // Hedef doğrultusunda hızlan
            float forwardDot = Vector3.Dot(transform.forward, directionToTarget.normalized);
            if (forwardDot > 0.9f && !IsCloseToTarget(maxMiningDistance)) // Doğru yöndeyse ve hedefe yakin degilse hızlan
            {
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, transform.forward * maxSpeed, acceleration * Time.deltaTime);
                return;
            }
        }
        // Dogru yönde degilse veya hedefe ulastiysa yavaşla
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, deceleration * Time.deltaTime);

    }

    private bool DetectObstacle(out Vector3 avoidanceDirection)
    {
        avoidanceDirection = Vector3.zero;

        // Ray'in başlangıç noktasını geminin önüne taşı
        Vector3 rayStart = transform.position + transform.forward * 1f; // 1 birim ileri

        // Çarpışma önleme: ileri doğru bir ışın gönder
        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, avoidanceDistance, obstacleLayer))
        {
            // Eğer çarpılan nesne geminin kendisiyse, engel olarak algılama
            if (hit.collider.gameObject == gameObject)
            {
                return false; // Kendi gövdesini engel sayma
            }

            // Çarpışma algılandı, ışını kırmızıya çevir
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);
            Debug.Log($"Obstacle Detected: {hit.collider.gameObject.name}");

            // Engel algılandı, kaçınma yönünü hesapla
            Vector3 hitNormal = hit.normal; // Çarpışma yüzeyinin normali
            avoidanceDirection = Vector3.Reflect(transform.forward, hitNormal); // Kaçınma yönü
            return true;
        }

        return false;
    }

    private void CorrectYAxisPosition()
    {
        // Geminin Y ekseni pozisyonunu sıfıra doğru düzelt
        if (Mathf.Abs(transform.position.y) > yAxisCorrectionThreshold)
        {
            Vector3 correctedPosition = transform.position;
            correctedPosition.y = Mathf.MoveTowards(transform.position.y, 0f, yAxisCorrectionSpeed * Time.deltaTime);
            transform.position = correctedPosition;

            // Y eksenindeki hızı azalt
            Vector3 velocity = rb.linearVelocity;
            velocity.y = Mathf.MoveTowards(rb.linearVelocity.y, 0f, yAxisCorrectionSpeed * Time.deltaTime);
            rb.linearVelocity = velocity;
        }
    }

    private bool IsCloseToTarget(float distance)
    {
        return Vector3.Distance(transform.position, currentTarget.position) <= distance;
    }

    private bool IsCloseToAsteroid(Transform asteroid, float miningDistance)
    {
        // Asteroidin SphereCollider'ını al
        SphereCollider sphereCollider = asteroid.GetComponent<SphereCollider>();
        float asteroidRadius = 0f;

        // Eğer asteroidin bir SphereCollider'ı varsa, yarıçapı al
        if (sphereCollider != null)
        {
            asteroidRadius = sphereCollider.radius * asteroid.transform.localScale.x; // Yerel ölçekle çarp
        }

        // Geminin asteroidin yüzeyine olan mesafesini kontrol et
        float distanceToSurface = Vector3.Distance(transform.position, asteroid.position) - asteroidRadius;
        return distanceToSurface <= miningDistance;
    }

    private void StartMining()
    {
        isMining = true;
        //rb.linearVelocity = Vector3.zero; // Madencilik sırasında dur
        StartCoroutine(MiningCoroutine());
    }

    private IEnumerator MiningCoroutine()
    {
        Asteroid asteroid = null;
        while (isMining && currentReserve.TotalOre < oreCapacity)
        {
            asteroid = CurrentTargetAsteroid();
            if (asteroid != null)
            {
                var chunkOfOre = orePerSecond * Time.deltaTime;
                var minedOre = asteroid.MineResource(chunkOfOre);
                currentReserve.Add(minedOre);
            }
            else
            {
                break; // Asteroid yoksa çık
            }

            yield return null;
        }

        isMining = false;

        // Kapasite dolduysa veya asteroid yoksa madeni bosaltmaya git
        if (asteroid == null || currentReserve.TotalOre >= oreCapacity)
        {
            FindClosestDropPoint();
        }
        else
        {
            SelectClosestAsteroid();
        }
    }

    private void DepositOre()
    {
        var reserveManager = Object.FindFirstObjectByType<ReserveManager>();
        reserveManager.AddReserve(currentReserve);
        currentReserve.Subtract(currentReserve); // Madeni boşalt
        SelectClosestAsteroid(); // Yeni hedefi seç
    }

    private Asteroid CurrentTargetAsteroid()
    {
        try
        {
            return currentTarget?.GetComponent<Asteroid>();
        }
        catch (MissingReferenceException)
        {
            return null;
        }
    }

    private bool SelectClosestAsteroid()
    {
        Asteroid[] asteroids = GameObject.FindObjectsByType<Asteroid>(FindObjectsSortMode.None);
        float closestDistance = Mathf.Infinity;
        Transform closestAsteroid = null;

        foreach (Asteroid asteroid in asteroids)
        {
            if (CountMinerShipsOnAsteroid(asteroid) >= 4)
                continue;
            
            float distance = Vector3.Distance(transform.position, asteroid.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAsteroid = asteroid.transform;
            }
        }

        if (closestAsteroid != null)
        {
            SetTarget(closestAsteroid);
            return true;
        }
        return false;
    }

    private int CountMinerShipsOnAsteroid(Asteroid asteroid)
    {
        MinerShip[] minerShips = GameObject.FindObjectsByType<MinerShip>(FindObjectsSortMode.None);
        int count = 0;
        foreach(var minerShip in minerShips)
        {
            if (minerShip.currentTarget == null) continue;

            var targetAsteroid = minerShip.currentTarget?.GetComponent<Asteroid>();
            if (targetAsteroid == asteroid) count++;
        }
        return count;
    }

    private void FindClosestDropPoint()
    {
        GameObject[] dropPoints = GameObject.FindGameObjectsWithTag("DropPoint");
        float closestDistance = Mathf.Infinity;
        Transform nearestPoint = null;

        foreach (GameObject dropPoint in dropPoints)
        {
            float distance = Vector3.Distance(transform.position, dropPoint.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestPoint = dropPoint.transform;
            }
        }

        if (nearestPoint != null)
        {
            SetTarget(nearestPoint);
        }
    }

    private void SetTarget(Transform target)
    {
        currentTarget = target;
    }
}
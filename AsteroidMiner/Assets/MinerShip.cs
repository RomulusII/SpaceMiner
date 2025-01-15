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
    private float currentOre = 0f; // Şu anki maden miktarı

    [Header("Hedefler")]
    private Transform currentTarget; // Şu anki hedef
    private bool isMining = false;

    public float maxMiningDistance = 2f; // Madencilik mesafesi
    public float dropRange = 10f; // DropPoint'e ulaşmak için gereken maksimum mesafe

    [Header("Kontrol")]
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SelectClosestAsteroid(); // İlk hedefi seç
    }

    void Update()
    {
        if (currentTarget != null)
        {
            MoveTowardsTarget();

            // Hedef asteroid ise ve yeterince yakınsa madenciliğe başla
            if (!isMining && currentTarget.CompareTag("Asteroid") && IsCloseToTarget(maxMiningDistance))
            {
                StartMining();
            }

            // Hedef DropPoint ise ve yeterince yakınsa madeni boşalt
            if (currentTarget.CompareTag("DropPoint") && IsCloseToTarget(dropRange))
            {
                DepositOre();
            }
        }

        CorrectYAxisPosition(); // Y eksenini düzelt
    }

    private void MoveTowardsTarget()
    {
        // Hedefe yönel
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Hedefe doğru hızlan
        float forwardDot = Vector3.Dot(transform.forward, direction);
        if (forwardDot > 0.0f) // Hedefe doğru bakıyorsa hızlan
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, transform.forward * maxSpeed, acceleration * Time.deltaTime);
        }
        else if (forwardDot < 0.0f) // Eğer ters yöne gidiyorsa yavaşla
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
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

    private void StartMining()
    {
        isMining = true;
        rb.linearVelocity = Vector3.zero; // Madencilik sırasında dur
        StartCoroutine(MiningCoroutine());
    }

    private IEnumerator MiningCoroutine()
    {
        while (isMining && currentOre < oreCapacity)
        {
            currentOre += orePerSecond * Time.deltaTime;
            yield return null;
        }

        isMining = false;

        // Kapasite dolduysa en yakın DropPoint'e git
        if (currentOre >= oreCapacity)
        {
            FindClosestDropPoint();
        }
        else
        {
            SelectClosestAsteroid(); // Madencilik bitti, yeni asteroid seç
        }
    }

    private void DepositOre()
    {
        currentOre = 0f; // Madeni boşalt
        Debug.Log("Madeni boşalttım, yeni hedef seçiliyor...");
        SelectClosestAsteroid(); // Yeni hedefi seç
    }

    private void FindClosestDropPoint()
    {
        GameObject[] dropPoints = GameObject.FindGameObjectsWithTag("DropPoint"); // Tüm DropPoint'leri bul
        float closestDistance = Mathf.Infinity;
        Transform nearestPoint = null;

        foreach (GameObject dropPoint in dropPoints)
        {
            float distance = Vector3.Distance(transform.position, dropPoint.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestPoint = dropPoint.transform; // En yakındaki DropPoint'i güncelle
            }
        }

        if (nearestPoint != null)
        {
            Debug.Log("En yakın DropPoint bulundu: " + nearestPoint.name);
            SetTarget(nearestPoint);
        }
        else
        {
            Debug.LogWarning("Hiçbir DropPoint bulunamadı!");
        }
    }

    private void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    private void SelectClosestAsteroid()
    {
        Asteroid[] asteroids = GameObject.FindObjectsByType<Asteroid>(FindObjectsSortMode.None);
        float closestDistance = Mathf.Infinity;
        Transform closestAsteroid = null;

        foreach (Asteroid asteroid in asteroids)
        {
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
        }
    }
}
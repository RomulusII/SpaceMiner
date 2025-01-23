using UnityEngine;

public class BomberShip : MonoBehaviour
{
    public float speed = 5f; // İleri hızı
    public float rotationSpeed = 100f; // Dönme hızı
    public float strafeSpeed = 3f; // Sağa sola strafe hızı
    public float shootDistance = 10f; // Atış mesafesi
    public float retreatDistance = 15f; // Kaçış mesafesi
    public GameObject bombPrefab; // Bomb prefab'ı
    public Transform bombSpawnPoint; // Bombanın atılacağı yer

    private Transform target; // MinerBase hedefi
    private Rigidbody rb;

    private void Start()
    {
        // MinerBase'i hedef al
        target = GameObject.FindWithTag("MiningBase").transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget > retreatDistance)
        {
            // Hedefe doğru yaklaş
            MoveTowardsTarget();
        }
        else if (distanceToTarget <= shootDistance)
        {
            // Saldırı yap
            ShootBomb();
            Retreat();
        }
        else
        {
            // Etrafta strafe yaparak kaç
            Strafe();
        }

        // Hedefe doğru dön
        RotateTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    private void Retreat()
    {
        Vector3 direction = (transform.position - target.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    private void Strafe()
    {
        float strafeDirection = Mathf.Sin(Time.time * strafeSpeed);
        rb.linearVelocity = transform.right * strafeDirection * strafeSpeed;
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    private void ShootBomb()
    {
        if (bombPrefab != null)
        {
            Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);
        }
    }
}
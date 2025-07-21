using UnityEngine;

public class BomberShip : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 10f;
    public float forwardAcceleration = 5f;
    public float strafeSpeed = 3f;
    public float rotationSpeed = 100f;

    public LayerMask obstacleLayer; // Engellerin layer'ı
    public LayerMask enemyLayer;   // Diğer gemilerin layer'ı

    [Header("Combat Settings")]
    public float shootDistance = 10f;
    public float retreatDistance = 15f;
    public float bombCooldown = 2f;
    public GameObject bombPrefab;
    public Transform bombSpawnPoint;

    private Transform target;
    private Rigidbody rb;
    private bool isRetreating = false;
    private bool hasBombed = false;
    private float nextBombTime = 0f;
    private float currentForwardSpeed = 0f;

    private void Start()
    {
        target = GameObject.FindWithTag("MiningBase").transform;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Düşman gemileri arasındaki çarpışmayı engelle
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy != gameObject)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), enemy.GetComponent<Collider>());
            }
        }
    }

    private void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        UpdateMovement(distanceToTarget, directionToTarget);
        UpdateCombatBehavior(distanceToTarget);
    }

    private void UpdateMovement(float distanceToTarget, Vector3 directionToTarget)
    {
        Vector3 targetDirection;

        if (isRetreating)
        {
            targetDirection = -directionToTarget; // Kaçış yönü
            currentForwardSpeed = maxForwardSpeed;
        }
        else
        {
            targetDirection = directionToTarget;

            // Hız yönetimi: Hedefe yaklaşırken hızlan veya yavaşla
            if (distanceToTarget > shootDistance * 0.8f)
            {
                currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, maxForwardSpeed, forwardAcceleration * Time.deltaTime);
            }
            else
            {
                currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, maxForwardSpeed * 0.5f, forwardAcceleration * Time.deltaTime);
            }
        }

        // Hedefe doğru açıyı hesapla ve döndür
        RotateTowardsTarget(targetDirection);

        // Hareket uygula (Y ekseni serbest)
        Vector3 movement = transform.forward * currentForwardSpeed;
        // movement.y = 0; // Bu satırı kaldırdık
        rb.linearVelocity = movement;
    }
    private void RotateTowardsTarget(Vector3 targetDirection)
    {
        // Hedef yönünü bir quaternion'a çevir (Y ekseni sabitlenmedi)
        if (targetDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);

            // Gemiyi hedef yönüne doğru döndür (smooth şekilde)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateCombatBehavior(float distanceToTarget)
    {
        if (isRetreating)
        {
            if (distanceToTarget >= retreatDistance)
            {
                isRetreating = false;
                hasBombed = false;
            }
        }
        else if (distanceToTarget <= shootDistance && !hasBombed && Time.time >= nextBombTime)
        {
            ShootBomb();
            isRetreating = true;
            hasBombed = true;
            nextBombTime = Time.time + bombCooldown;
        }
    }

    private void ShootBomb()
    {
        if (bombPrefab != null && bombSpawnPoint != null)
        {
            GameObject bomb = Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);

            // Bombayı küçült
            bomb.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Bu değerleri ihtiyaca göre ayarlayın

            Projectile bombRb = bomb.GetComponent<Projectile>();
            if (bombRb != null)
            {
                bombRb.speed += currentForwardSpeed;
            }

            var team = GetComponent<TeamObject>();
            var teamBomb = bomb.GetComponent<TeamProjectile>();
            if (team != null && teamBomb != null)
            {
                teamBomb.SetTeam(team.teamID);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Enemy"))
        //{
        //    // Eğer başka bir düşman gemisiyle çarpışma olursa, hareketi durdur
        //    rb.linearVelocity = Vector3.zero;
        //}
    }
}
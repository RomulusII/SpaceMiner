using UnityEngine;

public class BomberShip : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 10f;
    public float forwardAcceleration = 5f;
    public float strafeSpeed = 3f;
    public float rotationSpeed = 100f;
    public float maxBankAngle = 30f;

    public LayerMask obstacleLayer; // Engellerin layer'ı

    [Header("Combat Settings")]
    public float shootDistance = 10f;
    public float retreatDistance = 15f;
    public float bombCooldown = 2f;
    public GameObject bombPrefab;
    public Transform bombSpawnPoint;

    [Header("Collision Recovery")]
    public float stunDuration = 0f; // Çarpışma sonrası sersemleme süresi
    public float recoverySpeed = 5f; // Toparlanma hızı
    public float bounceForce = 5f; // Çarpışmadan sonra geri tepme kuvveti

    [Header("Vertical Movement")]
    public float maxVerticalSpeed = 20f;        // Daha yüksek
    public float verticalAcceleration = 30f;    // Daha yüksek
    public float returnToPlaneSpeed = 5f;       // Daha yüksek
    public float idealFlightHeight = 0f;        // Yerden yükseklik
    public float collisionAvoidanceDistance = 10f; // Daha uzun mesafeden tespit

    private float currentVerticalSpeed = 0f;
    private bool isAvoidingCollision = false;

    private Transform target;
    private Rigidbody rb;
    private bool isRetreating = false;
    private bool hasBombed = false;
    private bool isStunned = false;
    private float nextBombTime = 0f;
    private float currentForwardSpeed = 0f;
    private float stunEndTime = 0f;
    private Vector3 lastCollisionNormal;
    private Vector3 desiredRetreatDirection;

    private void Start()
    {
        target = GameObject.FindWithTag("MiningBase").transform;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Layer'ı kontrol et
        if (obstacleLayer == 0) // Eğer Inspector'da set edilmemişse
        {
            obstacleLayer = LayerMask.GetMask("ObstacleLayer");
            Debug.Log($"Obstacle layer automatically set to: {obstacleLayer.value}");
        }
    }

    private void Update()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        if (isStunned)
        {
            HandleStunnedState();
            return;
        }

        UpdateMovement(distanceToTarget, directionToTarget);
        UpdateCombatBehavior(distanceToTarget);
        CheckForObstacles();
    }

    private void HandleStunnedState()
    {
        if (Time.time > stunEndTime)
        {
            isStunned = false;
            // Çarpışmadan sonra kaçış moduna geç
            isRetreating = true;
            desiredRetreatDirection = -lastCollisionNormal;
        }
        else
        {
            // Sersemleme durumunda minimal hareket
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.deltaTime * recoverySpeed);
        }
    }

    private void CheckForObstacles()
    {
        bool obstacleDetected = false;
        RaycastHit hit;
        float sphereRadius = 1f; // Daha geniş tespit alanı

        // İleri kontrol
        Vector3 castStart = transform.position + transform.forward * sphereRadius; // Başlangıç pozisyonunu Collider dışında başlat
        if (Physics.SphereCast(castStart, sphereRadius, transform.forward, out hit, collisionAvoidanceDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != gameObject) // Kendi Collider'ını yok say
            {
                obstacleDetected = true;
                HandleObstacleAvoidance(hit.normal);
                Debug.Log($"Obstacle detected ahead: {hit.collider.gameObject.name}"); // Algılanan engel bilgisi
            }
        }

        // Çapraz kontroller
        Vector3[] checkDirections = new Vector3[]
        {
        (transform.forward + transform.right).normalized,
        (transform.forward - transform.right).normalized,
        (transform.forward + Vector3.up).normalized,
        (transform.forward + Vector3.down).normalized
        };

        foreach (Vector3 dir in checkDirections)
        {
            if (Physics.SphereCast(castStart, sphereRadius, dir, out hit, collisionAvoidanceDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject != gameObject) // Kendi Collider'ını yok say
                {
                    obstacleDetected = true;
                    HandleObstacleAvoidance(hit.normal);
                    Debug.Log($"Obstacle detected diagonally: {hit.collider.gameObject.name}"); // Algılanan engel bilgisi
                    break;
                }
            }
        }

        // Yukarı/aşağı kontrol
        if (Physics.Raycast(castStart, Vector3.down, out hit, collisionAvoidanceDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != gameObject) // Kendi Collider'ını yok say
            {
                obstacleDetected = true;
                HandleObstacleAvoidance(Vector3.up);
                Debug.Log($"Obstacle detected below: {hit.collider.gameObject.name}"); // Algılanan engel bilgisi
            }
        }
        else if (Physics.Raycast(castStart, Vector3.up, out hit, collisionAvoidanceDistance, obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != gameObject) // Kendi Collider'ını yok say
            {
                obstacleDetected = true;
                HandleObstacleAvoidance(Vector3.down);
                Debug.Log($"Obstacle detected above: {hit.collider.gameObject.name}"); // Algılanan engel bilgisi
            }
        }

        // Çarpışma tespit edildi mi?
        isAvoidingCollision = obstacleDetected;

        // Eğer çarpışma tespit edilmediyse ideal yüksekliğe dön
        if (!obstacleDetected)
        {
            float heightDifference = idealFlightHeight - transform.position.y;
            currentVerticalSpeed = Mathf.Lerp(
                currentVerticalSpeed,
                heightDifference * returnToPlaneSpeed,
                Time.deltaTime
            );

            Debug.Log($"Returning to ideal height. Height Difference: {heightDifference}, Current Vertical Speed: {currentVerticalSpeed}");
        }
    }

    private void HandleObstacleAvoidance(Vector3 avoidanceNormal)
    {
        // Daha güçlü dikey kaçınma
        if (avoidanceNormal.y > 0)
        {
            currentVerticalSpeed = maxVerticalSpeed;
        }
        else if (avoidanceNormal.y < 0)
        {
            currentVerticalSpeed = -maxVerticalSpeed;
        }

        // Daha keskin yatay kaçınma
        Vector3 horizontalAvoidance = Vector3.ProjectOnPlane(avoidanceNormal, Vector3.up).normalized;

        if (horizontalAvoidance.magnitude > 0.1f)
        {
            Vector3 targetDirection = transform.forward + horizontalAvoidance * 3f;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime * 2f
            );
        }

        // Daha hızlı yavaşlama
        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed,
            maxForwardSpeed * 0.3f,
            forwardAcceleration * Time.deltaTime * 2f);

        // Hareketi uygula
        Vector3 movement = transform.forward * currentForwardSpeed;
        movement.y = currentVerticalSpeed;
        rb.linearVelocity = movement;
    }

    private void UpdateMovement(float distanceToTarget, Vector3 directionToTarget)
    {
        Vector3 targetDirection;
        float rotationInput;

        if (isRetreating)
        {
            // Kaçış yönünü kullan
            targetDirection = desiredRetreatDirection;
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

        // Hedefe doğru açıyı hesapla
        float angleToTarget = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);
        rotationInput = Mathf.Clamp(angleToTarget / 45f, -1f, 1f);

        // Gemiyi hedefe doğru döndür
        RotateTowardsTarget(targetDirection);

        // Hareket uygula
        ApplyMovement(rotationInput, 0f);
    }

    private void RotateTowardsTarget(Vector3 targetDirection)
    {
        // Hedef yönünü bir quaternion'a çevir
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Gemiyi hedef yönüne doğru döndür (smooth şekilde)
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ApplyMovement(float rotationInput, float strafeInput)
    {
        // Dönüş
        transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);

        // Yanlara yatma
        float targetBankAngle = -strafeInput * maxBankAngle;
        Vector3 currentEuler = transform.rotation.eulerAngles;
        float currentBank = currentEuler.z;
        if (currentBank > 180) currentBank -= 360;
        float newBank = Mathf.MoveTowards(currentBank, targetBankAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, newBank);

        // Yatay hareket
        Vector3 movement = transform.forward * currentForwardSpeed;
        movement += transform.right * strafeInput * strafeSpeed;

        // Dikey hareket yönetimi
        if (!isAvoidingCollision)
        {
            // İdeal yüksekliğe dön
            float heightDifference = idealFlightHeight - transform.position.y;
            currentVerticalSpeed = Mathf.MoveTowards(
                currentVerticalSpeed,
                heightDifference * returnToPlaneSpeed,
                verticalAcceleration * Time.deltaTime
            );
        }

        // Toplam hareketi uygula
        movement.y = currentVerticalSpeed;
        rb.linearVelocity = movement;
    }

    private void UpdateCombatBehavior(float distanceToTarget)
    {
        if (isRetreating)
        {
            if (distanceToTarget >= retreatDistance)
            {
                isRetreating = false;
                hasBombed = false;
                desiredRetreatDirection = Vector3.zero;
            }
        }
        else if (distanceToTarget <= shootDistance && !hasBombed && Time.time >= nextBombTime)
        {
            ShootBomb();
            isRetreating = true;
            hasBombed = true;
            nextBombTime = Time.time + bombCooldown;
            // Kaçış yönünü hesapla
            desiredRetreatDirection = (transform.position - target.position).normalized;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isStunned = true;
        stunEndTime = Time.time + stunDuration;
        lastCollisionNormal = collision.contacts[0].normal;

        // Çarpışmadan kaçış için 3D vektör kullan
        Vector3 bounceVelocity = lastCollisionNormal * bounceForce;
        rb.linearVelocity = bounceVelocity;

        desiredRetreatDirection = new Vector3(lastCollisionNormal.x, 0, lastCollisionNormal.z).normalized;
    }


    private void ShootBomb()
    {
        if (bombPrefab != null && bombSpawnPoint != null)
        {
            // Bombayı oluştur
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * collisionAvoidanceDistance);
    }

    private void OnDrawGizmos()
    {
        //// Normal gizmoları çiz
        //OnDrawGizmosSelected();

        //// Dikey kontrol ışınlarını göster
        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(transform.position, Vector3.up * collisionAvoidanceDistance);
        //Gizmos.DrawRay(transform.position, Vector3.down * collisionAvoidanceDistance);
    }

}
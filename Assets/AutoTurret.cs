using UnityEngine;

public class AutoTurret : MonoBehaviour
{
    [Header("Turret Settings")]
    public Transform gunBarrel; // Merminin doğacağı nokta
    public GameObject bulletPrefab; // Mermi prefabı
    public float fireRate = 2f; // Saniyede atış sayısı
    public float bulletSpeed = 30f; // Merminin hızı
    public float detectionRange = 50f; // Taretin algılama menzili
    public float baseBulletDamage = 10f; // Merminin vereceği taban hasar

    [Header("Targeting Settings")]
    public LayerMask enemyLayer; // Düşmanları bulmak için kullanılacak layer
    public float rotationSpeed = 5f; // Taretin dönüş hızı

    private float fireCooldown = 0f; // Ateşleme için bekleme süresi
    private Transform currentTarget; // Taretin hedef aldığı düşman

    private bool isAimed = false; // Hedefe nişan alındı mı?

    private void Update()
    {
        // Hedefi güncelle
        UpdateTarget();

        // Eğer hedef varsa, hedefe nişan al ve ateş et
        if (currentTarget != null)
        {
            AimAtTarget();
            if (isAimed && fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / fireRate; // Ateşleme süresini ayarla
            }
        }

        // Cooldown süresini azalt
        fireCooldown -= Time.deltaTime;
    }

    private void UpdateTarget()
    {
        // Belirtilen algılama menzilinde tüm düşmanları bul
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider enemy in enemies)
        {
            if (enemy.tag.ToLower() == "asteroid") continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        // En yakın düşmanı hedef olarak seç
        currentTarget = nearestEnemy;
    }

    private void AimAtTarget()
    {
        isAimed = false; // Her seferinde baştan kontrol et

        if (currentTarget == null) return;

        // Düşmanın pozisyonunu ve hızını hesaba kat
        Rigidbody enemyRb = currentTarget.GetComponentInParent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 enemyVelocity = enemyRb.linearVelocity;

            // Hedefin gelecekteki pozisyonunu tahmin et
            Vector3 futurePosition = PredictFuturePosition(currentTarget.position, enemyVelocity);

            // Taretin hedefe doğru dönmesini sağla
            Vector3 direction = (futurePosition - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // Açısal farkı hesapla
            float angleDiff = Quaternion.Angle(transform.rotation, lookRotation);

            // %1'den az ise nişan alındı kabul et (360 * 0.01 = 3.6 derece)
            if (angleDiff < 3.6f)
            {
                isAimed = true;
            }
        }
    }

    private Vector3 PredictFuturePosition(Vector3 targetPosition, Vector3 targetVelocity)
    {
        // Hedefin gelecekteki pozisyonunu hesapla
        Vector3 directionToTarget = targetPosition - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        // Merminin hedefe varma süresini hesapla
        float timeToTarget = distanceToTarget / bulletSpeed;

        // Gelecekteki pozisyonu tahmin et
        return targetPosition + targetVelocity * timeToTarget;
    }

    private void Fire()
    {
        if (bulletPrefab == null || gunBarrel == null) return;

        // Mermiyi oluştur
        GameObject bullet = Instantiate(bulletPrefab, gunBarrel.position, gunBarrel.rotation * Quaternion.Euler(0, 90f, 0));

        // Merminin özelliklerini ayarla
        Projectile bulletScript = bullet.GetComponent<Projectile>();
        if (bulletScript != null)
        {
            bulletScript.speed = bulletSpeed;
            bulletScript.damage = baseBulletDamage; // Burada damage atanıyor
        }

        // Mermiyi belirli bir süre sonra yok et
        Destroy(bullet, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        // Algılama menzilini görselleştirmek için
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

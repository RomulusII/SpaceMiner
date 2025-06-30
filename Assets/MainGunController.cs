using UnityEngine;

public class MainGunController : MonoBehaviour
{
    public Transform gunBarrel; // Merminin doğacağı nokta  
    public GameObject bulletPrefab; // Mermi prefabı  
    public float fireRate = 5f; // Saniyede atılan mermi sayısı  
    public float bulletDamage = 10f; // Merminin verdiği hasar  
    public float bulletSpeed = 200f; // Mermi hızı  

    private float fireCooldown = 0f; // Ateşleme için bekleme süresi  

    public void Update()
    {
        // Fare veya dokunmatik ekran girdisini takip et  
        FollowMouse();

        // Sol tıklama veya dokunmatik ekrana dokunma  
        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Fire();
            fireCooldown = 1f / fireRate; // Ateşleme süresini ayarla  
        }

        // Cooldown süresini azalt  
        fireCooldown -= Time.deltaTime;
    }

    private void FollowMouse()
    {
        // Fare pozisyonunu ekrandan al
        Vector3 mousePosition = Input.mousePosition;

        // Fare pozisyonunu dünya koordinatlarına çevir
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // Y = 0 + position.y düzlemini temsil eden bir düzlem oluştur
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        // Eğer ışın (ray) düzlemle kesişiyorsa
        if (groundPlane.Raycast(ray, out float enter))
        {
            // Kesim noktasını hesapla
            Vector3 targetPoint = ray.GetPoint(enter);

            // Silahı hedefe döndür (sadece Y ekseninde dönüş yapacak)
            Vector3 direction = (targetPoint - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y - 90f, 0);
        }
    }

    private void Fire()
    {
        // Mermiyi oluştur  
        GameObject bullet = Instantiate(bulletPrefab, gunBarrel.position, gunBarrel.rotation * Quaternion.Euler(0, 90f, 0));

        var team = GetComponentInParent<TeamObject>();
        var projectile = bullet.GetComponent<TeamProjectile>();
        projectile.SetTeam(team.teamID);

        // Merminin hasarını ata  
        Projectile bulletScript = bullet.GetComponent<Projectile>();
        if (bulletScript != null)
        {
            bulletScript.damage = bulletDamage;
            bulletScript.speed = bulletSpeed; // Merminin hızını ayarla
            bulletScript.lifeTime = 5f; // Merminin yaşam süresi
        }

        // Mermiyi belirli bir süre sonra yok et  
        //Destroy(bullet, 5f);
    }
}
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Bombanın hızı
    public float damage = 20f; // Bombanın hasar gücü
    public float lifeTime = 5f; // Bombanın yaşam süresi

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Bomba belirli bir süre sonra yok olur
    }

    private void Update()
    {
        // İleri hareket
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Eğer bir "Destructable" nesneye çarparsa hasar ver
        Destructable destructable = other.GetComponent<Destructable>();
        if (destructable != null)
        {
            destructable.TakeDamage(damage);
        }

        // Bomba çarpışmadan sonra yok olur
        Destroy(gameObject);
    }
}
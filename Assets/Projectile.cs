using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Bombanın hızı
    public float damage = 20f; // Bombanın hasar gücü
    public float lifeTime = 5f; // Bombanın yaşam süresi
    private bool hasCollided = false; // Çarpışma durumu kontrolü

    private int teamID => gameObject.GetComponent<TeamProjectile>().teamID;

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

        // Eğer çarpılan nesne aynı takımdaysa etkileşimi yok say
        TeamObject teamObject = other.GetComponentInParent<TeamObject>();
        if (teamObject != null && teamObject.teamID == teamID)
        {
            return; // Aynı takımdan olduğu için çarpışmayı yok say
        }

        if (hasCollided) return; // Eğer çarpıştıysa bir daha tetiklenmesin

        hasCollided = true; // Çarpışma durumunu ayarla

        // Eğer bir asteroid veya nötr nesneye çarptıysa
        if (other.gameObject.layer == LayerMask.NameToLayer("Team0Ships"))
        {
            Debug.Log("Asteroide çarpıldı!");
            // Asteroide hasar verme kodu buraya eklenebilir
        }

        // Eğer bir "Destructable" nesneye çarparsa hasar ver
        Destructable destructable = other.GetComponentInParent<Destructable>();
        if (destructable != null)
        {
            destructable.TakeDamage(damage);
        }

        // Bomba çarpışmadan sonra yok olur
        Destroy(gameObject);
    }

}
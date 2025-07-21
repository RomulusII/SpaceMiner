using UnityEngine;

public class Destructable : Damageable
{
    public override float TakeDamage(float damage) 
    {
        var remDamage = base.TakeDamage(damage);

        if (Hitpoints <= 0)
        {
            Destroy(gameObject); // Can 0'ın altına düşerse nesneyi yok et
        }
        return remDamage; // Geriye kalan hasarı döndür
    }
}

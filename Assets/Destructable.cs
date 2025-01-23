using UnityEngine;

public class Destructable : HealthMeter
{

    public float Hitpoints;
    public float initialHitpoints;

    private void Start()
    {
        initialHitpoints = Hitpoints;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        Hitpoints -= damage;

        UpdateHealthBar();

        if (Hitpoints <= 0)
        {
            Destroy(gameObject); // Can 0'ın altına düşerse nesneyi yok et
        }
    }

    private void UpdateHealthBar()
    {
        SetValue(Hitpoints, initialHitpoints);
    }
}
public class Damageable : HealthMeter
{
    public float Hitpoints;
    public float initialHitpoints;

    private void Start()
    {
        initialHitpoints = Hitpoints;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        SetValue(Hitpoints, initialHitpoints);
    }

    public virtual float TakeDamage(float damage)
    {
        Hitpoints -= damage;
        if (Hitpoints < 0)
        {
            damage = -Hitpoints;
            Hitpoints = 0;
        }
        else
        {
            damage = 0; // Hasar 0'ın altına düşerse 0 olarak ayarla
        }

        UpdateHealthBar();
        return damage; // Geriye kalan hasarı döndür
    }
}
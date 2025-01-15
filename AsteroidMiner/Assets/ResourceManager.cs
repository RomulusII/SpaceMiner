using UnityEngine;
using TMPro;

public class ReserveManager : MonoBehaviour
{
    // Reserves sınıfı, tüm kaynakları saklar
    private Reserves reserves;

    // UI Referansları (TextMeshPro)
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI uraniumText;
    public TextMeshProUGUI aluminiumText;
    public TextMeshProUGUI powerText;


    void Start()
    {
        // Kaynaklar başlangıç değerleri ile oluşturulur
        reserves = new Reserves
        {
            Iron = 1000f,
            Uranium = 500f,
            Aluminium = 800f,
            Power = 300f
        };

        UpdateUI(); // Başlangıçta UI'yi güncelle
    }

    public Reserves GetReserves() => reserves;

    // Tek bir kaynağı artırmak için
    public void AddReserve(ReserveType reserveType, float amount)
    {
        switch (reserveType)
        {
            case ReserveType.Iron:
                reserves.Iron += amount;
                break;
            case ReserveType.Uranium:
                reserves.Uranium += amount;
                break;
            case ReserveType.Aluminium:
                reserves.Aluminium += amount;
                break;
            case ReserveType.Power:
                reserves.Power += amount;
                break;
        }

        UpdateUI(); // UI'yi güncelle
    }

    // Birden fazla kaynağı artırmak için (overload)
    public void AddReserve(Reserves addedReserves)
    {
        reserves.Iron += addedReserves.Iron;
        reserves.Uranium += addedReserves.Uranium;
        reserves.Aluminium += addedReserves.Aluminium;
        reserves.Power += addedReserves.Power;

        UpdateUI(); // UI'yi güncelle
    }

    public bool SpendReserve(ReserveType reserveType, float amount)
    {
        bool success = false;
        switch (reserveType)
        {
            case ReserveType.Iron:
                if (reserves.Iron >= amount)
                {
                    reserves.Iron -= amount;
                    success = true;
                }
                break;
            case ReserveType.Uranium:
                if (reserves.Uranium >= amount)
                {
                    reserves.Uranium -= amount;
                    success = true;
                }
                break;
            case ReserveType.Aluminium:
                if (reserves.Aluminium >= amount)
                {
                    reserves.Aluminium -= amount;
                    success = true;
                }
                break;
            case ReserveType.Power:
                if (reserves.Power >= amount)
                {
                    reserves.Power -= amount;
                    success = true;
                }
                break;
        }

        UpdateUI(); // UI'yi güncelle
        return success;
    }

    private void UpdateUI()
    {
        // UI'deki metinleri kaynak değerleri ile güncelle
        if (ironText != null) ironText.text = $"Iron: {reserves.Iron:F2}";
        if (uraniumText != null) uraniumText.text = $"Uranium: {reserves.Uranium:F2}";
        if (aluminiumText != null) aluminiumText.text = $"Aluminium: {reserves.Aluminium:F2}";
        if (powerText != null) powerText.text = $"Power: {reserves.Power:F2}";
    }
}
using UnityEngine;
using UnityEngine.UI;

public class MeterBar : MonoBehaviour
{
    [Header("UI Settings")]
    public Image emptyBar; // arka plan çizgisi
    public Image fullBar; // gösterge çizgisi
    public bool ShowIfFull = false;
    public bool ShowIfEmpty = false;

    public void SetValue(float currentValue, float maxValue)
    {        
        currentValue = currentValue < 0 ? 0 : currentValue;
        currentValue = currentValue > maxValue ? maxValue : currentValue;

        maxValue = maxValue == 0 ? 1 : maxValue;

        // Doluluk oranını hesapla (% olarak)
        float fillPercent = currentValue / maxValue;
        // Gösterge çizgisinin genişliğini ayarla
        if (emptyBar != null && fullBar != null)
        {
            fullBar.enabled = emptyBar.enabled = (ShowIfFull || currentValue < maxValue) && (ShowIfEmpty || currentValue > 0);

            // Gösterge çizgisinin genişliği, baz çizginin genişliği ile orantılı
            RectTransform emptyBarRect = emptyBar.rectTransform;
            RectTransform fullBarRect = fullBar.rectTransform;

            // Gösterge çizgisinin genişliğini doluluk oranına göre ayarla
            fullBarRect.sizeDelta = new Vector2(emptyBarRect.sizeDelta.x * fillPercent, emptyBarRect.sizeDelta.y);
        }
    }
}

public class CargoMeter : MeterBar { }
public class HealthMeter : MeterBar { }

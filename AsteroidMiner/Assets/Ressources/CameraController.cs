using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 100f; // Hareket hızı
    public float rotationSpeed = 20f; // Döndürme hızı
    public float scrollSpeed = 20f; // Yakınlaştırma/uzaklaştırma hızı
    public float minDistance = 10f; // Kamera için minimum mesafe
    public float maxDistance = 1000f; // Kamera için maksimum mesafe
    private float factor = 1f;

    void Update()
    {
        factor = Input.GetKey(KeyCode.LeftShift) ? 10f : 1f; // Eğer Shift tuşu basılıysa hızı 2x arttır

        // 1. Kamerayı hareket ettir (ASWD)
        HandleMovement();

        // 2. Kamerayı döndür (Q ve E)
        HandleRotation();

        // 3. Mouse tekerleği ile yakınlaştırma/uzaklaştırma
        HandleZoom();
    }

    private void HandleMovement()
    {
        // Klavyeden girişleri al (ASWD tuşları)
        float horizontal = Input.GetAxis("Horizontal"); // A (-1) ve D (+1)
        float vertical = Input.GetAxis("Vertical");     // W (+1) ve S (-1)

        // Kameranın yatay (X-Z düzlemindeki) yönünü hesapla
        Vector3 forward = transform.forward;
        forward.y = 0; // Y bileşenini sıfırla (yatay hareket için)
        forward.Normalize(); // Vektörü normalize et

        Vector3 right = transform.right;
        right.y = 0; // Y bileşenini sıfırla
        right.Normalize(); // Vektörü normalize et

        // Hareket vektörünü hesapla
        Vector3 movement = forward * vertical + right * horizontal;

        // Kamerayı hareket ettir
        transform.position += movement * moveSpeed * Time.deltaTime * factor;
    }

    private void HandleRotation()
    {
        // Q ve E tuşlarıyla Y ekseni etrafında kamerayı döndür
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime * factor, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * factor, Space.World);
        }
    }

    private void HandleZoom()
    {
        // Mouse tekerleği girişini al
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Eğer herhangi bir scroll girişi yoksa işlem yapma
        if (Mathf.Abs(scrollInput) < 0.01f) return;

        // Kameranın dinamik focus point'ini hesapla
        Vector3 focusPoint = CalculateFocusPoint();

        // Kameranın odak noktasına olan mesafesini hesapla
        float distance = transform.position.y; // Vector3.Distance(transform.position, focusPoint);

        var yeniPos = transform.position + transform.forward * scrollInput * scrollSpeed * factor;

        var minMaxFactor = 1f;

        if (yeniPos.y > maxDistance)
        {
            var deltaYeni = yeniPos.y - transform.position.y;
            var deltaMax = maxDistance - transform.position.y;

            minMaxFactor = deltaMax / deltaYeni;
        }

        if (yeniPos.y < minDistance)
        {
            var deltaYeni = yeniPos.y - transform.position.y;
            var deltaMin = minDistance - transform.position.y;

            minMaxFactor = deltaMin / deltaYeni;
        }

        // Kamerayı focus point'e doğru veya uzağa hareket ettir
        transform.position += transform.forward * scrollInput * scrollSpeed * factor * minMaxFactor;
        

        //// Tekerlek girişine göre kamerayı ileri veya geri hareket ettir
        //if ((scrollInput > 0f && distance > minDistance) || (scrollInput < 0f && distance < maxDistance))
        //{
        //    // Kamerayı focus point'e doğru veya uzağa hareket ettir
        //    transform.position += transform.forward * scrollInput * scrollSpeed * factor;
        //}
    }

    // Dinamik focus point hesaplama: Kameranın yön vektöründen y=0 düzlemini kesen noktayı bul
    private Vector3 CalculateFocusPoint()
    {
        // Kameranın pozisyonu ve yön vektörü
        Vector3 cameraPosition = transform.position;
        Vector3 cameraForward = transform.forward;

        // Y = 0 düzlemini kesmek için bir oran hesapla
        float t = -cameraPosition.y / cameraForward.y;

        // Kamera yönünde çizginin Y = 0 düzlemini kestiği noktayı hesapla
        Vector3 focusPoint = cameraPosition + cameraForward * t;

        return focusPoint;
    }
}
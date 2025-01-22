using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // Ana kamerayı bulun
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // Canvas'ı sürekli kameraya doğru döndür
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
}
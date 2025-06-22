using UnityEngine;

public class AimAtMouse : MonoBehaviour
{
    void Update()
    {
        // Fare pozisyonunu dünya koordinatýna çevir
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Z eksenini sýfýrla çünkü 2D

        // FirePoint yönünü hesapla
        Vector3 direction = mousePos - transform.position;

        // Açýyý hesapla
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // FirePoint objesini döndür
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

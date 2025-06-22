using UnityEngine;

public class AimAtMouse : MonoBehaviour
{
    void Update()
    {
        // Fare pozisyonunu d�nya koordinat�na �evir
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Z eksenini s�f�rla ��nk� 2D

        // FirePoint y�n�n� hesapla
        Vector3 direction = mousePos - transform.position;

        // A��y� hesapla
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // FirePoint objesini d�nd�r
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

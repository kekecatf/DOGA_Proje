using UnityEngine;

public class JoystickFireController : MonoBehaviour
{
    public Joystick fireJoystick;          // Sadece ateþ için kullanýlacak joystick
    public Transform firePoint;                 // Silahýn ucu
    public GameObject bulletPrefab;             // Mermi prefabý
    public float fireRate = 0.2f;               // Ateþ etme sýklýðý

    private float fireTimer = 0f;

    void Update()
    {
        Vector2 direction = new Vector2(fireJoystick.Horizontal, fireJoystick.Vertical);

        if (direction.magnitude > 0.1f)
        {
            // Joystick yönüne göre firePoint objesini döndür
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

            // Belirli aralýklarla ateþ et
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                Fire();
                fireTimer = 0f;
            }
        }
        else
        {
            fireTimer = fireRate;
        }
    }

    void Fire()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}

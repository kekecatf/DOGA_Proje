using UnityEngine;

public class JoystickFireController : MonoBehaviour
{
    public Joystick fireJoystick;          // Sadece ate� i�in kullan�lacak joystick
    public Transform firePoint;                 // Silah�n ucu
    public GameObject bulletPrefab;             // Mermi prefab�
    public float fireRate = 0.2f;               // Ate� etme s�kl���

    private float fireTimer = 0f;

    void Update()
    {
        Vector2 direction = new Vector2(fireJoystick.Horizontal, fireJoystick.Vertical);

        if (direction.magnitude > 0.1f)
        {
            // Joystick y�n�ne g�re firePoint objesini d�nd�r
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

            // Belirli aral�klarla ate� et
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

using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public BulletPool1 bulletPool;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        RotateTowardsCursor();
        Debug.Log("Current Time.timeScale: " + Time.timeScale);

        if (Input.GetKeyDown(KeyCode.Mouse0)) // Fire on left mouse click
        {
            Fire();
        }
    }

    void RotateTowardsCursor()
    {
        if (firePoint == null) return;

        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure it's 2D

        // Calculate direction to mouse
        Vector2 direction = (mousePosition - firePoint.position).normalized;

        // Rotate firePoint to look at the cursor
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Fire()
    {
        if (firePoint == null || bulletPool == null)
        {
            Debug.LogWarning("FirePoint or BulletPool is not assigned!");
            return;
        }

        GameObject bullet = bulletPool.GetBullet1();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayShootSound();
        }
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * projectileSpeed;
        }

        bullet.GetComponent<Projectile>().SetPool(bulletPool);
    }
}

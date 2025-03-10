using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public BulletPool bulletPool;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) // Fire on left mouse click
        {
            Fire();
        }
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

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * projectileSpeed;
        }

        bullet.GetComponent<Bullet>().SetPool(bulletPool);
    }
}

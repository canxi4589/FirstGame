using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public GameObject projectilePrefab; // Projectile prefab
    public Transform firePoint; // Where the projectile spawns
    public float projectileSpeed = 10f;
    public PlayerMovement playerMovement; // Reference to PlayerMovement script

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        FollowCursor();

        if (Input.GetKeyDown(KeyCode.Mouse0)) // Fire on left mouse click
        {
            Fire();
        }
    }

    void FollowCursor()
    {
        if (firePoint == null) return;

        // Get mouse position in world coordinates
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Make weapon holder follow the cursor
        transform.position = mousePosition;

        // Rotate weapon holder to look at cursor
        Vector2 direction = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Fire()
    {
        if (firePoint == null || projectilePrefab == null)
        {
            Debug.LogWarning("FirePoint or ProjectilePrefab is not assigned!");
            return;
        }

        // Get direction
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Vector2 direction = (mousePosition - firePoint.position).normalized;

        // Update aiming direction in playerMovement
        if (playerMovement != null)
        {
            playerMovement.UpdateAimingDirection(direction);
        }

        // Instantiate projectile
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Rotate projectile to match fire direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Move bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }
    }
}

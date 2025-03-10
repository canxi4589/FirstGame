using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public GameObject projectilePrefab; // Projectile prefab
    public Transform firePoint; // Where the projectile spawns
    public float projectileSpeed = 10f;
    public PlayerMovement playerMovement; // Reference to PlayerMovement script

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) // Fire on left mouse click
        {
            Fire();
        }
    }

    void Fire()
    {
        if (firePoint == null || projectilePrefab == null)
        {
            Debug.LogWarning("FirePoint or ProjectilePrefab is not assigned!");
            return;
        }

        // Get mouse position in world coordinates
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure the Z-axis is 0 for 2D

        // Calculate direction from FirePoint to Mouse
        Vector2 direction = (mousePosition - firePoint.position).normalized;
        Debug.Log(direction.sqrMagnitude);
        if (playerMovement != null)
        {
            playerMovement.UpdateAimingDirection(direction);
        }

        // Instantiate the projectile
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Rotate projectile to face the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Move bullet forward
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }
    }
}

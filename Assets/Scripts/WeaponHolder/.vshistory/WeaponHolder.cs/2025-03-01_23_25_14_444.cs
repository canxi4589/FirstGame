using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    private SpriteRenderer weaponRenderer;

    public Sprite defaultWeapon; // Assign in Inspector
    public Sprite newWeapon;     // Assign in Inspector (New weapon sprite)
    public GameObject projectilePrefab; // Projectile prefab
    public Transform firePoint; // Where the projectile spawns
    public float projectileSpeed = 5f;
    private Animator animator;

    void Start()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (defaultWeapon != null)
            weaponRenderer.sprite = defaultWeapon;
    }

    // Change weapon sprite
    public void ChangeWeapon(Sprite newWeaponSprite)
    {
        if (newWeaponSprite != null)
        {
            weaponRenderer.sprite = newWeaponSprite;
        }
    }

    // Fire projectile function
    public void Fire()
    {
        if (projectilePrefab != null)
        {
            // Play attack animation
            animator.SetTrigger("Attack");

            // Create projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Add force to move the projectile
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = transform.right * projectileSpeed;
            }
        }
    }
}

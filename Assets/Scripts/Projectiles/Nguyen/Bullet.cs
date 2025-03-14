using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 moveDirection;
    [SerializeField] private float moveSpeed = 5f;  // Speed of the bullet (adjustable in Inspector)
    private float fireRange = 10f;                 // Maximum distance the bullet can travel
    private Vector2 startPosition;                 // Starting position of the bullet
    [SerializeField] private int damage = 1;       // Damage dealt to the player (adjustable in Inspector)

    private void OnEnable()
    {
        startPosition = transform.position;        // Record starting position when enabled
    }

    private void Update()
    {
        // Move the bullet
        transform.Translate(moveDirection * moveSpeed * Time.unscaledDeltaTime);

        // Check if the bullet has exceeded its fire range
        if (Vector2.Distance(startPosition, transform.position) > fireRange)
        {
            gameObject.SetActive(false);  // Deactivate instead of destroying
        }
    }

    public void SetMoveDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;  // Ensure direction is normalized
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the bullet hits the player
        if (collision.CompareTag("Player"))
        {
            // Deal damage to the player
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Bullet dealt {damage} damage to player. Player health: {player.GetHealth()}");
            }
            gameObject.SetActive(false);  // Deactivate when hitting the player
        }
        // Also deactivate if hitting Decor (e.g., walls)
        else if (collision.CompareTag("Decor"))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        CancelInvoke();  // Cancel any pending invokes (though none are currently used)
    }

    // Optional: Method to set different speeds for different bullet patterns (if needed)
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
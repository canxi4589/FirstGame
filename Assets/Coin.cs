using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;          // Value of the coin
    [SerializeField] private float moveSpeed = 5f;       // Speed at which the coin moves toward the player
    [SerializeField] private float delayBeforeMove = 1f; // Delay before the coin starts moving

    private Transform player;                            // Reference to the player's transform
    private bool isMoving = false;                       // Track if the coin is moving toward the player
    private float timer = 0f;                            // Timer for the delay

    private void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found for Coin! Destroying coin.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Wait for the delay before starting to move
        if (!isMoving)
        {
            timer += Time.deltaTime;
            if (timer >= delayBeforeMove)
            {
                isMoving = true;
            }
            return;
        }

        // Move toward the player
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // Check if the coin is close enough to the player to be collected
        if (Vector2.Distance(transform.position, player.position) < 0.5f)
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.AddCoins(coinValue); // Add coins to the player
            Destroy(gameObject);
        }
    }
}
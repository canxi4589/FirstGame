using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private int keyValue = 1;          // Number of keys this object represents
    [SerializeField] private float moveSpeed = 5f;      // Speed at which the key moves toward the player
    [SerializeField] private float delayBeforeMove = 1f; // Delay before the key starts moving

    private Transform player;                           // Reference to the player's transform
    private bool isMoving = false;                      // Track if the key is moving toward the player
    private float timer = 0f;                           // Timer for the delay

    private void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found for Key! Destroying key.");
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

        // Check if the key is close enough to the player to be collected
        if (Vector2.Distance(transform.position, player.position) < 0.5f)
        {
            CollectKey();
        }
    }

    private void CollectKey()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Add keys to the player
            playerMovement.AddKeys(keyValue);

            // Show dialog message (e.g., "I found a key!")
            if (DialogManager.Instance != null)
            {
                Sprite playerDialogSprite = playerMovement.GetPlayerDialogSprite();
                string message = keyValue == 1 ? "I found a key!" : $"I found {keyValue} keys!";
                DialogManager.Instance.ShowDialog(message, playerDialogSprite, 2f);
            }

            // Destroy the key object after collection
            Destroy(gameObject);
        }
    }
}
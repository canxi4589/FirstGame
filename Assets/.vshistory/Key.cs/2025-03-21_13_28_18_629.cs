using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private int keyValue = 1; // Number of keys this object represents

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player collided with the key
        if (other.CompareTag("Player"))
        {
            CollectKey(other);
        }
    }

    private void CollectKey(Collider2D playerCollider)
    {
        // Get the PlayerMovement component from the player
        PlayerMovement playerMovement = playerCollider.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Add keys to the player
            playerMovement.AddKeys(keyValue);

            // Show dialog message (e.g., "I found a key!")
            if (DialogManager.Instance != null)
            {
                Sprite playerDialogSprite = playerMovement.GetPlayerDialogSprite(); // Assuming a method to get the sprite
                DialogManager.Instance.ShowDialog("I found a key!", playerDialogSprite, 2f);
            }

            // Destroy the key object after collection
            Destroy(gameObject);
        }
    }
}
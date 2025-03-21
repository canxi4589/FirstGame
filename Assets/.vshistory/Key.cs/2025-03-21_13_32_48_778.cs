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
        PlayerMovement playerMovement = playerCollider.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.AddKeys(keyValue);

            Destroy(gameObject);
        }
    }
}
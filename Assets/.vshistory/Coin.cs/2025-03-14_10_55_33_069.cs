using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1; // Value of the coin

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.AddCoins(coinValue); // Add coins to the player
                Destroy(gameObject); // Destroy the coin after collection
            }
        }
    }
}
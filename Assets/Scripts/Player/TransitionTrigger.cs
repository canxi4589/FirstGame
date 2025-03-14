using Cinemachine;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [SerializeField] private Transform targetPosition; // The position to teleport to
    [SerializeField] private AudioClip transitionSound; // Optional: Specific sound for this transition
    [SerializeField] private PolygonCollider2D newBoundingShape; // The new bounding shape for the camera confiner

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player")) // Ensure "p1" has the "Player" tag
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null && targetPosition != null)
            {
                // Play transition sound (use player's SoundManager or this clip if assigned)
                if (player.GetComponent<SoundManager>() != null && (transitionSound != null || SoundManager.Instance != null))
                {
                    AudioClip soundToPlay = transitionSound != null ? transitionSound : player.GetDefaultTransitionSound();
                    SoundManager.Instance.PlaySound(soundToPlay);
                    Debug.Log("Playing transition sound: " + soundToPlay.name);
                }
                CinemachineConfiner confiner = Camera.main.GetComponentInChildren<CinemachineConfiner>();
                if (confiner != null && newBoundingShape != null)
                {
                    confiner.m_BoundingShape2D = newBoundingShape;
                    confiner.InvalidatePathCache(); // Refresh the confiner to apply the new bounds
                    Debug.Log("Updated camera confiner to new bounding shape: " + newBoundingShape.name);
                }
                else
                {
                    Debug.LogWarning("Cinemachine Confiner or new bounding shape not found!");
                }

                // Teleport the player
                player.TeleportToPosition(targetPosition.position);
            }
            else
            {
                Debug.LogWarning("Player or target position not found!");
            }
        }
    }
}
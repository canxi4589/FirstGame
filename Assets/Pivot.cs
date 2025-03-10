using UnityEngine;

public class Pivot: MonoBehaviour
{
    public Transform player; // Assign the Player in the Inspector
    public float orbitRadius = 1.5f; // Distance from the player
    public float orbitSpeed = 2f; // Speed of rotation

    private float angle = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (player == null) return;

        // Follow the player's position
        transform.position = player.position;

        // Rotate around the player
        angle += orbitSpeed * Time.deltaTime;
        float x = Mathf.Cos(angle) * orbitRadius;
        float y = Mathf.Sin(angle) * orbitRadius;

        // Move the book (Gun) in an orbit
        transform.GetChild(0).localPosition = new Vector3(x, y, 0);

        // Make the book face the cursor
        RotateBookTowardsCursor();
    }

    void RotateBookTowardsCursor()
    {
        if (mainCamera == null) return;

        // Get mouse position in world coordinates
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Get direction from book to cursor
        Vector2 direction = (mousePosition - transform.GetChild(0).position).normalized;

        // Rotate the book (Gun) to face the cursor
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.GetChild(0).rotation = Quaternion.Euler(0, 0, angle);
    }
}

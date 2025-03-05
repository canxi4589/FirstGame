using UnityEngine;

public class FireBullet : MonoBehaviour
{
    [SerializeField]
    private int bulletsAmount = 10;

    [SerializeField]
    private float angleSpread = 30f; // The spread angle for the bullets

    [SerializeField]
    private float fireDistance = 5f; // The specific distance at which to fire

    private Transform player; // Reference to the player's transform

    // Start is called before the first frame update 
    void Start()
    {
        // Find the player by tag (make sure your player GameObject is tagged as "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Start invoking the Fire method repeatedly
        InvokeRepeating("CheckAndFire", 0f, 0.5f); // Check every 0.5 seconds
    }

    private void CheckAndFire()
    {
        if (player == null)
            return;

        // Calculate the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if the player is within the specified fire distance
        if (distanceToPlayer <= fireDistance)
        {
            Fire();
        }
    }

    private void Fire()
    {
        // Calculate the direction to the player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // Calculate the angle to the player
        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        // Adjust the start and end angles based on the angle to the player
        float startAngle = angleToPlayer - angleSpread / 2;
        float endAngle = angleToPlayer + angleSpread / 2;

        float angleStep = (endAngle - startAngle) / bulletsAmount;
        float angle = startAngle;

        for (int i = 0; i < bulletsAmount; i++)
        {
            float bulDirX = transform.position.x + Mathf.Cos((angle * Mathf.PI) / 180f);
            float bulDirY = transform.position.y + Mathf.Sin((angle * Mathf.PI) / 180f);

            Vector3 bulMoveVector = new Vector3(bulDirX, bulDirY, 0f);
            Vector2 bulDir = (bulMoveVector - transform.position).normalized;

            GameObject bul = BulletPool.instance.GetBullet();
            bul.transform.position = transform.position;
            bul.transform.rotation = transform.rotation;
            bul.SetActive(true);
            bul.GetComponent<Bullet>().SetMoveDirection(bulDir);

            angle += angleStep;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueSlimeEnemy : MonoBehaviour
{
    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private int health = 3;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float damageInterval = 1f;     // Time between damage instances

    // Coin Drop variables
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins = 1;
    [SerializeField] private int maxCoins = 3;
    [SerializeField] private float coinDropRadius = 1f;
    [SerializeField] private float contactDamageRadius = 0.5f; // Radius for contact damage

    private enum State
    {
        Roaming
    }

    private State state;
    private EnemyPathfinding enemyPathfinding;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private float damageTimer = 0f;                        // Timer for damage interval

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyPathfinding == null)
        {
            Debug.LogError("EnemyPathfinding component not found on " + gameObject.name);
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
        }
        if (coinPrefab == null)
        {
            Debug.LogWarning("Coin Prefab not assigned in the Inspector for BlueSlimeEnemy!");
        }

        state = State.Roaming;
    }

    private void Start()
    {
        if (enemyPathfinding != null)
        {
            StartCoroutine(RoamingRoutine());
        }
        else
        {
            Debug.LogError("Cannot start RoamingRoutine because enemyPathfinding is null.");
        }
    }

    private void Update()
    {
        if (isDead) return;

        // Check for player contact and deal damage
        CheckPlayerContact();

        // Update damage timer
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private IEnumerator RoamingRoutine()
    {
        while (state == State.Roaming && !isDead)
        {
            Vector2 roamPosition = GetRoamingPosition();
            enemyPathfinding.MoveTo(roamPosition);
            yield return new WaitForSeconds(roamChangeDirFloat);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void CheckPlayerContact()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // Check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerObj.transform.position);

        if (distanceToPlayer <= contactDamageRadius && damageTimer <= 0)
        {
            PlayerMovement player = playerObj.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Slime dealt {damage} damage to player. Player health: {player.GetHealth()}");
                damageTimer = damageInterval; // Reset timer to prevent constant damage
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        isDead = true;
        DropCoins();
        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (coinPrefab == null) return;

        int coinsToDrop = Random.Range(minCoins, maxCoins + 1);
        for (int i = 0; i < coinsToDrop; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * coinDropRadius;
            Vector3 coinPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            Instantiate(coinPrefab, coinPosition, Quaternion.identity);
        }
    }

    // Optional: Visualize the damage radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, contactDamageRadius);
    }
}
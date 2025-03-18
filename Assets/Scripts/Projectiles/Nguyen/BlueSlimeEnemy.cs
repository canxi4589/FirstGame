using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueSlimeEnemy : MonoBehaviour
{
    [SerializeField] private float roamChangeDirFloat = 2f;

    private enum State
    {
        Roaming
    }

    private State state;
    private EnemyPathfinding enemyPathfinding;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        if (enemyPathfinding == null)
        {
            Debug.LogError("EnemyPathfinding component not found on " + gameObject.name);
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

    private IEnumerator RoamingRoutine()
    {
        while (state == State.Roaming)
        {
            Vector2 roamPosition = GetRoamingPosition();
            enemyPathfinding.MoveTo(roamPosition); // Line 33 - error occurs here if enemyPathfinding is null
            yield return new WaitForSeconds(roamChangeDirFloat);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            return;
        }
        if (collision.CompareTag("Enemy"))
        {
            return;
        }
    }
}
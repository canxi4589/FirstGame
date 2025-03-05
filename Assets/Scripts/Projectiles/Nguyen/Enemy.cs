using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float lineOfSite;
    public float shootingRange;
    public float fireRate = 1f;
    public float nextfireTime;
    public GameObject bullet;
    public GameObject bulletParent;
    private Transform player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
    }

    // Update is called once per frame
    void Update()
    {
        float disFromPlayer = Vector2.Distance(player.position, transform.position);
        if (disFromPlayer > shootingRange && disFromPlayer < lineOfSite)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, player.position, speed * Time.deltaTime);
        }
        else if(disFromPlayer <= shootingRange && nextfireTime < Time.time){
            Instantiate(bullet, bulletParent.transform.position, Quaternion.identity);
            nextfireTime = Time.time + fireRate;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawWireMesh(transform.position lineOfSite);
        //Gizmos.DrawWireMesh(transform.position, shootingRange);
    }
}

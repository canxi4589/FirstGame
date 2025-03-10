using System.Collections.Generic;
using UnityEngine;

public class BulletPool1 : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int poolSize = 50;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    void Start()
    {
        // Pre-instantiate bullets
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet1()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        else
        {
            GameObject bullet = Instantiate(bulletPrefab);
            return bullet;
        }
    }

    public void ReturnBullet1(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}

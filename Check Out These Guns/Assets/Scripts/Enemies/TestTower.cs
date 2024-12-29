using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class testTower : MonoBehaviour
{
    public GameObject bullet;
    public float initShootCD = 2f;
    public float shootCooldown = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shootCooldown -= Time.deltaTime;
        if (shootCooldown < 0)
        {
            Shoot();
            shootCooldown = initShootCD;
        }
    }

    void Shoot()
    {
        Instantiate(bullet, transform.position, transform.rotation);
    }
}

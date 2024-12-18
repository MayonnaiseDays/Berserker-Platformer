using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadGeneric : MonoBehaviour
{
    //set this by the artillery parent later
    public payloadTypes payloadType = payloadTypes.Cannonball;
    public Transform groundSensor; // Assign in the inspector or dynamically
    public LayerMask groundLayer;
    public GameObject explosionPrefab; // Prefab for explosion effect
    public GameObject enemyPrefab; // Prefab for enemy to spawn
    public List<GameObject> multipleEnemiesPrefabs; // Prefabs for multiple enemies to spawn
    public float explosionRadius = 2f;
    public int Damage = 10;
    public bool activated;

    // Start is called before the first frame update
    void Start()
    {
        activated = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if Grounded
        if (Physics2D.OverlapCircle(groundSensor.position, 0.1f, groundLayer))
        {   
            //maybe spawn a dust cloud ot something?
            Activate();
        } 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //maybe remove the if grounded check from above and instead have a specific ground near the player to check for collisions with
        //so something like the payload colliding on a ground above the player doesnt happen
        if (collision.gameObject.CompareTag("Player"))
        {
            Activate();
        }
    }

    void OnBecameInvisible()
    {
        transform.parent.GetComponent<testArtillery>().payloadOffsceen();
        Destroy(gameObject);
    }

    void Activate()
    {
        activated = true;
        switch (payloadType)
        {
            case payloadTypes.Cannonball:
                //do damage to any touching players
                break;
            case payloadTypes.Explosive:
                //explode
                break;
            case payloadTypes.SingleEnemy:
                //spoawn the enemy
                break;
            case payloadTypes.MultEnemies:
                //monkey
                break;
        }
    }
}


public enum payloadTypes
{
    Cannonball,
    Explosive,
    SingleEnemy,
    MultEnemies
}

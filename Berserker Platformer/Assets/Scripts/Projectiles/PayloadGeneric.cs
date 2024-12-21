using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadGeneric : MonoBehaviour
{
    //set this by the artillery parent later
    public payloadTypes payloadType = payloadTypes.Cannonball;
    //public Transform groundSensor; // Assign in the inspector or dynamically
    public LayerMask groundLayer;
    public GameObject explosionPrefab; // Prefab for explosion effect
    public GameObject enemyPrefab; // Prefab for enemy to spawn
    public List<GameObject> multipleEnemiesPrefabs; // Prefabs for multiple enemies to spawn
    public float explosionRadius = 2f;
    public int Damage = 10;
    public bool activated;

    // Time to move from sunrise to sunset position, in seconds.
    public float journeyTime = 2.0f;
    // The time at which the animation started.
    private float startTime;

    public Transform cannonPos;
    //vector bc transform makes it follow the player
    public Vector3 landingPos;

    public float fracComplete;

    // Start is called before the first frame update
    void Start()
    {
        activated = false;
        startTime = Time.time;
        //cannonPos = GameObject.Find("Artillery").transform;
        landingPos = GameObject.Find("Player").transform.position;

        Destroy(gameObject, 4);
    }

    // Update is called once per frame
    void Update()
    {
        SlerpMovement(cannonPos, landingPos);
        
        if (fracComplete == 1)
        {   
            //maybe spawn a dust cloud ot something?
            Activate();
            Destroy(gameObject);
        } 
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        //maybe remove the if grounded check from above and instead have a specific ground near the player to check for collisions with
        //so something like the payload colliding on a ground above the player doesnt happen
        if (collision.gameObject.CompareTag("Player"))
        {
            Activate();
        }
    }*/

    /*void OnBecameInvisible()
    {
        transform.parent.GetComponent<testArtillery>().payloadOffsceen();
        Destroy(gameObject);
    }*/

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

    void SlerpMovement(Transform startPos, Vector3 endPos)
    {
        //startPos = Transform
        //endPos = Vector3

        // The center of the arc
        Vector3 center = (startPos.position + endPos) * 0.5f;

        // move the center a bit downwards to make the arc vertical
        center -= new Vector3(0, 50, 0);

        // Interpolate over the arc relative to center
        Vector3 riseRelCenter = startPos.position - center;
        Vector3 setRelCenter = endPos - center;

        // The fraction of the animation that has happened so far is
        // equal to the elapsed time divided by the desired time for
        // the total journey.
        fracComplete = (Time.time - startTime) / journeyTime;

        transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
        transform.position += center;
    }

}


public enum payloadTypes
{
    Cannonball,
    Explosive,
    SingleEnemy,
    MultEnemies
}

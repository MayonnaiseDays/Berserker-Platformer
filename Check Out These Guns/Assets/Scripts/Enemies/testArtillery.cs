using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class testArtillery : MonoBehaviour
{

    public GameObject[] payloadPrefabs; // Array to store different payload types
    public Transform firePoint; // Point where the payload is fired from
    public float fireForce;
    public float initShootCD = 5f;
    public float shootCooldown;

    // Start is called before the first frame update
    void Start()
    {
        fireForce = 20f;
        shootCooldown = 4f;
    }

    // Update is called once per frame
    void Update()
    {
        shootCooldown -= Time.deltaTime;
        if (shootCooldown < 0)
        {
            Launch();
            shootCooldown = initShootCD;
        }
        
    }


    //maybe have the first shot of each artillery shoot in front so that a speedrun becomes cooler
    void Launch()
    {
        if (payloadPrefabs.Length < 0) 
            //NoMoreAmmo();
            return;
        GameObject payload = Instantiate(payloadPrefabs[0], firePoint.position, Quaternion.identity);
        payload.transform.GetComponent<PayloadGeneric>().cannonPos = firePoint;
        //Rigidbody2D rb = payload.GetComponent<Rigidbody2D>();
        //rb.AddForce(Vector2.up * fireForce, ForceMode2D.Impulse); // Fire up

    }

    void NoMoreAmmo()
    {
        //maybe play silly anim
        //maybe detroy self after
    }

    /*public void payloadOffsceen()
    {
        //wait some time then respawn it in foreground
        StartCoroutine(RespawnPayloadAfterDelay(2f));
        GameObject payload = Instantiate(payloadPrefabs[0], firePoint.position, Quaternion.identity, gameObject.transform);
    }
    IEnumerator RespawnPayloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified time

        // Spawn the new payload
        GameObject payload = Instantiate(payloadPrefabs[0], firePoint.position, Quaternion.identity, gameObject.transform);
        Rigidbody2D rb = payload.GetComponent<Rigidbody2D>();
        payload.layer = LayerMask.NameToLayer("Background");

        if (rb != null)
        {
            rb.AddForce(Vector2.up * fireForce, ForceMode2D.Impulse); // Fire up the payload
        }
    }*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGeneric : MonoBehaviour
{
    public float launchForce = 10f; // Base launch force
    public Vector2 launchDirection = Vector2.right; // Direction the projectile will be launched in
    public float mass = 1f;
    public float size = 1f;
    private Rigidbody2D rb;

    public bool belongsToPlayer = false; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Adjust the Rigidbody2D mass based on the projectile's mass property
        rb.mass = mass;

        // Adjust the size of the projectile sprite based on the size factor
        transform.localScale = new Vector3(size, size, 1);

        // Apply force to launch the projectile
        LaunchProjectile();
    }

    // Method to apply the launch force to the projectile
    void LaunchProjectile()
    {
        // Add force in the direction the projectile should move
        rb.AddForce(launchDirection.normalized * launchForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with the player or enemies
        if (collision.gameObject.CompareTag("Player") && !belongsToPlayer)
        {
            Debug.Log("about to touchy");
            // If projectile is shot by enemy and hits the player
            HandlePlayerHit(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy") && belongsToPlayer)
        {
            // If projectile is shot by the player and hits an enemy
            HandleEnemyHit(collision.gameObject);
        }
        else 
        {
            // If projectile hits something else, just destroy it (or add other logic)
            //Destroy(gameObject);
        }
    }

    void HandlePlayerHit(GameObject player)
    {
        Debug.Log("touchy");
        PlayerBehaviorTest playerScript = player.GetComponent<PlayerBehaviorTest>();
        if (playerScript.currState == PlayerBehaviorTest.playerState.isAbsorbing)
        {
            Debug.Log("absorbattempt");
            playerScript.numOfProjectiles += 1;
            player.transform.localScale += new Vector3(Mathf.Sign(player.transform.localScale.x) * size / 5f, size / 5f, 0);
            Destroy(gameObject);
        }
        // Here, you can apply damage or effects to the player
        Debug.Log("Player hit by enemy projectile!");

        // Optionally, destroy the projectile when it hits the player
        //Destroy(gameObject);

        // Apply damage or effects to the player
        // player.GetComponent<PlayerHealth>().TakeDamage(10);
    }

    void HandleEnemyHit(GameObject enemy)
    {
        // Here, you can apply damage or effects to the enemy
        Debug.Log("Enemy hit by player projectile!");

        // Optionally, destroy the projectile when it hits the enemy
        Destroy(gameObject);

        // Apply damage or effects to the enemy
        // enemy.GetComponent<EnemyHealth>().TakeDamage(10);
    }
}

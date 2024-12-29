using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericEnemy : MonoBehaviour
{
    [Header("Movement")]
    private float speed = 2f;
    private bool isFacingRight = false;
    private float flipTimer = 0f;


    [Header("Attack")]
    private float atkDamage;
    private float atkRange;
    private float atkCooldown;
    private bool isAttacking;
    private float atkTimer;

    
    [Header("Player Detection")]
    public Transform player;
    public LayerMask playerLayer;
    
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [SerializeField] private Rigidbody2D rb;
    private Animator anim;

    //grounded stuff
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    public bool isGrabbed;

    // Start is called before the first frame update
    void Start()
    {
        //set variables
        //rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        float gravityScale = 3f;
        isGrabbed = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbed)
            return;
        //move and attack and maybe decision
        Move();
        if (PlayerInRange())
        {
            Attack();
        }

        atkTimer += Time.deltaTime;
        flipTimer += Time.deltaTime;

        //check health and die if needed
    }

    bool PlayerInRange()
    {
        return Vector2.Distance(transform.position, player.position) <= atkRange;
    }
    
    private void Move()
    {   
        if (isAttacking)
            return;
        
        // Check if the enemy is about to fall off the edge using a raycast
        if (!IsGroundAhead())
        {
            if (flipTimer >= 0f)
                Flip();
        }

        float direction = isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        

        //something like if (thing ahead) is a wall
        /*if (transform.position.x + (direction * 10))
        {
            Console.Log("hit a wall");
            //need to flip
        }*/


        //flip sprite based on direction
        //transform.localScale = new Vector3(direction, 1, 1);

        //is this how to animate?
        //anim.SetBool("isMoving", true);
    }


    private bool IsGroundAhead()
    {
        // Raycast downwards from the groundCheck position
        RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.red);

        // Return true if there's ground ahead, false otherwise
        return groundInfo.collider != null;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        flipTimer = -1f;
    }

    private void Attack()
    {
        isAttacking = true;
        //attack
        isAttacking = false;
    }
}

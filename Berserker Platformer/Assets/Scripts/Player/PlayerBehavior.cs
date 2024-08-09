using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{

    private float horizontal;
    //maybe have accelertaion instead?
    private float speedwalk = 8f;
    private float speeddash = 20f;
    private float jumpPower = 16f;
    private bool isFacingRight = true;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundSensor;
    [SerializeField] private LayerMask groundlayer;

 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //gets the left right inputs from both a d and arrow keys
        horizontal = Input.GetAxisRaw("Horizontal");

        //when space is pressed and on floor
        if (Input.GetKeyDown("space") && IsGrounded())
        {
            //go up
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        }

        //when space is released
        if (Input.GetKeyUp("space") && rb.velocity.y > 0f)
        {
            //stop going up? if the .6f is 1, you go up as if you held space
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * .6f);
        }

        //on right click
        if (Input.GetKeyDown("mouse 1"))
        {   
            Debug.Log("pickle");
            rb.AddForce(transform.forward * speeddash, ForceMode2D.Impulse);
        }

        Flip();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speedwalk, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundSensor.position, 0.2f, groundlayer);
    }

    private void Flip()
    {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}

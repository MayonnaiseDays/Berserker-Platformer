using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerBehaviorTest : MonoBehaviour
{
    #region Variables
    float moveInput;
    float moveSpeed = 8f;
    float acceleration = 6f;
    float deceleration = 3f;
    float velPower = 1.15f;
    float frictionAmount = .2f;
    float jumpForce = 16f;
    float jumpCutMultiplier = .4f;

    //amount of time you can fall off a ledge and still jump
    float jumpCoyoteTime = .2f;
    //amount of time to buffer a jump
    float jumpBufferTime = .1f;
    float fallGravityMultiplier = 1.5f;
    float lastGroundedTime;
    float lastJumpTime;
    bool isJumping;
    bool jumpInputReleased;
    
    
    
    //gotta look into this more
    float gravityScale = 3f;

    [SerializeField] private Transform groundSensor;
    [SerializeField] private LayerMask groundlayer;

    #endregion


    [SerializeField] private Rigidbody2D rb;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        moveInput = Input.GetAxisRaw("Horizontal");

        //if Grounded
        if (Physics2D.OverlapCircle(groundSensor.position, 0f, groundlayer) && Mathf.Abs(rb.velocity.y) < 0.1f)
        {   
            
            //reset the coyote timer
            lastGroundedTime = jumpCoyoteTime;
            isJumping = false; 
        }  
        
        //coyote timers
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;

        #region Jump

        if(Input.GetKeyDown("space"))
        {
            lastJumpTime = jumpBufferTime;
        }

        //when space is pressed and its been within (var) seconds since you left the floor
        //and its been within (var) seconds since last jumped(this is more for double jumps or possible jump buffers)
        //and the reason you are midair is not because you pressed jump

        if (lastGroundedTime > 0f && lastJumpTime > 0f && !isJumping)
        {
            //go up
            Jump();
        }
        if (Input.GetKeyUp("space"))
            onJumpRelease();
        
        #endregion 

        //fast fall like fox in smash
        #region Jump Gravity
        //if youre falling
        if (rb.velocity.y < 0)
        {
            //fall faster
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            //otherwise(when jumping), go up normally
            rb.gravityScale = gravityScale;
        }
        #endregion
    }

    private void FixedUpdate()
    {
        #region Run

        //calculate directio nand velocity
        float targetSpeed = moveInput * moveSpeed;

        //calc difference between current velocity and desired velocity
        //basically applies less force when already going in desired direction and more force when going in opposite
        float speedDif = targetSpeed - rb.velocity.x;

        //change acceleration rate depending on situation
        float accelRate = (Mathf.Abs(targetSpeed) > .01f) ? acceleration : deceleration;

        //applies acceleration to speed difference, the raises to a set power so acceleration increases with higher speeds
        //then mutiplies by sign to reapply direction
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        //applies force to rigidbody, multiplying by vector2.right so that it only affects x axis
        rb.AddForce(movement * Vector2.right);

        #endregion

        #region Friction       COMMENTED OUT CAUSE I DONT GET INPUTHANDLER

        //check if grounded and no keys pressed
        if (lastGroundedTime > 0 && Mathf.Abs(moveInput) < .01f)
        {
            //thne use either friction amount or velocity
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            //sets to movement direction
            amount *= Mathf.Sign(rb.velocity.x);
            //applies force against movement direction
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        #endregion Friction
        
    }

    private void Jump()
    {
        //apply impulse force
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); 
        lastGroundedTime = 0;
        lastJumpTime = 0;
        isJumping = true;
        jumpInputReleased = false;
    
    }

    public void onJumpRelease()
    {
        //if you let go of space and your jump is going up and the reason youre going up is because you jumped
        if (rb.velocity.y > 0f && isJumping)
        {
            
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }

        jumpInputReleased = true;
        lastJumpTime = 0;
    }
    

}

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
    //bool jumpInputReleased;
    
    
    
    //gotta look into this more
    float gravityScale = 3f;

    [SerializeField] private Transform groundSensor;
    [SerializeField] private LayerMask groundlayer;

    bool canDash = true;
    bool isDashing;
    bool isUpDashing;
    float originalGravity;
    float dashingPower = 24f;
    float dashingTime = .3f;
    float dashingCooldown = .1f;
    //pressed updash too early timer
    float lastUpDashTime;
    float upDashBufferTime = .1f;
    float stillCanUpDashTime;
    float upDashCoyoteTime = .2f;

    [SerializeField] TrailRenderer tr;
    [SerializeField] private Rigidbody2D rb;
    private bool isFacingRight = true;



    #endregion


    
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        #region DashInput
        //if you press w even before dashing, allow up dash anyways
        
        
        if (Input.GetKey("w"))
        {
            lastUpDashTime = upDashBufferTime;
        }
        /*if (lastUpDashTime > 0f && canDash)
        {
            //add code
            StartCoroutine(UpDash());
        }*/
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            if (lastUpDashTime > 0f)
            {
                StartCoroutine(UpDash());
            }
            else
            {
                StartCoroutine(Dash());
                stillCanUpDashTime = upDashCoyoteTime;
            }
        }
        if (isDashing || isUpDashing)
        {
            lastUpDashTime -= Time.deltaTime;
            stillCanUpDashTime -= Time.deltaTime;
            //basically dont take most of the inputs
            //maybe have a coyote time for dashing to decode if its an up dash
            if (stillCanUpDashTime > 0f && Input.GetKeyDown("w"))
            {
                StartCoroutine(UpDash());
            }



            return;

        }
        #endregion

        moveInput = Input.GetAxisRaw("Horizontal");

        //if Grounded
        if (Physics2D.OverlapCircle(groundSensor.position, 0f, groundlayer) && Mathf.Abs(rb.velocity.y) < 0.1f)
        {   
            
            //restart the coyote timer
            lastGroundedTime = jumpCoyoteTime;
            isJumping = false; 
        }  
        
        //coyote timers
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        stillCanUpDashTime -= Time.deltaTime;
        lastUpDashTime -= Time.deltaTime;
        

        #region Jump Input

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
        #region Fast Fall
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
        //face left/right
        Flip();
    }

    private void FixedUpdate()
    {
        //Attention! may need to attach
        if(isDashing || isUpDashing)
            return;
        //if theres anything happening thats not supposed to during dashing
        #region Actual Movement

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

        #region Friction 

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
        //jumpInputReleased = false;
    
    }

    public void onJumpRelease()
    {
        //if you let go of space and your jump is going up and the reason youre going up is because you jumped
        if (rb.velocity.y > 0f && isJumping)
        {
            
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }

        //jumpInputReleased = true;
        lastJumpTime = 0;
    }
    
    #region Dash
    public IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        //maybe change this to addforce impulse
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    } 

    public IEnumerator UpDash()
    {
        canDash = false;
        isUpDashing = true;
        originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        //30 60 90 triangle if hypothenuse is 1
        rb.velocity = new Vector2(transform.localScale.x * dashingPower * .865f, transform.localScale.y * dashingPower * .5f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isUpDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
    #endregion
    private void Flip()
    {
        if(isFacingRight && moveInput < 0f || !isFacingRight && moveInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}

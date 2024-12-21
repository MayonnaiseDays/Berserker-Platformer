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

    bool isGrabbing;
    bool isThrowing;
    GameObject grabbedEnemy;
    float throwForce = 20f;
    
    public bool isAbsorbing;
    public float numOfProjectiles;
    float tempAbsorbTimer;
    
    
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

    float throwDuration;
    float currThrowDuration;

    #endregion


    
    

    // Start is called before the first frame update
    void Start()
    {
        isJumping = false;
        isDashing = false;
        isGrabbing = false;
        isThrowing = false;
        isAbsorbing = false;
        numOfProjectiles = 0;
        tempAbsorbTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        #region Grab Input
        if (isDashing && Input.GetKeyDown("e"))
        {
            isGrabbing = true;
        }

        //commenting out cause if im grabbing im also dashing and dash already has this
        /*if (isGrabbing || isThrowing)
        {
            //disable the other stuff like inputs during grabs
            return;
        }*/
        #endregion


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

        #region Absorb
        tempAbsorbTimer -= Time.deltaTime;
        if (Input.GetKeyDown("s"))
        {
            StartAbsorb();
            Debug.Log(tempAbsorbTimer);
            
        }
        if (tempAbsorbTimer < 0f)
        {
           //Debug.Log(tempAbsorbTimer);
            EndAbsorb();
        }
        /*if (isAbsorbing && Physics2D.OverlapCircle(transform.position, 0f, Projectiles))
        {
            numOfProjectiles += 1;
        }*/
        #endregion
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
        rb.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower, 0f);
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
        rb.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower * .865f, Mathf.Sign(transform.localScale.y) * dashingPower * .5f);
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

    #region Grabs + Throws
    void OnCollisionEnter2D(Collision2D collision)
    {
        //probably need to add isGrabbable on enemies
        if (isGrabbing && collision.gameObject.CompareTag("Enemy"))
        {
            // If enemy is valid for grabbing
            grabbedEnemy = collision.gameObject;
            
            StartCoroutine(ThrowArc());
        }
    }
    /*void StartThrow()
    {

        
        //apply force
        rb.velocity = Vector2.down * throwForce;
        
        // Simultaneously apply force to the enemy
        Rigidbody2D enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.velocity = Vector2.up * throwForce;
        }

        // End throw after a short delay
        StartCoroutine(EndThrow());
    }*/

    IEnumerator ThrowArc()
    {
        //it would be really funny to grab multiple enemies and toss them all at once
        //like katamari
        isDashing = false;
        isGrabbing = false;
        isThrowing = true;

        throwDuration = 2f;
        currThrowDuration = Time.time;

        //maybe make a game object as the target and be able to move it left or right so the player can control where they land
        SlerpMovement(gameObject.transform.position, gameObject.transform.position + new Vector3(5 * (isFacingRight ? 1 : -1), 0, 0), throwDuration, currThrowDuration);

        //to kinda have landing effects, maybe not needed
        //StartCoroutine(EndThrow());
        isThrowing = false;
        
        // Reset enemy and player state
        grabbedEnemy = null;

        yield return null;
    }

    void SlerpMovement(Vector3 startPos, Vector3 endPos, float journeyTime, float startTime)
    {
        //startPos = Vector3
        //endPos = Vector3
        float fracComplete;

        // The center of the arc
        Vector3 center = (startPos + endPos) * 0.5f;

        // move the center a bit downwards to make the arc vertical
        center -= new Vector3(0, 5, 0);

        // Interpolate over the arc relative to center
        Vector3 riseRelCenter = startPos - center;
        Vector3 setRelCenter = endPos - center;

        // The fraction of the animation that has happened so far is
        // equal to the elapsed time divided by the desired time for
        // the total journey.
        fracComplete = (Time.time - startTime) / journeyTime;

        transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
        transform.position += center;
    }

    IEnumerator EndThrow()
    {
        yield return new WaitForSeconds(0.5f);
        //maybe have like a slowed down effect or ground break effect
    }
    #endregion

    void StartAbsorb()
    {
        isAbsorbing = true;
        tempAbsorbTimer = 20f;
    }

    void EndAbsorb()
    {
        isAbsorbing = false;
    }

}
/*
#region Projectiles
void ShootProjectiles()
{

}

#endregion*/
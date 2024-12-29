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
   
    //bool jumpInputReleased;

    //bool isGrabbing;
    //bool isThrowing;
    GameObject grabbedEnemy;
    float throwForce = 20f;
    
    //public bool isAbsorbing;
    public float numOfProjectiles;
    float tempAbsorbTimer;
    
    
    //gotta look into this more
    float gravityScale = 3f;

    [SerializeField] private Transform groundSensor;
    [SerializeField] private LayerMask groundlayer;

    bool canDash = true;
    //bool isDashing;
    //bool isUpDashing;
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
    float size = 1f;

    //this could maybe be the exact gameobject absorb rather than a generic projectile
    public GameObject projectilePrefab;
    float launchForce = 30f;

    public playerState currState;
    #endregion

    bool isJumping;
    bool isIdle;
    bool isUpDashing;
    bool IsDashGrab;
    bool isCargoEnemy;
    public enum playerState
    {
        isMoving, //maybe split to isWalking and isRunning
        //took out isjumping because dash would reset jump which made dash jump dash jump possible
        //isJumping,
        isAttacking,
        isDashing,
        //isUpDashing,
        isDashGrabbing,
        isGrabbing,
        isThrowing,
        isAbsorbing,
        isReleasing
    }
    

    // Start is called before the first frame update
    void Start()
    {
        currState = playerState.isMoving;
        numOfProjectiles = 0;
        tempAbsorbTimer = 0;
        //IsDashGrab = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Current State: " + currState);

        //currently only used for release but maytbe could have something like strike force heroes 2
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        Vector3 mouseDirection = (mousePosition - transform.position).normalized;
        mousePosition.z = 0f;
        //first for normalize calcs, second cause it still had nonzero z's
        //Debug.Log($"Mouse Direction: {mouseDirection}");


        #region Grab Input
        if (Input.GetKeyDown("e"))
        {
            Debug.Log("input detect e");
            if (currState == playerState.isDashing)
            {
                Debug.Log("dash to dasg gerab");
                currState = playerState.isDashGrabbing;
            }
            else
            {
                Debug.Log("nondash to grab");
                currState = playerState.isGrabbing;
            }
            Debug.Log("Im grabbing");
            //include grab whiffs
        }

        #endregion

        if (Input.GetMouseButtonDown(1) && (numOfProjectiles > 0 || isCargoEnemy))
        {
            currState = playerState.isReleasing;

            ShootProjectiles(mouseDirection);
            currState = playerState.isMoving;

        }


        #region DashInput


        //if you press w even before dashing, allow up dash anyways
        if (Input.GetKey("w"))
        {
            lastUpDashTime = upDashBufferTime;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            if (lastUpDashTime > 0f)
            {
                StartCoroutine(UpDash());
            }
            else
            {
                isUpDashing = false;
                StartCoroutine(Dash());
                stillCanUpDashTime = upDashCoyoteTime;
            }
        }
        if (currState == playerState.isDashing)
        {
            lastUpDashTime -= Time.deltaTime;
            stillCanUpDashTime -= Time.deltaTime;
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
        if(!(currState == playerState.isMoving))
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
        isJumping = false;
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
    //lets try changing this to a 
    public IEnumerator Dash()
    {
        canDash = false;
        currState = playerState.isDashing;
        originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        //maybe change this to addforce impulse
        rb.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        currState = playerState.isMoving;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        Debug.Log("reg dash end");
    } 

    public IEnumerator UpDash()
    {
        canDash = false;
        currState = playerState.isDashing;
        isUpDashing = true;
        originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        //30 60 90 triangle if hypothenuse is 1
        rb.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * dashingPower * .865f, Mathf.Sign(transform.localScale.y) * dashingPower * .5f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        currState = playerState.isMoving;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        Debug.Log("up dash end");
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
        Debug.Log("playerstate: " + currState.ToString());
        if (currState == playerState.isGrabbing && collision.gameObject.CompareTag("Enemy"))
        {
            //if iscargoenemy = false;
            Debug.Log(" about to grab");
            // If enemy is valid for grabbing
            grabbedEnemy = collision.gameObject;
            //grabbedEnemy.GetComponent<BoxCollider2D>().enabled = false;
            
            TESTCargoGrabEnemy(grabbedEnemy);
        }
    }

    void TESTCargoGrabEnemy(GameObject grabbedEnemy)
    {
        Rigidbody2D enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.velocity = Vector2.zero; // Stop the enemy's motion
            enemyRb.isKinematic = true; // Disable physics temporarily
        }

        // Attach the enemy to the player (parent it)
        grabbedEnemy.transform.SetParent(transform);
        grabbedEnemy.transform.localPosition = new Vector3(0, 1, 0);

        isCargoEnemy = true;
    }

    void ThrowArc()
    {
        Debug.Log("throwing start");
        //it would be really funny to grab multiple enemies and toss them all at once
        //like katamari
        currState = playerState.isThrowing;

        throwDuration = 2f;
        currThrowDuration = Time.time;

        //maybe make a game object as the target and be able to move it left or right so the player can control where they land
        SlerpMovement(gameObject.transform.position, gameObject.transform.position + new Vector3(5 * (isFacingRight ? 1 : -1), 0, 0), throwDuration, currThrowDuration);

        //to kinda have landing effects, maybe not needed
        //StartCoroutine(EndThrow());
        currState = playerState.isMoving;
        
        // Reset enemy and player state
        grabbedEnemy = null;

    }

    void SlerpMovement(Vector3 startPos, Vector3 endPos, float journeyTime, float startTime)
    {
        //startPos = Vector3
        //endPos = Vector3
        float fracComplete;
        Debug.Log("start + end: " + startPos + " " + endPos);

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
        currState = playerState.isAbsorbing;
        tempAbsorbTimer = 1f;
    }

    void EndAbsorb()
    {
        currState = playerState.isMoving;
    }

    #region Projectiles
    void ShootProjectiles(Vector3 mouseDirection)
    {
        numOfProjectiles -= 1;
        
        GameObject projectile = Instantiate(projectilePrefab, mouseDirection + transform.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.AddForce(mouseDirection * launchForce, ForceMode2D.Impulse);
        Rigidbody2D prb = GetComponent<Rigidbody2D>();
        prb.AddForce(mouseDirection * launchForce * -1, ForceMode2D.Impulse);
        Debug.Log("projectile direction" + mouseDirection * launchForce);
        Debug.Log("velocity  + magnitude: " + rb.velocity + "  " + rb.velocity.magnitude);
        Debug.Log("player velo:" + prb.velocity);
        transform.localScale -= new Vector3(Mathf.Sign(transform.localScale.x) * size / 5f, size / 5f, 0);
    }

    void ShootEnemy(Vector3 mouseDirection)
    {
        if (grabbedEnemy == null) 
            return;

        // Detach the enemy from the player
        grabbedEnemy.transform.SetParent(null);

        // Enable physics on the enemy
        Rigidbody2D enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.isKinematic = false;
            enemyRb.AddForce(mouseDirection * launchForce, ForceMode2D.Impulse); // Apply force
        }

        // Clear the grabbed enemy reference
        grabbedEnemy = null;
    }

    #endregion

}


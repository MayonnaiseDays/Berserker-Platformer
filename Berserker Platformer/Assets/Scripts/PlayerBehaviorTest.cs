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
    float lastGroundedTime;
    float lastJumpTime;
    bool isJumping;
    bool jumpInputReleased;
    #endregion Variables


    [SerializeField] private Rigidbody2D rb;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
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

       /* #region Friction

        //check if grounded and no keys pressed
        if (lastGroundedTime > 0 && Mathf.Abs(InputHandler.instance.MoveInput) < .01f)
        {
            //thne use either friction amount or velocity
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            //sets to movement direction
            amount *= Mathf.Sign(rb.velocity.x);
            //applies force against movement direction
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        #endregion Friction*/
        
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

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundSensor.position, 0.2f, groundlayer);
    }
}

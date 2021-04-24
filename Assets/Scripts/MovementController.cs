using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float speed;
    public float maxJumpHeight;
    public float minJumpHeight;
    public float jumpTime;
    public float timeToReachSpeed;
    public float maxJerk = 2;
    
    private float maxGravity;
    private float minGravity;
    private float jumpSpeed;

    public MovementActions controls;
    private RayMover mover;
    private Vector2 movementDampVelocity;

    private float jumpPressTime = Mathf.NegativeInfinity;
    public float jumpWindow = 0.01f;
    public float coyoteTime = 0.01f;

    private bool hasSetupControls = false;
    void SetupControls()
    {
        if (!hasSetupControls)
        {
            hasSetupControls = true;
            controls.Enable();
            controls.Gameplay.Jump.performed += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                jumpPressTime = Time.time;
                print("Jumping");
            };
        }
    }

    void Start()
    {
        mover = GetComponent<RayMover>();
        mover.CheckForSupport(Vector2.down);
        controls = new MovementActions();
        

        // 0.5 a t^2 + b t   = h
        //     a t   + b     = 0
        //             b     = -a t
        // 0.5 a t^2 - a t^2 = h
        // -0.5 a t^2 = h => a = -0.5 h / t^2
        minGravity = 0.5f * maxJumpHeight / (jumpTime * jumpTime);
        jumpSpeed = minGravity * jumpTime;
        // 0.5 c t^2 + b t   = h
        //     c t   + b     = 0
        //     c             = -b / t
        //-0.5 b t   + b t   = h
        // 0.5 b t           = h
        //               t   = h / (0.5 b) 
        float minJumpTime = minJumpHeight / (0.5f * jumpSpeed);
        maxGravity = jumpSpeed / minJumpTime;
    }

    // Called on a fixed time step.
    void FixedUpdate()
    {
        SetupControls();
        if (!mover.Sliding())
        {
            Vector2 desiredMovement = controls.Gameplay.Move.ReadValue<Vector2>();
            Vector2 perpVelocity = mover.PerpendicularVelocity(Vector2.down);
            Vector2 desiredPerpVelocity = Vector2.SmoothDamp(perpVelocity, -mover.Left(Vector2.down) * speed * desiredMovement.x, ref movementDampVelocity, timeToReachSpeed, maxJerk);
            mover.velocity += desiredPerpVelocity - perpVelocity;
            if (mover.TimeInAir() < coyoteTime)
            {
                if (Time.time - jumpPressTime < jumpWindow)
                {
                    mover.velocity += Vector2.up * jumpSpeed;
                    jumpPressTime = Mathf.NegativeInfinity; // We handled the jump input
                }
            }
        }
        mover.Move(Time.fixedDeltaTime, Vector2.down * maxGravity);
    }
}

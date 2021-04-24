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

    public float maxEnergy = 10.0f;
    public float currentEnergy = 0.0f;

    /// How much should we decrease the draw on us while
    /// we are against a wall and falling.
    public float wallFriction = 0.5f;
    /// When actively digging into a wall, how quickly
    /// should we decrease our velocity each second.
    public float wallSlideSlowdownPerSecond = 0.5f;
    public float wallSlideExhaustion = 1.0f;
    /// Actual per-tick slowdown.
    private float wallSlidePerTickSlowdown = 0.0f;
    private bool attachedToWall;
    
    public float maxGravity;
    public float minGravity;
    public float jumpSpeed;

    public float activeGravity;

    public MovementActions controls;
    private RayMover mover;
    private Vector2 movementDampVelocity;

    private float jumpPressTime = Mathf.NegativeInfinity;
    public int jumpBufferFrameCount = 3;
    public int coyoteTimeFrameCount = 3;

    private bool hasSetupControls = false;
    void SetupControls()
    {
        if (!hasSetupControls)
        {
            hasSetupControls = true;
            controls.Enable();
            controls.Gameplay.Jump.started += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                jumpPressTime = Time.time;
            };
            controls.Recording.Record.started += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                mover.ToggleRecording();
            };
            controls.Recording.Play.started += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                mover.TogglePlayback();
            };
            controls.Recording.Advance.started += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                mover.Advance();
            };
            controls.Recording.Reverse.started += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                mover.Reverse();
            };


        }
    }

    public void CalculateControls()
    {
        // 0.5 a t^2 + b t   = h
        //     a t   + b     = 0
        //             b     = -a t
        // 0.5 a t^2 - a t^2 = h
        // -0.5 a t^2 = h => a = -0.5 h / t^2
        minGravity = maxJumpHeight / (0.5f * jumpTime * jumpTime);
        jumpSpeed = minGravity * jumpTime;
        // 0.5 c t^2 + b t   = h
        //     c t   + b     = 0
        //       t           = -b / c
        // 0.5 b^2 / c - b^2 / c = h
        // (-0.5 b^2) /c = h
        // c = (-0.5 b^2) / h
        maxGravity = 0.5f * jumpSpeed * jumpSpeed / minJumpHeight;

        // Want slowdownPerTick^(1 / fixedUpdateTime) = slowdownPerSecond
        // So slowdownPerTick = Math.pow(slowdownPerSecond, fixedUpdateTime)
        wallSlidePerTickSlowdown = Mathf.Pow(wallSlideSlowdownPerSecond, Time.fixedDeltaTime);
    }

    void Start()
    {
        mover = GetComponent<RayMover>();
        mover.CheckForSupport(Vector2.down);
        controls = new MovementActions();
        
        CalculateControls();
    }

    void ClearExhaustion()
    {
        if (currentEnergy < maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }

    bool ShouldJump()
    {
        return Time.time - jumpPressTime < Time.fixedDeltaTime * jumpBufferFrameCount;
    }

    void SetVelocityComponent(Vector2 component, float amount)
    {
        Vector2 velocity = mover.velocity;
        float velocityInDirection = Vector2.Dot(component, velocity);
        mover.velocity += (amount - velocityInDirection) * component;
    }

    void MultiplyVelocityComponent(Vector2 component, float ratio)
    {
        Vector2 velocity = mover.velocity;
        float velocityInDirection = Vector2.Dot(component, velocity);
        mover.velocity += (ratio - 1) * velocityInDirection * component;
    }

    void JumpTowards(Vector2 component, float amount)
    {
        SetVelocityComponent(component, amount);
        jumpPressTime = Mathf.NegativeInfinity; // We handled the jump input
    }

    // Called on a fixed time step.
    void FixedUpdate()
    {
        SetupControls();
        
        // Read the value of our jump button. This is reported as a float, so we can have analog input.
        // Who know, maybe we will actually use that.
        float input = controls.Gameplay.Jump.ReadValue<float>();
        activeGravity = Mathf.Lerp(maxGravity, minGravity, input);
        Vector2 desiredMovement = controls.Gameplay.Move.ReadValue<Vector2>();

        if (!mover.Sliding())
        {
            Vector2 perpVelocity = mover.PerpendicularVelocity(Vector2.down);
            Vector2 desiredPerpVelocity = Vector2.SmoothDamp(perpVelocity, -mover.Left(Vector2.down) * speed * desiredMovement.x, ref movementDampVelocity, timeToReachSpeed, maxJerk);
            mover.velocity += desiredPerpVelocity - perpVelocity;
            if (ShouldJump() && mover.TimeInAir() < Time.fixedDeltaTime * coyoteTimeFrameCount)
            {
                JumpTowards(Vector2.up, jumpSpeed);
            }

            if (mover.Supported())
            {
                ClearExhaustion();
            }
        }
        
        if (!mover.Supported())
        {
            // If we are clinging to a wall, decreate our gravity.
            RayMover.WallDirection wall = mover.GetClingableWall();
            if (wall != RayMover.WallDirection.None)
            {
                // If we are pressing into a wall, then slow down our movement along the wall
                float wallDirectionModifier = wall == RayMover.WallDirection.Left ? -1 : 1;
                if (desiredMovement.x * wallDirectionModifier > 0)
                {
                    attachedToWall = true;
                    if (ShouldJump())
                    {
                        JumpTowards(new Vector2(-wallDirectionModifier, 1).normalized, jumpSpeed);
                    }

                    // If we are pressing into the wall, then slow ourselves down.
                    MultiplyVelocityComponent(mover.WallDown(), wallSlidePerTickSlowdown);
                }

                if (attachedToWall)
                {
                    activeGravity *= 0.5f;
                }
            }
            else
            {
                attachedToWall = false;
            }
        }
        else
        {
            attachedToWall = false;
        }

        mover.Move(Time.fixedDeltaTime, Vector2.down * activeGravity);
    }
}

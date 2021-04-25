using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public MovementControllerSettings settings;
    public RunVars run;
    public JumpVars jump;
    public WallSlideVars wallSlide;
    public EnergyVars energy;
    public WallRunVars wallRun;

    private MovementActions controls;
    private RayMover mover;

    private bool hasSetupControls = false;
    void SetupControls()
    {
        if (!hasSetupControls)
        {
            hasSetupControls = true;
            controls.Enable();
            controls.Gameplay.Jump.started += (UnityEngine.InputSystem.InputAction.CallbackContext _) => {
                jump.jumpPressTime = Time.time;
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

            CalculateControls();
        }
    }

    public void CalculateControls()
    {
        run = settings.GetRunVars();
        jump = settings.GetJumpVars();
        wallSlide = settings.GetWallSlideVars();
        energy = settings.GetEnergyVars();
        wallRun = settings.GetWallRunVars();
    }

    void Start()
    {
        mover = GetComponent<RayMover>();
        mover.CheckForSupport(Vector2.down, false);
        controls = new MovementActions();
        
        CalculateControls();
    }

    void SetVelocityComponent(Vector2 component, float amount)
    {
        Vector2 velocity = mover.velocity;
        float velocityInDirection = Vector2.Dot(component, velocity);
        mover.velocity += (amount - velocityInDirection) * component;
        if (mover.velocity.magnitude < Mathf.Epsilon)
        {
            print("Hit a zero case!");
        }
    }

    void MultiplyVelocityComponent(Vector2 component, float ratio)
    {
        Vector2 velocity = mover.velocity;
        float velocityInDirection = Vector2.Dot(component, velocity);
        mover.velocity += (ratio - 1) * velocityInDirection * component;
    }

    void JumpTowards(Vector2 component, float amount, bool add = false)
    {
        if (add)
        {
            mover.velocity += component * amount;
        }
        else
        {
            SetVelocityComponent(component, amount);
        }
        jump.jumpPressTime = Mathf.NegativeInfinity; // We handled the jump input
    }

    public bool CanJump()
    {
        return mover.TimeInAir() < jump.coyoteTolerance;
    }

    public bool CanWallJump()
    {
        return Time.time - wallSlide.lastAttachedToWall < jump.coyoteTolerance;
    }

    public void BeginBackWallRun()
    {
        wallRun.runningOnBackWall = true;
        wallRun.canRunOnBackWall = false;
    }

    public void BeginFrontWallRun()
    {
        wallRun.runningOnFrontWall = true;
        wallRun.runningOnBackWall = false;
    }

    public void DropWallRun()
    {
        if (wallRun.runningOnBackWall)
        {
            wallRun.runningOnBackWall = false;
        }
        if (wallRun.runningOnFrontWall)
        {
            wallRun.runningOnFrontWall = false;
        }
    }

    // Called on a fixed time step.
    void FixedUpdate()
    {
        SetupControls();
        
        // Read the value of our jump button. This is reported as a float, so we can have analog input.
        // Who know, maybe we will actually use that.
        float input = controls.Gameplay.Jump.ReadValue<float>();
        float activeGravity = Mathf.Lerp(jump.maxGravity, jump.minGravity, input);
        Vector2 desiredMovement = controls.Gameplay.Move.ReadValue<Vector2>();
        bool doWallRun = controls.Gameplay.WallRun.ReadValue<float>() > 0 && wallRun.speedNeddedForRun <= mover.velocity.magnitude;

        if (!doWallRun)
        {
            DropWallRun();
        }

        if (mover.velocity.magnitude > run.baseSpeed)
        {
            print("Hit the bad case");
        }

        if (!mover.Sliding())
        {
            float perpVelocity = -mover.PerpendicularVelocity(Vector2.down);
            float newVelocity = run.GetVelocity(perpVelocity, desiredMovement.x, mover.Supported());
            if (mover.velocity.magnitude > run.baseSpeed)
            {
                print("Hit the bad case");
            }
            SetVelocityComponent(-mover.Left(Vector2.down), newVelocity);
            if (mover.velocity.magnitude > run.baseSpeed)
            {
                print("Hit the bad case");
            }

            if (doWallRun && mover.GetClingableWall() != RayMover.WallDirection.None)
            {
                MultiplyVelocityComponent(mover.WallDown(), wallSlide.clingSpeedMultiplierPerTick);
            }

            if (mover.Supported())
            {
                energy.Reset();
                wallRun.canRunOnBackWall = true;
            }
            else
            {
                if (wallRun.backWallCount > 0 && wallRun.canRunOnBackWall && doWallRun)
                {
                    BeginBackWallRun();
                }
                if (wallRun.backWallCount == 0)
                {
                    DropWallRun();
                }

                if (wallRun.IsWallRunning())
                {
                    float verticalVelocity = Vector2.Dot(mover.velocity, Vector2.up);
                    float newVerticalVelocity = run.GetVelocity(verticalVelocity, desiredMovement.y, true);
                    SetVelocityComponent(Vector2.up, newVerticalVelocity * wallSlide.clingSpeedMultiplierPerTick);
                }
            }

            if (mover.SupportedByWall())
            {
                wallSlide.attachedToWall = true;
                wallSlide.lastAttachedToWall = Time.time;
                if (!wallRun.runningOnFrontWall) BeginFrontWallRun();
            }
            else if (mover.GetClingableWall() == RayMover.WallDirection.None)
            {
                wallSlide.attachedToWall = false;
            }
        }
        
        if (!mover.Supported() && !wallRun.IsWallRunning())
        {
            // If we are clinging to a wall, decreate our gravity.
            RayMover.WallDirection wall = mover.GetClingableWall();
            if (wall != RayMover.WallDirection.None)
            {
                // If we are pressing into a wall, then slow down our movement along the wall
                wallSlide.wallDirectionModifier = wall == RayMover.WallDirection.Left ? -1 : 1;
                if (desiredMovement.x * wallSlide.wallDirectionModifier > 0)
                {
                    wallSlide.attachedToWall = true;

                    // If we are pressing into the wall, then slow ourselves down.
                    MultiplyVelocityComponent(mover.WallDown(), wallSlide.clingSpeedMultiplierPerTick);
                }

                if (wallSlide.attachedToWall)
                {
                    activeGravity *= 0.5f;
                    wallSlide.lastAttachedToWall = Time.time;
                }
            }
            else
            {
                wallSlide.attachedToWall = false;
            }
        }

        if (jump.ShouldJump())
        {
            if (CanWallJump() || wallRun.IsWallRunning())
            {
                if (wallRun.runningOnBackWall)
                {
                    JumpTowards(desiredMovement.normalized, jump.launchSpeed, true);
                }
                else
                {
                    // First clear any momentum we have going toward the wall.
                    SetVelocityComponent(mover.WallNormal(), 0.0f);
                    JumpTowards(new Vector2(-wallSlide.wallDirectionModifier, 1).normalized, jump.launchSpeed);
                }
            }

            if (CanJump())
            {
                JumpTowards(Vector2.up, jump.launchSpeed);
            }
        }

        mover.Move(Time.fixedDeltaTime, Vector2.down * activeGravity, doWallRun);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Runnable")
        {
            wallRun.backWallCount++;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Runnable")
        {
            wallRun.backWallCount--;
        }
    }
}

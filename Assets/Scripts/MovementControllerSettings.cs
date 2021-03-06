using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MovementControllerSettings
{
    #region Run Settings
    public float baseSpeed;
    public float deathVelocity;
    public float accelerationTime;
    public float skidSpeed;
    public float stopSkiddingSpeed;
    public float maxBaseSkidAcceleration;
    public float skidMinControl;
    public float skidMaxControl;
    #endregion

    #region Jump Settings
    public float maxJumpHeight;
    public float minJumpHeight;
    public float secondsToApex;

    public int coyoteFrames;
    public int jumpBuffer;
    #endregion

    #region Wall Run Settings
    public float wallRunSpeed;
    #endregion

    #region Wall Slide Settings
    public float clingSpeedMultiplierPerSecond;
    public float maxClingTime;
    public float wallSlideAccelerationMultiplier;
    #endregion

    #region Exhaustion Settings
    public float maxEnergy;
    #endregion

    public RunVars GetRunVars()
    {
        RunVars vars = new RunVars();
        vars.baseSpeed = baseSpeed;
        vars.invBaseSpeed = 1 / baseSpeed;
        // We are using the exponential function as our base. So we want our acceleration to be
        //                  k Speed (1 - |Speed| / Max)
        // We will hit max acceleration at Max / 2. So we want to solve for
        //       acceleration = k (Max / 2) * .5 = k Max / 4
        // Therefore the power of our acceleration and deceleration can be given by
        //      k = (4 acceleration) / Max
        vars.baseAcceleration = baseSpeed / accelerationTime;
        vars.skidPower = (4 * maxBaseSkidAcceleration) / baseSpeed;
        vars.skidSpeed = skidSpeed;
        vars.skidMaxControl = skidMaxControl;
        vars.skidMinControl = skidMinControl;
        vars.stopSkiddingSpeed = stopSkiddingSpeed;
        return vars;
    }

    public JumpVars GetJumpVars()
    {
        JumpVars vars = new JumpVars();
        // 0.5 a t^2 + b t   = h
        //     a t   + b     = 0
        //             b     = -a t
        // 0.5 a t^2 - a t^2 = h
        // -0.5 a t^2 = h => a = -0.5 h / t^2
        vars.minGravity = maxJumpHeight / (0.5f * secondsToApex * secondsToApex);
        vars.launchSpeed = vars.minGravity * secondsToApex;
        // 0.5 c t^2 + b t   = h
        //     c t   + b     = 0
        //       t           = -b / c
        // 0.5 b^2 / c - b^2 / c = h
        // (-0.5 b^2) /c = h
        // c = (-0.5 b^2) / h
        vars.maxGravity = 0.5f * vars.launchSpeed * vars.launchSpeed / minJumpHeight;

        vars.coyoteTolerance = coyoteFrames * Time.fixedDeltaTime;
        vars.inputTolerance = jumpBuffer * Time.fixedDeltaTime;
        vars.deathVelocity = deathVelocity;
        return vars;
    }

    public WallRunVars GetWallRunVars()
    {
        WallRunVars vars = new WallRunVars();
        vars.canRunOnBackWall = true;
        vars.speedNeddedForRun = wallRunSpeed;
        return vars;
    }

    public WallSlideVars GetWallSlideVars()
    {
        WallSlideVars vars = new WallSlideVars();
        vars.clingSpeedMultiplierPerTick = Mathf.Pow(clingSpeedMultiplierPerSecond, Time.fixedDeltaTime);
        vars.wallSlideAccelerationMultiplier = wallSlideAccelerationMultiplier;
        return vars;
    }

    public EnergyVars GetEnergyVars()
    {
        EnergyVars vars = new EnergyVars();
        vars.maxEnergy = maxEnergy;
        vars.clingEnergyPerTick = maxClingTime / Time.fixedDeltaTime;
        vars.currentEnergy = maxEnergy;
        return vars;
    }
}

[System.Serializable]
public struct RunVars
{
    public float baseSpeed;
    public float bailSpeed;
    public float invBaseSpeed;
    public float rampupPower;
    public float skidPower;
    public float skidSpeed;
    public float stopSkiddingSpeed;
    public float baseAcceleration;
    public float skidMinControl;
    public float skidMaxControl;

    public bool isSkidding;
    public float skidDirection;

    public float GetVelocity(float currentVelocity, float moveInput, bool grounded)
    {
        float speed = Mathf.Abs(currentVelocity);
        // Detect when we should stop skidding.
        if (isSkidding && (speed < stopSkiddingSpeed || Mathf.Sign(currentVelocity) != skidDirection))
        {
            isSkidding = false;
        }
        float moveDegree = Mathf.Abs(moveInput);
        float speedDegree = speed * invBaseSpeed;
        if (speed > skidSpeed && moveDegree > Mathf.Epsilon && Mathf.Sign(moveInput) != Mathf.Sign(currentVelocity))
        {
            skidDirection = Mathf.Sign(currentVelocity);
            isSkidding = true;
        }

        if (isSkidding && grounded)
        {
            float control = Mathf.Lerp(skidMaxControl, skidMinControl, (1 + moveInput * skidDirection) / 2.0f);
            float acceleration = -skidPower * control * currentVelocity * speedDegree;
            return currentVelocity + acceleration * Time.fixedDeltaTime;
        }

        float target = moveInput * baseSpeed;
        float targetChange = target - currentVelocity;
        float currentChange = baseAcceleration * Time.fixedDeltaTime;
        float actualChange = Mathf.Sign(targetChange) * Mathf.Min(Mathf.Abs(targetChange), currentChange);

        return currentVelocity + actualChange;
    }
}

[System.Serializable]
public struct JumpVars
{
    public float minGravity;
    public float maxGravity;
    public float launchSpeed;
    public float deathVelocity;

    public float coyoteTolerance;
    public float inputTolerance;

    public float jumpPressTime;

    public bool ShouldJump()
    {
        return Time.time - jumpPressTime <= inputTolerance;
    }
}

[System.Serializable]
public struct WallSlideVars
{
    public float clingSpeedMultiplierPerTick;
    public float wallSlideAccelerationMultiplier;

    public bool attachedToWall;
    public float lastAttachedToWall;
    public float wallDirectionModifier;
}

[System.Serializable]
public struct WallRunVars
{
    public float speedNeddedForRun;
    public Vector2 currentVerticalWall;
    public bool canRunOnBackWall;
    public bool runningOnBackWall;
    public bool runningOnFrontWall;
    public int backWallCount;

    public bool IsWallRunning()
    {
        return runningOnBackWall || runningOnFrontWall;
    }
}

[System.Serializable]
public struct EnergyVars
{
    public float maxEnergy;
    public float clingEnergyPerTick;

    public float currentEnergy;

    public void Reset()
    {
        currentEnergy = maxEnergy;
    }
}
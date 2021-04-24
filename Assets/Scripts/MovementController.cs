using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float speed;
    public float timeToReachSpeed;
    public float maxJerk;
    // TODO: Make this human readable.
    public float gravity;
    public MovementActions controls;
    private RayMover mover;
    private Vector2 movementDampVelocity;

    void Start()
    {
        mover = GetComponent<RayMover>();
        mover.CheckForSupport(Vector2.down);
        controls = new MovementActions();
        
    }

    // Called on a fixed time step.
    void FixedUpdate()
    {
        controls.Enable();
        if (!mover.Sliding())
        {
            Vector2 desiredMovement = controls.Gameplay.Move.ReadValue<Vector2>();
            Vector2 perpVelocity = mover.PerpendicularVelocity(Vector2.down);
            Vector2 desiredPerpVelocity = Vector2.SmoothDamp(perpVelocity, -mover.Left(Vector2.down) * speed * desiredMovement.x, ref movementDampVelocity, timeToReachSpeed, maxJerk);
            mover.velocity += desiredPerpVelocity - perpVelocity;
        }
        mover.Move(Time.fixedDeltaTime, Vector2.down * gravity);
    }
}

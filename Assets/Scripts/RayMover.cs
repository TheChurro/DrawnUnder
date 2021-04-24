using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMover : MonoBehaviour
{
    public float radius;
    public float skinwidth;
    public Vector2 velocity;
    public int iterationCount;
    public float maxSlopeAngle;
    private float maxSlopeCos;
    public float solidTolerance;
    public LayerMask layerMask;

    public bool supported;
    public bool sliding;
    public Vector2 supportNormal;
    public float inAirFor;
    public bool wallOnLeft;
    public bool wallOnRight;

    public float[] times;

    public void Awake()
    {
        maxSlopeCos = Mathf.Cos(Mathf.Deg2Rad * maxSlopeAngle);
        times = new float[iterationCount];
    }

    public void Move(float time, Vector2 netForce)
    {
        for (int i = 0; i < iterationCount; i++)
        {
            // Do not move into the slope we are running parallel to.
            Vector2 displacement = (velocity + netForce * time) * time;
            if (supported)
            {
                displacement -= Vector2.Dot(displacement, supportNormal) * supportNormal;
            }
            if (CastAndMove(displacement, netForce, ref time))
            {
                break;
            }
            times[i] = time;
        }
    }

    private bool CastAndMove(Vector2 displacement, Vector2 netForce, ref float time)
    {
        float distance = displacement.magnitude;
        // Early out of moving if we aren't moving...
        if (distance < Mathf.Epsilon)
        {
            return true;
        }
        Vector2 direction = displacement / distance;
        if (Cast(direction, distance, out var hit))
        {
            // Update the amount we have moved. This includes the effect of netforce
            // on our velocity.
            float timeToFirstHit = hit.distance / distance;
            time -= timeToFirstHit;
            velocity += netForce * timeToFirstHit;
            // Move in the direction the given distance, but then move away from the
            // hit object by skin width. Note: we can move into another object this way
            // but over time we will resolve out of the objects.
            transform.position += (Vector3) (direction * hit.distance);
            TransferSupport(hit.normal, netForce);
            return false;
        }
        else
        {
            velocity += netForce * time;
            transform.position += (Vector3) displacement;
            time = 0;
            CheckForSupport(netForce);
            return true;
        }
    }

    public void CheckForSupport(Vector2 forceDirection)
    {
        if (Cast(forceDirection.normalized, skinwidth, out var hit))
        {
            supported = true;
            supportNormal = hit.normal;
            transform.position += (Vector3)(forceDirection.normalized * hit.distance);
            velocity -= supportNormal * Vector2.Dot(supportNormal, velocity);
            sliding = Vector2.Dot(hit.normal, -forceDirection) < maxSlopeCos;
        }
        else
        {
            supported = false;
        }
    }

    // Updates our supported state to true with the new given normal.
    // Also updates our velocity to keep the same perpendicular speed
    // we had relative to our last support if we can move on this slope
    private void TransferSupport(Vector2 newSupportNormal, Vector2 netForce)
    {
        Vector2 forceDirection = netForce.normalized;
        Vector2 oldSupport = forceDirection;
        if (supported)
        {
            oldSupport = supportNormal;
        }
        supportNormal = newSupportNormal;
        supported = true;

        if (Vector2.Dot(-forceDirection, newSupportNormal) < maxSlopeCos)
        {
            // Remove movement in the direction of our normal vector
            velocity -= Vector2.Dot(velocity, newSupportNormal) * newSupportNormal;
            sliding = true;
        }
        else
        {
            // Move perpendicular to our normal in the same way we were moving
            // perpendicular to our last normal.
            float perpSpeed = Vector2.Dot(velocity, Vector2.Perpendicular(oldSupport));
            velocity = perpSpeed * Vector2.Perpendicular(newSupportNormal);
            sliding = false;
        }
    }

    private bool Cast(Vector2 direction, float distance, out RaycastHit2D firstHit)
    {
        firstHit = new RaycastHit2D();
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius - skinwidth, direction, distance + skinwidth, layerMask);
        if (hits.Length == 0)
        {
            return false;
        }
        float firstHitDistance = Mathf.Infinity;
        bool hasHit = false;
        foreach (var hit in hits)
        {
            // Ignore surfaces we are moving away from. Allows for one way platforms and jumping out of walls.
            if (Vector2.Dot(hit.normal, direction) > 0.0f)
            {
                continue;
            }
            if (hit.distance < firstHitDistance)
            {
                firstHitDistance = hit.distance;
                firstHit = hit;
                firstHit.distance = Mathf.Min(0.0f, firstHit.distance - skinwidth);
                hasHit = true;
            }
        }
        return hasHit;
    }

    public bool Supported()
    {
        return supported;
    }

    public bool Sliding()
    {
        return supported && sliding;
    }

    public Vector2 Left(Vector2 netForce)
    {
        if (supported)
        {
            return Vector2.Perpendicular(supportNormal);
        }
        return -Vector2.Perpendicular(netForce.normalized);
    }

    public Vector2 PerpendicularVelocity(Vector2 netForce)
    {
        Vector2 left = Left(netForce);
        return left * Vector2.Dot(left, velocity);
    }
}

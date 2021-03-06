using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LineExtension
{
    public static bool IsHorizontal(this RayMover.Line line)
    {
        return line == RayMover.Line.Left || line == RayMover.Line.Right;
    }

    public static bool IsVertical(this RayMover.Line line)
    {
        return line == RayMover.Line.Up || line == RayMover.Line.Down;
    }

    public static Vector2 Direction(this RayMover.Line line)
    {
        switch (line)
        {
            case RayMover.Line.Down:
                return Vector2.down;
            case RayMover.Line.Up:
                return Vector2.up;
            case RayMover.Line.Left:
                return Vector2.left;
            case RayMover.Line.Right:
                return Vector2.right;
        }
        return Vector2.zero;
    }
}

public class DebugData
{
    [System.Flags]
    public enum DebugTag : int {
        None = 0,
        MainCast = 1,
        AdjustCast = 2,
        GroundCast = 4,
        Supports = 8,
        Positions = 16,
        SupportTransfer = 32,
    }
    public Dictionary<DebugTag, List<RaycastInfo>> castInfo = new Dictionary<DebugTag, List<RaycastInfo>>();
    public List<SupportInfo> supports = new List<SupportInfo>();
    public List<RecordInfo> positions = new List<RecordInfo>();
    public List<TransferRecord> momentumTransferRecord = new List<TransferRecord>();
    private int index = 0;
    public void Clear()
    {
        castInfo.Clear();
        supports.Clear();
        positions.Clear();
        momentumTransferRecord.Clear();
    }
    public void Insert(DebugTag tag, RaycastInfo info)
    {
        info.index = index;
        if (!castInfo.ContainsKey(tag)) castInfo.Add(tag, new List<RaycastInfo>());
        castInfo[tag].Add(info);
    }
    public void AddSupportInfo(SupportInfo support)
    {
        support.index = index;
        supports.Add(support);
    }
    public void AddRecord(RayMover.Record record)
    {
        positions.Add(new RecordInfo(){
            record = record,
            index = index,
        });
    }
    public void MarkMomentumTransfer(Vector2 oldNormal, Vector2 newNormal, Vector2 oldPerp, Vector2 newPerp, bool flipped, Vector2 oldVelocityTowardsNewPerp)
    {
        momentumTransferRecord.Add(new TransferRecord() {
            index = index,
            oldNormal = oldNormal,
            newNormal = newNormal,
            oldPerp = oldPerp,
            newPerp = newPerp,
            flipped = flipped,
            oldVelocityTowardsNewPerp = oldVelocityTowardsNewPerp,
        });
    }
    public void SetIndex(int i)
    {
        index = i;
    }
    public int GetIndex()
    {
        return index;
    }
    public struct RaycastInfo {
        public Vector2 start;
        public Vector2 end;
        public int index;
    }
    public struct SupportInfo {
        public Vector2 supportLocation;
        public Vector2 supportNormal;
        public int index;
    }
    public struct RecordInfo {
        public RayMover.Record record;
        public int index;
    }
    public struct TransferRecord {
        public Vector2 oldNormal;
        public Vector2 newNormal;
        public Vector2 oldPerp;
        public Vector2 newPerp;
        public Vector2 oldVelocityTowardsNewPerp;
        public bool flipped;
        public int index;
    }
}

public class RayMover : MonoBehaviour
{
    public float width;
    public float height;
    public Vector2 offset;
    public int pointsPerLine;
    public float skinwidth;
    public int iterationCount;
    public float maxSlopeAngle;
    public float minWallAngle;
    private float maxSlopeCos;
    private float minWallCos;
    public LayerMask layerMask;
    

    public Vector2 velocity;
    public bool supported;
    public bool sliding;
    public Vector2 supportNormal;
    public float airtimeStart;
    public bool wallOnLeft;
    public Vector2 leftWallNormal;
    public bool wallOnRight;
    public Vector2 rightWallNormal;

    public DebugData.DebugTag displayCastTags;
    private DebugData data = new DebugData();

    public enum Line {
        Down,
        Left,
        Up,
        Right
    }

    public enum WallDirection {
        None,
        Left,
        Right
    }

    public struct Record {
        public Vector2 velocity;
        public bool supported;
        public bool sliding;
        public bool allowWallRunning;
        public Vector2 supportNormal;
        public Vector2 netForce;
        public Vector3 position;
        public float time;
    }
    private List<Record> records;
    private bool recording;
    private bool playingBack;
    private int playbackIndex;
    private int dataIndex = -2;

    private Record MakeRecord(Vector2 netForce, float time, bool allowWallRunning)
    {
        return new Record() {
            velocity = velocity,
            supported = supported,
            sliding = sliding,
            allowWallRunning = allowWallRunning,
            supportNormal = supportNormal,
            netForce = netForce,
            position = transform.position,
            time = time,
        };
    }

    private void LoadRecord(Record record, ref Vector2 netForce, ref bool allowWallRunning)
    {
        velocity = record.velocity;
        supported = record.supported;
        sliding = record.sliding;
        supportNormal = record.supportNormal;
        netForce = record.netForce;
        transform.position = record.position;
        allowWallRunning = record.allowWallRunning;
    }
    
    public void ToggleRecording()
    {
        if (recording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    public void StartRecording()
    {
        records = new List<Record>();
        recording = true;
        playingBack = false;
    }

    public void StopRecording()
    {
        recording = false;
    }

    public void TogglePlayback()
    {
        if (playingBack)
        {
            StopPlayback();
        }
        else
        {
            StartPlayback();
        }
    }

    public void StartPlayback()
    {
        StopRecording();
        playingBack = records != null && records.Count > 0;
        playbackIndex = 0;
    }

    public void Advance()
    {
        if (playingBack)
        {
            dataIndex++;
            if (dataIndex > data.GetIndex())
            {
                dataIndex = -2;
                playbackIndex++;
                if (playbackIndex >= records.Count)
                    playbackIndex = records.Count - 1;
            }
        }
    }

    public void Reverse()
    {
        if (playingBack)
        {
            dataIndex--;
            if (dataIndex < -2)
            {
                dataIndex = iterationCount;
                playbackIndex--;
                if (playbackIndex < 0)
                    playbackIndex = 0;
            }
        }
    }

    public void StopPlayback()
    {
        playingBack = false;
    }

    private void GetLinePoints(Line line, out Vector2 point0, out Vector2 point1)
    {
        GetLinePoints(line, out point0, out point1, this.transform.position);
    }

    private void GetLinePoints(Line line, out Vector2 point0, out Vector2 point1, Vector2 basePos)
    {
        basePos += offset;
        point1 = basePos;
        point0 = basePos;
        switch (line)
        {
            case Line.Down:
                point0 = basePos + new Vector2(-width / 2 + skinwidth, -height / 2 + skinwidth);
                point1 = basePos + new Vector2( width / 2 - skinwidth, -height / 2 + skinwidth);
                return;
            case Line.Up:
                point1 = basePos + new Vector2( width / 2 - skinwidth,  height / 2 - skinwidth);
                point0 = basePos + new Vector2(-width / 2 + skinwidth,  height / 2 - skinwidth);
                return;
            case Line.Left:
                point0 = basePos + new Vector2(-width / 2 + skinwidth,  height / 2 - skinwidth);
                point1 = basePos + new Vector2(-width / 2 + skinwidth, -height / 2 + skinwidth);
                return;
            case Line.Right:
                point0 = basePos + new Vector2( width / 2 - skinwidth,  height / 2 - skinwidth);
                point1 = basePos + new Vector2( width / 2 - skinwidth, -height / 2 + skinwidth);
                return;
        }
    }

    public void UpdateFromBoxCollider()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        width = collider.size.x;
        height = collider.size.y;
        offset = collider.offset;
    }

    public void Awake()
    {
        maxSlopeCos = Mathf.Cos(Mathf.Deg2Rad * maxSlopeAngle);
        minWallCos = Mathf.Cos(Mathf.Deg2Rad * minWallAngle);
    }

    public void Move(float time, Vector2 netForce, bool allowWallRunning)
    {
        if (recording)
        {
            records.Add(MakeRecord(netForce, time, allowWallRunning));
        }
        else if (playingBack)
        {
            LoadRecord(records[playbackIndex], ref netForce, ref allowWallRunning);
        }
        data.Clear();
        data.SetIndex(-2);
        if (!SupportedByWall())
        {
            CheckForSupport(netForce.normalized, allowWallRunning);
        }
        data.AddRecord(MakeRecord(netForce, time, allowWallRunning));
        data.SetIndex(-1);
        if (allowWallRunning && !supported)
        {
            CheckWallCling(netForce.normalized);
            if (wallOnLeft || wallOnRight) UpdateNormal(WallNormal(), netForce.normalized, allowWallRunning);
        }
        data.AddRecord(MakeRecord(netForce, time, allowWallRunning));
        int iteration = 0;
        for (; iteration < iterationCount; iteration++)
        {
            data.SetIndex(iteration);
            data.AddRecord(MakeRecord(netForce, time, allowWallRunning));
            // Do not move into the slope we are running parallel to.
            Vector2 displacement = (velocity + netForce * time) * time;
            if (supported)
            {
                float normalMovement = Vector2.Dot(displacement, supportNormal);
                if (normalMovement < 0) // Moving into the platform we are supported by.
                {
                    displacement -= normalMovement * supportNormal;
                }
            }
            if (CastAndMove(displacement, netForce, allowWallRunning, ref time))
            {
                iteration++;
                break;
            }
        }
        data.SetIndex(iteration);
        CheckWallCling(netForce.normalized);
        if (allowWallRunning && (!supported || SupportedByWall()))
        {
            if (wallOnLeft || wallOnRight) UpdateNormal(WallNormal(), netForce.normalized, allowWallRunning);
        }
        data.AddRecord(MakeRecord(netForce, time, allowWallRunning));
    }

    private bool CastAndMove(Vector2 displacement, Vector2 netForce, bool allowWallRunning, ref float time)
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
            // Move in the direction the given distance, but then move away from the
            // hit object by skin width. Note: we can move into another object this way
            // but over time we will resolve out of the objects.
            transform.position += (Vector3) (direction * hit.distance);
            TransferSupport(hit.normal, netForce, allowWallRunning);
            // Update the amount we have moved. This includes the effect of netforce
            // on our velocity.
            float timeToFirstHit = time * hit.distance / distance;
            time -= timeToFirstHit;
            velocity += netForce * timeToFirstHit;
            return false;
        }
        else
        {
            velocity += netForce * time;
            transform.position += (Vector3) displacement;
            time = 0;
            CheckForSupport(netForce, allowWallRunning);
            return true;
        }
    }

    public void UpdateNormal(Vector2 newNormal, Vector2 forceDirection, bool allowWallRunning, bool updateVelocity = true)
    {
        supportNormal = newNormal;
        supported = Vector2.Dot(-forceDirection, supportNormal) > minWallCos; // We aren't touching a wall
        sliding = Vector2.Dot(-forceDirection, supportNormal) < maxSlopeCos && !(allowWallRunning && supported); // We aren't sliding on that wall.
        if (supported && updateVelocity)
        {
            float velocityTowardSupport = Vector2.Dot(supportNormal, velocity);
            if (velocityTowardSupport < 0)
            {
                velocity -= velocityTowardSupport * supportNormal;
            }
        }
    }

    public void CheckForSupport(Vector2 forceDirection, bool allowWallRunning)
    {
        RaycastHit2D hit = new RaycastHit2D();
        if (CastLine(Line.Down, Vector2.down, skinwidth, ref hit, DebugData.DebugTag.GroundCast))
        {
            UpdateNormal(hit.normal, forceDirection, allowWallRunning);
            transform.position += (Vector3)(Vector2.down * hit.distance);
            float speedAwayFromNormal = Vector2.Dot(supportNormal, velocity);
            if (speedAwayFromNormal < 0.0f)
            {
                velocity -= supportNormal * speedAwayFromNormal;
            }
            data.AddSupportInfo(new DebugData.SupportInfo() {
                supportLocation = hit.point,
                supportNormal = hit.normal,
            });
        }
        else
        {
            if (supported && !sliding)
                airtimeStart = Time.time;
            supported = false;
        }
    }

    public void CheckWallCling(Vector2 forceDirection)
    {
        RaycastHit2D hit = new RaycastHit2D();
        wallOnLeft = CastLine(Line.Left, Line.Left.Direction(), skinwidth, ref hit, DebugData.DebugTag.GroundCast);
        if (wallOnLeft)
        {
            // TODO: check that these are actually walls...
            leftWallNormal = hit.normal;
        }
        wallOnRight = CastLine(Line.Right, Line.Right.Direction(), skinwidth, ref hit, DebugData.DebugTag.GroundCast);
        if (wallOnRight)
        {
            rightWallNormal = hit.normal;
        }
    }

    // Updates our supported state to true with the new given normal.
    // Also updates our velocity to keep the same perpendicular speed
    // we had relative to our last support if we can move on this slope
    private void TransferSupport(Vector2 newSupportNormal, Vector2 netForce, bool allowWallRunning)
    {
        Vector2 forceDirection = netForce.normalized;
        Vector2 oldSupport = -forceDirection;
        if (supported)
        {
            oldSupport = supportNormal;
        }
        bool wasSliding = Sliding();
        UpdateNormal(newSupportNormal, forceDirection, allowWallRunning, false);
        if (!supported || (!allowWallRunning && (sliding || wasSliding)))
        {
            // Remove movement in the direction of our normal vector
            float velocityIntoNormal = Vector2.Dot(velocity, newSupportNormal);
            if (velocityIntoNormal < 0.0f)
            {
                velocity -= velocityIntoNormal * newSupportNormal;
            }
        }
        else
        {
            
            // Move perpendicular to our normal in the same way we were moving
            // perpendicular to our last normal.
            float perpSpeed = Vector2.Dot(velocity, Vector2.Perpendicular(oldSupport));
            Vector2 targetPerp = Vector2.Perpendicular(newSupportNormal);
            float velocityTowardsTransferDirection = Vector2.Dot(velocity, targetPerp);
            bool flipped = false;
            if (Mathf.Abs(velocityTowardsTransferDirection) > Mathf.Epsilon && Mathf.Sign(velocityTowardsTransferDirection) != Mathf.Sign(perpSpeed))
            {
                targetPerp *= -1;
                flipped = true;
            }
            velocity = perpSpeed * targetPerp;
            data.MarkMomentumTransfer(oldSupport, supportNormal, perpSpeed * Vector2.Perpendicular(oldSupport), perpSpeed * targetPerp, flipped, velocityTowardsTransferDirection * Vector2.Perpendicular(newSupportNormal));
        }
    }

    private bool CastLine(Line line, Vector2 direction, float distance, ref RaycastHit2D closestHit, DebugData.DebugTag tag)
    {
        return CastLine(line, direction, distance, Vector2.zero, skinwidth, ref closestHit, tag);
    }
    private bool CastLine(Line line, Vector2 direction, float distance, Vector2 offset, float padding, ref RaycastHit2D closestHit, DebugData.DebugTag tag)
    {
        GetLinePoints(line, out var start, out var end);
        return CastLine(start + offset, end + offset, direction, distance, padding, ref closestHit, tag);
    }

    private bool CastLine(Vector2 start, Vector2 end, Vector2 direction, float distance, float padding, ref RaycastHit2D closestHit, DebugData.DebugTag tag)
    {
        float closestHitDistance = distance + padding;
        bool hasHit = false;
        for (int i = 0; i < pointsPerLine; i++)
        {
            float ratio = i / (float)(pointsPerLine - 1);
            Vector2 position = Vector2.Lerp(start, end, ratio);
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, closestHitDistance, layerMask);
            foreach (var hit in hits)
            {
                // Ignore things we are moving away from.
                if (Vector2.Dot(hit.normal, direction) > Mathf.Epsilon)
                {
                    continue;
                }
                if (hit.distance < closestHitDistance)
                {
                    closestHitDistance = hit.distance;
                    closestHit = hit;
                    closestHit.distance = Mathf.Max(0.0f, closestHit.distance - padding);
                    hasHit = true;
                }
            }
            data.Insert(tag, new DebugData.RaycastInfo(){
                start = position,
                end = position + direction * (hasHit ? closestHit.distance : closestHitDistance),
            });
        }
        return hasHit;
    }

    private float GetUsedSkin(Line line, Vector2 targetOffset)
    {
        float usedSkin = 0.0f;
        RaycastHit2D hit = new RaycastHit2D();
        if (CastLine(line, line.Direction(), skinwidth, targetOffset, 0.0f, ref hit, DebugData.DebugTag.AdjustCast))
        {
            usedSkin = skinwidth - hit.distance;
        }
        return usedSkin;
    }

    private bool Cast(Vector2 direction, float distance, out RaycastHit2D firstHit)
    {
        firstHit = new RaycastHit2D();
        firstHit.distance = distance;
        bool hasHit = false;
        if (Mathf.Abs(direction.x) > Mathf.Epsilon)
        {
            hasHit = CastLine(direction.x < 0 ? Line.Left : Line.Right, direction, distance, ref firstHit, DebugData.DebugTag.MainCast);
        }
        if (Mathf.Abs(direction.y) > Mathf.Epsilon)
        {
            hasHit |= CastLine(direction.y < Mathf.Epsilon ? Line.Down : Line.Up, direction, firstHit.distance, ref firstHit, DebugData.DebugTag.MainCast);
        }

        // We not want to adjust our movement so that we do not hit into anything. To do this, we are going to
        // Cast directly outward along the edges of our square at the location we are attempting to travel to.
        // This will give us how far we need to move along each direction to place us at skin-width away from
        // the object we just hit. This will not modify the hit information aside from the distance we moved.
        if (hasHit)
        {
            Vector2 targetOffset = firstHit.distance * direction;
            Line horizontalLine = direction.x < 0 ? Line.Left : Line.Right;
            float horizontalAdjustment = GetUsedSkin(horizontalLine, targetOffset);
            Line verticalLine = direction.y < Mathf.Epsilon ? Line.Down : Line.Up;
            float verticalAdjustment = GetUsedSkin(verticalLine, targetOffset);
            Vector2 absDirection = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

            float totalAdjustment = 0.0f;

            // The goal of this is numerical precision...
            if (absDirection.x < absDirection.y)
            {
                float horizontalAdjustmentForVertical = verticalAdjustment * absDirection.x / absDirection.y;
                if (horizontalAdjustmentForVertical >= horizontalAdjustment)
                {
                    // If adjusting our position for vertical movement if enough to overcome our needed horizontal adjustment, use
                    // only the adjustment we need for our vertical movement.
                    totalAdjustment = new Vector2(verticalAdjustment, horizontalAdjustmentForVertical).magnitude;
                }
                else
                {
                    float verticalAdjustmentForHorizontal = horizontalAdjustment * absDirection.y / absDirection.x;
                    totalAdjustment = new Vector2(horizontalAdjustment, verticalAdjustmentForHorizontal).magnitude;
                }
            }
            else
            {
                float verticalAdjustmentForHorizontal = horizontalAdjustment * absDirection.y / absDirection.x;
                
                if (verticalAdjustmentForHorizontal >= verticalAdjustment)
                {
                    totalAdjustment = new Vector2(horizontalAdjustment, verticalAdjustmentForHorizontal).magnitude;
                }
                else
                {
                    float horizontalAdjustmentForVertical = verticalAdjustment * absDirection.x / absDirection.y;
                    totalAdjustment = new Vector2(verticalAdjustment, horizontalAdjustmentForVertical).magnitude;
                }
            }

            firstHit.distance -= totalAdjustment;
            if (firstHit.distance < 0) firstHit.distance = 0;
        }

        return hasHit;
    }

    public bool Supported()
    {
        return supported;
    }

    public bool SupportedByWall()
    {
        return supported && !sliding && Vector2.Dot(supportNormal, Vector2.up) < maxSlopeCos;
    }

    public WallDirection GetClingableWall()
    {
        return wallOnRight ? WallDirection.Right : wallOnLeft ? WallDirection.Left : WallDirection.None;
    }

    public Vector2 WallNormal()
    {
        return wallOnRight ? rightWallNormal : wallOnLeft ? leftWallNormal : Vector2.zero;
    }

    public Vector2 WallDown()
    {
        Vector2 wallNormal = WallNormal();
        Vector2 wallPerp = Vector2.Perpendicular(wallNormal);
        if (Vector2.Dot(wallPerp, Vector2.down) < 0)
        {
            wallPerp *= -1;
        }
        return wallPerp;
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

    public float PerpendicularVelocity(Vector2 netForce)
    {
        Vector2 left = Left(netForce);
        return Vector2.Dot(left, velocity);
    }

    public float TimeInAir()
    {
        return Supported() ? 0.0f : Time.time - airtimeStart;
    }

    public void OnDrawGizmos()
    {
        if (dataIndex > data.GetIndex())
        {
            dataIndex = data.GetIndex();
        }

        if ((displayCastTags & DebugData.DebugTag.MainCast) == DebugData.DebugTag.MainCast)
        {
            Gizmos.color = Color.red;
            if (data.castInfo.TryGetValue(DebugData.DebugTag.MainCast, out var lines))
            {
                foreach (var line in lines)
                {
                    if (line.index == dataIndex)
                    {
                        Gizmos.DrawLine(line.start, line.end);
                    }
                }
            }
        }

        if ((displayCastTags & DebugData.DebugTag.GroundCast) == DebugData.DebugTag.GroundCast)
        {
            Gizmos.color = Color.green;
            if (data.castInfo.TryGetValue(DebugData.DebugTag.GroundCast, out var lines))
            {
                foreach (var line in lines)
                {
                    if (line.index == dataIndex)
                    {
                        Gizmos.DrawLine(line.start, line.end);
                        Gizmos.DrawCube(line.start, new Vector3(0.01f, 0.01f, 0.01f));
                        Gizmos.DrawCube(line.start - new Vector2(0, skinwidth), new Vector3(0.01f, 0.01f, 0.01f));
                    }
                }
            }
        }

        if ((displayCastTags & DebugData.DebugTag.AdjustCast) == DebugData.DebugTag.AdjustCast)
        {
            Gizmos.color = Color.white;
            if (data.castInfo.TryGetValue(DebugData.DebugTag.AdjustCast, out var lines))
            {
                foreach (var line in lines)
                {
                    if (line.index == dataIndex)
                    {
                        Gizmos.DrawLine(line.start, line.end);
                    }
                }
            }
        }

        if ((displayCastTags & DebugData.DebugTag.Supports) == DebugData.DebugTag.Supports)
        {
            Gizmos.color = Color.magenta;
            foreach (var support in data.supports)
            {
                if (support.index == dataIndex)
                {
                    Gizmos.DrawLine(support.supportLocation, support.supportLocation + support.supportNormal);
                    Gizmos.DrawSphere(support.supportLocation + support.supportNormal, 0.01f);
                }
            }
        }

        if ((displayCastTags & DebugData.DebugTag.Positions) == DebugData.DebugTag.Positions)
        {
            foreach (var record in data.positions)
            {
                if (record.index == dataIndex)
                {
                    Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                    float timeLeft = record.record.time / Time.fixedDeltaTime;
                    Gizmos.DrawCube(record.record.position + (Vector3.down * height * (1 - timeLeft) / 2), new Vector2(width, height * timeLeft));
                    Gizmos.color = Color.yellow;
                    Vector2 pos0, pos1;
                    GetLinePoints(Line.Up, out pos0, out pos1, record.record.position);
                    Gizmos.DrawLine(pos0, pos1);
                    GetLinePoints(Line.Down, out pos0, out pos1, record.record.position);
                    Gizmos.DrawLine(pos0, pos1);
                    GetLinePoints(Line.Left, out pos0, out pos1, record.record.position);
                    Gizmos.DrawLine(pos0, pos1);
                    GetLinePoints(Line.Right, out pos0, out pos1, record.record.position);
                    Gizmos.DrawLine(pos0, pos1);
                    if (record.record.supported)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(record.record.position + new Vector3(width, height / 3), height / 6);
                    }
                    if (record.record.sliding)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(record.record.position + new Vector3(width, -height / 3), height / 6);
                    }
                    if (record.record.allowWallRunning)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(record.record.position + new Vector3(-width, -height / 3), height / 6);
                    }
                    foreach (var transfer in data.momentumTransferRecord)
                    {
                        if (transfer.index == dataIndex)
                        {
                            Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.5f);
                            Vector3 visPos = record.record.position + new Vector3(-width, height / 3);
                            Gizmos.DrawSphere(visPos, height / 6);
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawRay(visPos, transfer.oldPerp);
                            Gizmos.color = Color.red;
                            Gizmos.DrawRay(visPos, transfer.newPerp);
                            visPos += new Vector3(height / 3, width / 3);
                            if (transfer.flipped)
                            {
                                Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.5f);
                                Gizmos.DrawSphere(visPos, height / 6);
                            }
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawRay(visPos, transfer.oldNormal);
                            Gizmos.color = Color.red;
                            Gizmos.DrawRay(visPos, transfer.newNormal);
                            Gizmos.color = Color.grey;
                            Gizmos.DrawSphere(visPos, transfer.oldVelocityTowardsNewPerp.magnitude);
                        }
                    }
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(record.record.position, record.record.supportNormal * Mathf.Min(width, height) / 4);
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(record.record.position, record.record.velocity * Mathf.Min(width, height) / 4);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public CompositeCollider2D bounds;
    public Vector2 draw;
    public Door[] doors;
    public bool loaded = false;
    // Start is called before the first frame update
    void Start()
    {
        doors = GetComponentsInChildren<Door>();
        loaded = true;
    }

    public Vector2 GetDoorSpawn(string fromRoom)
    {
        foreach (var door in doors)
        {
            print($"Door Name: {door.toRoom} | From Room Name: {fromRoom}");
            if (door.toRoom == fromRoom)
            {
                return door.playerSpawn.position;
            }
        }
        return transform.position;
    }

    public bool IsLoaded()
    {
        return loaded;
    }
}

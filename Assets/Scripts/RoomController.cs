using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public static RoomController instance;

    [System.Serializable]
    public struct RoomInstance
    {
        public string name;
        public GameObject prefab;
    }

    public RoomInstance[] roomDescriptors;
    public string startRoomName;
    public string priorRoomName;
    private string loadingRoomName;
    private bool waitingOnRoomLoad = false;
    private GameObject loadingRoom;
    private Room activeRoom;
    public MovementController player;
    public Cinemachine.CinemachineConfiner confiner;
    public Transform activeRoomTransform;
    public Transform loadingRoomTransform;
    private Vector3 lastSpawn;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        TransitionToRoom(startRoomName);
    }

    bool hasKeys;

    public void SetHasKeys(bool value)
    {
        hasKeys = value;
    }

    public bool HasKeys()
    {
        return hasKeys;
    }

    public void Update()
    {
        if (waitingOnRoomLoad)
        {
            Room room = loadingRoom.GetComponent<Room>();
            if (room.IsLoaded())
            {
                loadingRoom = null;
                waitingOnRoomLoad = false;
                if (activeRoom) GameObject.Destroy(activeRoom.gameObject);
                activeRoom = room;
                activeRoom.transform.position = activeRoomTransform.position;

                confiner.m_BoundingShape2D = room.bounds;
                player.Unpause();
                player.Teleport(room.GetDoorSpawn(priorRoomName));
                player.SetDraw(room.draw);
                priorRoomName = loadingRoomName;

                lastSpawn = player.transform.position;

                waitingOnRoomLoad = false;

                // TODO: Unscreen transition
            }
        }
    }

    public void TransitionToRoom(string roomName)
    {
        if (waitingOnRoomLoad) return;
        foreach (var room in roomDescriptors)
        {
            if (room.name == roomName)
            {
                loadingRoomName = room.name;
                loadingRoom = GameObject.Instantiate(room.prefab, loadingRoomTransform.position, Quaternion.identity);
                waitingOnRoomLoad = true;

                player.Pause();
                // TODO: Screen transition
                return;
            }
        }
    }

    public Vector3 GetLastSpawn()
    {
        return lastSpawn;
    }
}

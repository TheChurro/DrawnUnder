using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string toRoom;
    public Transform playerSpawn;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            RoomController.instance?.TransitionToRoom(toRoom);
        }
    }
}

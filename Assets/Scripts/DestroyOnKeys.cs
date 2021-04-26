using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnKeys : MonoBehaviour
{
    public bool expectedValue;

    // Update is called once per frame
    void Update()
    {
        if (RoomController.instance?.HasKeys() == expectedValue)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Internal Controller Class */
//Purpose: Control the game's camera

public class CameraController : MonoBehaviour
{
    public Camera GameCamera;

    public float Speed = 1.0f;

    private Dictionary<string, Vector3> directionMappings;

    //current movement direction of the camera
    private Vector3 _curDirection;

    void Start()
    {
        directionMappings = new Dictionary<string, Vector3>();

        //set up direction mappings
        directionMappings.Add("none", new Vector3(0, 0, 0));
        directionMappings.Add("north", new Vector3(0, 0, 1));
        directionMappings.Add("south", new Vector3(0, 0, -1));
        directionMappings.Add("west", new Vector3(-1, 0, 0));
        directionMappings.Add("east", new Vector3(1, 0, 0));
        directionMappings.Add("north-west", new Vector3(-Mathf.Sqrt(2) / 2, 0, Mathf.Sqrt(2) / 2));
        directionMappings.Add("north-east", new Vector3(Mathf.Sqrt(2) / 2, 0, Mathf.Sqrt(2) / 2));
        directionMappings.Add("south-west", new Vector3(-Mathf.Sqrt(2) / 2, 0, -Mathf.Sqrt(2) / 2));
        directionMappings.Add("south-east", new Vector3(Mathf.Sqrt(2) / 2, 0, -Mathf.Sqrt(2) / 2));
    }

    // Use LateUpdate instead so the movement is always processed after a DirectionKeyEvent
    void LateUpdate()
    {
        if(_curDirection != directionMappings["none"])
        {
            Vector3 translation = _curDirection * Speed * Time.deltaTime;
            /*
            the camera is rotated by a quaternion, so moving along the axis will not lead to
            the expected movement based on the direction vector. To move along the direction vector,
            need to apply the opposite of the quaternion's rotation to the translation vector, the two rotations
            will cancel each other out
            */
            translation = Quaternion.Inverse(Camera.main.transform.rotation) * translation;
            GameCamera.transform.Translate(translation);
        }
    }

    public void SetDirection(string direction)
    {
        _curDirection = directionMappings[direction];
    }
}

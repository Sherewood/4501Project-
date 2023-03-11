using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Internal Controller Class */
//Purpose: Control the game's camera

public class CameraController : MonoBehaviour
{
    public Camera GameCamera;

    //initial speed for the camera
    public float BaseSpeed;

    //maximum speed for the camera
    public float MaxSpeed;

    //accleration rate of the camera
    public float Acceleration;


    private Dictionary<string, Vector3> directionMappings;

    //current movement direction of the camera
    private Vector3 _curDirection;

    //previous movement direction of the camera
    private Vector3 _prevDirection;

    private Vector3 _curVelocity;


    void Start()
    {
        directionMappings = new Dictionary<string, Vector3>();
        _curVelocity = new Vector3();
        _curDirection = new Vector3();

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

    // Update is called once per frame
    void Update()
    {
        if(_curDirection != directionMappings["none"])
        {
            //set velocity to base speed if velocity is below base speed
            if (_curVelocity.magnitude < BaseSpeed)
            {
                _curVelocity = _curDirection * BaseSpeed;
            }
            //if direction changes, keep the same speed but in that direction
            //might be a bit awkward but w/e
            else if(_curDirection != _prevDirection)
            {
                _curVelocity = _curDirection * _curVelocity.magnitude;
            }
            else
            {
                //update velocity based on direction and acceleration
                _curVelocity += _curDirection * Acceleration * Time.deltaTime;

                //if speed exceeds maximum, flatten it to maximum
                if(_curVelocity.magnitude > MaxSpeed)
                {
                    _curVelocity /= _curVelocity.magnitude;
                    _curVelocity *= MaxSpeed;
                }
            }


            Vector3 translation = _curVelocity * Time.deltaTime;
            /*
            the camera is rotated by a quaternion, so moving along the axis will not lead to
            the expected movement based on the direction vector. To move along the direction vector,
            need to apply the opposite of the quaternion's rotation to the translation vector, the two rotations
            will cancel each other out
            */
            /*
             however, with camera rotated on y axis, the static up/down orientation is also not what we want
             Solution: Apply the y-axis rotation first, then apply the inverse rotation to rotate the translation by the y-axis rotation on its own
            */
            translation = Quaternion.Inverse(Camera.main.transform.rotation) * (Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, new Vector3(0, 1, 0)) * translation);
            
            GameCamera.transform.Translate(translation);
        }
        else
        {
            _curVelocity = Vector3.zero;
        }

        _prevDirection = _curDirection;
    }

    public void SetDirection(string direction)
    {
        _curDirection = directionMappings[direction];
    }
}

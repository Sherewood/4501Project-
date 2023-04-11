using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Purpose: Control a progress bar, keeping it facing the screen and facilitating updates to it
public class ProgressBarControl : MonoBehaviour
{
    //game's camera, used for keeping progess bar in view
    private GameObject _gameCamera;

    private MeshRenderer _progressBarMesh;

    //extra offset required to align the progress bar
    public float AlignmentAngleOffset;

    [Tooltip("Specify colors here to be set when needed.")]
    public List<Color> ColorOptions;

    void Awake()
    {
        //get the main camera
        _gameCamera = GameObject.Find("Main Camera");

        _progressBarMesh = gameObject.GetComponentInChildren<MeshRenderer>();

        //align progress bar with the camera
        AlignWithCamera();
    }

    //align the progress bar so it faces the camera
    private void AlignWithCamera()
    {
        //get the opposite of the y axis rotation of the camera
        float negativeCameraYAxisAngle = -_gameCamera.transform.rotation.eulerAngles.y;
        Quaternion yAxisRot = Quaternion.AngleAxis(negativeCameraYAxisAngle + AlignmentAngleOffset, Vector3.up);

        // rotate our progress bar such that it will face the opposite of the camera's y axis rotation, so it is facing straight at the player
        // need to take into account the parent transform (should be root!!!) to form the full rotation
        transform.rotation = Quaternion.RotateTowards(transform.parent.rotation, yAxisRot, 360);
    }

    void Update()
    {
        //keep aligned with camera
        AlignWithCamera();
    }

    //set the percentage of the progress bar that is filled
    public void SetPercentage(float percentage)
    {
        _progressBarMesh.material.SetFloat("_ProgressPct", percentage);
    }

    //set the primary and secondary colors of the bar
    public void SetColors(int colorIndex1, int colorIndex2)
    {
        Color color1 = ColorOptions[colorIndex1];
        Color color2 = ColorOptions[colorIndex2];
        if (_progressBarMesh != null)
        {
            _progressBarMesh.material.SetColor("_Color", color1);
            _progressBarMesh.material.SetColor("_EmptyColor", color2);
        }
    }
}

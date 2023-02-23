using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* Internal Controller Class */
//Purpose: Configures game settings, didn't include this in the proposal but whatever

public class GameConfigController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //force 60 fps to make my GPU stop whining
        Application.targetFrameRate = 60;
    }
}

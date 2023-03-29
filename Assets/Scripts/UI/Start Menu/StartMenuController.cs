using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

//purpose: Handle the functionality of the start menu
public class StartMenuController : MonoBehaviour
{
    //NOTE:
    //Add the scene you want the Start Menu to load to the build settings (File -> Build Settings) before playing!!!!
    //otherwise it will crash!!!!!11!!1111!
    [Tooltip("The scene that should be loaded when the user presses play")]
    public string GameSceneName;

    //starts the game by loading the game scene
    public void OnPlay()
    {
        if (GameSceneName.Equals(""))
        {
            Debug.LogError("Game scene is not defined.");
            return;
        }

        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    //exits the game by loading the exit scene
    public void OnExit()
    {
        EditorApplication.isPlaying = false;
    }
}

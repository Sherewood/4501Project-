using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* External Controller Class */
//Purpose: To handle mouse input by the user

public class MouseController : MonoBehaviour
{
    //constants
    private const int LEFT_MOUSE_BUTTON = 0;
    private const int RIGHT_MOUSE_BUTTON = 1;
    private const int NUM_MOUSE_BUTTONS_TRACKED = 2;

    //variables for tracking mouse click status
    private bool _mouseHeld;
    private int _heldMouseButton;

    //stored raycast of the initial mouse click
    private RaycastHit _initialMouseSelection;

    //game camera, used for raycasting
    public Camera gameCamera;

    //selection event callback - left click
    public SelectionEvent SelectionEvent;

    //mouse order event callback - right click
    public MouseOrderEvent MouseOrderEvent;

    //temporary - should be set in UI for dynamic adjustment based on what menus are open
    //denotes regions on the screen occupied by UI
    //format is (x1, y1, x2, y2), with all x,y in range [0,1] where 0 is one end of the screen, and 1 is the other end
    public List<Vector4> UIRegions;

    // Start is called before the first frame update
    void Start()
    {
        _mouseHeld = false;
        _heldMouseButton = -1;
    }

    // Update is called once per frame
    void Update()
    {
        //if mouse button not held down, check to see if any mouse buttons pressed
        if (!_mouseHeld)
        {
            for (int i = 0; i < NUM_MOUSE_BUTTONS_TRACKED; i++)
            {
                //if mouse button down, monitor mouse while held down using coroutine, and track held mouse button
                if (Input.GetMouseButtonDown(i))
                {
                    if (IsMouseInUIRegion(Input.mousePosition))
                    {
                        Debug.Log("MouseController - click done in UI-occupied region. Ignore.");
                        break;
                    }

                    StartCoroutine(MonitorPressedMouse(i));
                    _mouseHeld = true;
                    _heldMouseButton = i;

                    //perform the initial raycast immediately, so shift in camera does not alter intended target
                    Ray initalMouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);
                    //if nothing hit, or a UI element is hit, ignore the mouse click and continue doing whatever
                    if (!Physics.Raycast(initalMouseRay, out _initialMouseSelection))
                    {
                        //if raycast doesn't work (due to no object being hit), mouse controller has no reason to continue monitoring this mouse selection
                        Debug.Log("MouseController - No target for raycast on initial mouse click. Canceling mouse selection tracking...");
                        StopCoroutine(MonitorPressedMouse(i));
                        _mouseHeld = false;
                        _heldMouseButton = -1;
                    }


                    break;
                }
            }
        }
        //if mouse button held down, check if it is released
        else
        {
            if (Input.GetMouseButtonUp(_heldMouseButton))
            {
                //if released, stop the coroutine, and trigger corresponding callback using raycasted mouse position
                _mouseHeld = false;
                StopCoroutine(MonitorPressedMouse(_heldMouseButton));

                //selection callback
                if(_heldMouseButton == LEFT_MOUSE_BUTTON)
                {         
                    SelectionEvent.Invoke(_initialMouseSelection);
                }
                //order callback
                else if(_heldMouseButton == RIGHT_MOUSE_BUTTON)
                {
                    MouseOrderEvent.Invoke(_initialMouseSelection);
                }
            }
        }
    }

    //if mouse is in predefined 'UI region' of screen, then return true
    //no doubt in my mind there's a better way of handling this, and this will probably fail in the future, but should work for now.
    private bool IsMouseInUIRegion(Vector3 mousePosition)
    {

        Debug.Log(mousePosition);

        //convert mouse position so it is in range [0-1,0-1] with 0 being start of screen, 1 being end of screen
        //also invert mouse y coords because unity....
        Vector2 mousePosConverted = new Vector2(mousePosition.x / Screen.width, (Screen.height - mousePosition.y) / Screen.height);

        Debug.Log(mousePosConverted);

        //check if in boundary
        foreach (Vector4 UIRegion in UIRegions)
        {
            if((mousePosConverted.x >= UIRegion.x && mousePosConverted.x <= UIRegion.z) &&
                (mousePosConverted.y >= UIRegion.y && mousePosConverted.y <= UIRegion.w))
            {
                return true;
            }
        }
        return false;
    }


    //coroutine to monitor mouse while it is pressed
    //not in use for now.... will come into play for area selection...
    IEnumerator MonitorPressedMouse(int mouseButton)
    {
        yield return null;
    }
}

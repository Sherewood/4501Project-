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

    //distance threshold for when the initial mouse click and held mouse position are considered to be for an area selection
    private const float AREA_SELECTION_DISTANCE_THRESHOLD = 0.5f;

    //variables for tracking mouse click status
    private bool _mouseHeld;
    private int _heldMouseButton;

    //stored raycast of the initial mouse click
    private RaycastHit _initialMouseSelection;

    //stored raycast of latest mouse position while held down
    private RaycastHit _latestHeldMouseSelection;

    //true if an area selection is being made
    private bool _areaSelectionInProgress;

    //game camera, used for raycasting
    public Camera gameCamera;

    //selection event callback - left click
    public SelectionEvent SelectionEvent;

    //area selection event - left click and drag over a region
    public AreaSelectionEvent AreaSelectionEvent;

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
        _areaSelectionInProgress = false;
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
                Debug.Log("RELEASE");

                //if released, stop the coroutine, and trigger corresponding callback using raycasted mouse position
                _mouseHeld = false;
                StopCoroutine(MonitorPressedMouse(_heldMouseButton));

                //selection callback
                if(_heldMouseButton == LEFT_MOUSE_BUTTON)
                {
                    //area selection
                    if (_areaSelectionInProgress)
                    {
                        _areaSelectionInProgress = false;
                        AreaSelectionEvent.Invoke(_initialMouseSelection, _latestHeldMouseSelection);
                    }
                    else
                    {
                        SelectionEvent.Invoke(_initialMouseSelection);
                    }
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
        //convert mouse position so it is in range [0-1,0-1] with 0 being start of screen, 1 being end of screen
        //also invert mouse y coords because unity....
        Vector2 mousePosConverted = new Vector2(mousePosition.x / Screen.width, (Screen.height - mousePosition.y) / Screen.height);

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
        while (true)
        {
            //if not holding left mouse button down, nothing to see here
            if (mouseButton != LEFT_MOUSE_BUTTON)
            {
                yield break;
            }

            //perform a raycast with the latest mouse position
            Ray mouseRay = gameCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit oldRaycastHit = _latestHeldMouseSelection;

            //if nothing hit, or a UI element is hit, ignore the mouse for this frame
            if (IsMouseInUIRegion(Input.mousePosition) || !Physics.Raycast(mouseRay, out _latestHeldMouseSelection))
            {
                //revert to last saved position
                _latestHeldMouseSelection = oldRaycastHit;
                yield return null;
                continue;
            }

            //compare the distance between the world space positions of the two raycasts to determine if it is sufficient for an area selection
            float mouseDist = Vector3.Distance(_initialMouseSelection.point, _latestHeldMouseSelection.point);

            _areaSelectionInProgress = (mouseDist >= AREA_SELECTION_DISTANCE_THRESHOLD);

            yield return null;
        }
    }
}

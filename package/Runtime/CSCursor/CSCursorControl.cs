using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// CSScreenControl
// a general class for integrating touchscreen and mouse movement
// will allow for touches, swipes, touch and hold, joystick, etc
// created 8/11/23
// modified 9/1/24


public class CSCursorControl : MonoBehaviour
{
    [SerializeField] private CSCursorInterface cursorInterface;
    [SerializeField] private CSCursorManager cursor;
    [SerializeField] private float dragDistance = 0.01f; // fraction of screen width required before counting as dragging
    [SerializeField] private float cursorShortTime = 0.2f;
#if UNITY_EDITOR
    [Header("DEBUG VALUES")]
    [SerializeField] private bool fakeTouchWithMouse; // if true, pretend the mouse is a touchpad
#endif
    private bool button0Down;
    private bool button1Down;
    private Vector3 pressStartPos;
    private Vector3 cursorPos;
    private Vector3 mousePos;
    private bool pressStartOverUI;
    private bool dragging;
    private float cursorDuration;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    // button0: left click, or touchscreen
    // button1: right click
    private void UpdateCursor(Vector3 pos, bool button0, bool button1, bool mouseElseTouch)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
        bool active = false;

        if (button0Down)
        {
            // button 0 was down previously, so check for button release or cursor drag
            if (button0)
            {
                cursorDuration += Time.deltaTime;
                if (dragging)
                {
                    Vector3 drag = pos - cursorPos;
                    if (drag.magnitude > 1f)
                    {
                        if (cursorInterface)
                            cursorInterface.CursorDrag(pos, drag, 0, mouseElseTouch, pressStartOverUI);
                        active = true;
                    }
                }
                else
                {
                    // button held, check for dragging
                    Vector3 offset = pos - pressStartPos;

                    if (offset.magnitude > Screen.width * dragDistance)
                    {
                        dragging = true;
                        if (cursorInterface)
                            cursorInterface.CursorDrag(pos, pos - cursorPos, 0, mouseElseTouch, pressStartOverUI);
                        active = true;
                    }
                    else if (cursorDuration > cursorShortTime)
                    {
                        if (cursorInterface)
                            cursorInterface.CursorHold(pos, 0, mouseElseTouch, pressStartOverUI);
                        active = true;
                    }
                }
            }
            else
            {
                // button released, check if it was a short or long press
                button0Down = false;
                // if the press started over the UI don't process click events - Unity UI will handle it
                if (dragging)
                {
                    if (cursorInterface)
                        cursorInterface.CursorDragRelease(pos, 0, mouseElseTouch, pressStartOverUI);
                    dragging = false;
                    active = true;
                }
                else if (cursorDuration > cursorShortTime)
                {
                    if (cursorInterface)
                        cursorInterface.CursorSlowRelease(pos, 0, mouseElseTouch, pressStartOverUI);
                    active = true;
                }
                else
                {
                    if (cursorInterface)
                        cursorInterface.CursorFastRelease(pos, 0, mouseElseTouch, pressStartOverUI);
                    active = true;
                }
                cursorDuration = 0;
            }
        }
        else if (button1Down)
        {
            // button 1 was down previously, so check for button release or cursor drag
            if (button1)
            {
                cursorDuration += Time.deltaTime;
                if (dragging)
                {
                    Vector3 drag = pos - cursorPos;
                    if (drag.magnitude > 1f)
                    {
                        if (cursorInterface)
                            cursorInterface.CursorDrag(pos, drag, 1, mouseElseTouch, pressStartOverUI);
                        active = true;
                    }
                }
                else
                {
                    // button held, check for dragging
                    Vector3 offset = pos - pressStartPos;

                    if (offset.magnitude > Screen.width * dragDistance)
                    {
                        dragging = true;
                        if (cursorInterface)
                            cursorInterface.CursorDrag(pos, pos - cursorPos, 1, mouseElseTouch, pressStartOverUI);
                        active = true;
                    }
                    else if (cursorDuration > cursorShortTime)
                    {
                        if (cursorInterface)
                            cursorInterface.CursorHold(pos, 1, mouseElseTouch, pressStartOverUI);
                        active = true;
                    }
                }
            }
            else
            {
                // button released, check if it was a short or long press
                button1Down = false;
                // if the press started over the UI don't process click events - Unity UI will handle it
                if (dragging)
                {
                    if (cursorInterface)
                        cursorInterface.CursorDragRelease(pos, 1, mouseElseTouch, pressStartOverUI);
                    dragging = false;
                    active = true;
                }
                else if (cursorDuration > cursorShortTime)
                {
                    if (cursorInterface)
                        cursorInterface.CursorSlowRelease(pos, 1, mouseElseTouch, pressStartOverUI);
                    active = true;
                }
                else
                {
                    if (cursorInterface)
                        cursorInterface.CursorFastRelease(pos, 1, mouseElseTouch, pressStartOverUI);
                    active = true;
                }
                cursorDuration = 0;
            }
        }
        else
        {
            if (button0)
            {
                List<RaycastResult> overUI = CSUtils.GetUIObjects(pos);
                pressStartOverUI = (overUI.Count > 0);

                pressStartPos = pos;
                button0Down = true;
                cursorDuration = 0;
                if (cursorInterface)
                    cursorInterface.CursorPress(pos, 0, mouseElseTouch, pressStartOverUI);
                active = true;
            }
            else if (button1)
            {
                List<RaycastResult> overUI = CSUtils.GetUIObjects(pos);
                pressStartOverUI = (overUI.Count > 0);

                pressStartPos = pos;
                button1Down = true;
                cursorDuration = 0;
                if (cursorInterface)
                    cursorInterface.CursorPress(pos, 1, mouseElseTouch, pressStartOverUI);
                active = true;
            }
        }

        // TODO: detect when the cursor is over an interactable for the mouse cursor variation
        cursor.CursorUpdate(pos, button0, button1, mouseElseTouch, false);
        if (cursorInterface)
        {
            if (!active && !mouseElseTouch)
                cursorInterface.CursorGone(pos); // touch just ended, send a notice in case it matters
            else
                cursorInterface.CursorPos(pos, mouseElseTouch, active);
        }
        cursorPos = pos;

    }

    void Update()
    {
        bool mouseActive = false;
        if (Input.mousePresent)
        {
            // if there is a mouse, only update it if it is active
            // we want to be updating this even if touches are in play (overriding mouse input) so the mouse doesn't immediately take over when touch is released
            if (mousePos != Input.mousePosition)
            {
                mouseActive = true;
                mousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                mouseActive = true;
        }
        // check touches first
        if (Input.touchSupported)
        {
            if (Input.touches.Length > 0)
            {
                UpdateCursor(Input.touches[0].position, true, false, false);
                return;
            }
            else if (button0Down || button1Down) // send tap release
            {
                UpdateCursor(cursorPos, false, false, false);
                return;
            }
        }
        // no touches to handle, so check for a mouse
#if UNITY_EDITOR
        if (Input.mousePresent && fakeTouchWithMouse)
        {
            if (Input.GetMouseButton(0))
                UpdateCursor(Input.mousePosition, true, false, false);
            else if (button0Down || button1Down) // send "tap" release
                UpdateCursor(cursorPos, false, false, false);
        }
        else
#endif
        // only update the mouse if there has been any change
        if (mouseActive || button0Down || button1Down)
            UpdateCursor(Input.mousePosition, Input.GetMouseButton(0), Input.GetMouseButton(1), true);
    }
}

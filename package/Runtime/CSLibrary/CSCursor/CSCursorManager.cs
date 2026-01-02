using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CSCursorManager
// manages the cursor itself, updating state and visibility
// created: 9/11/23

public class CSCursorManager : MonoBehaviour
{
    [Header("Mouse settings")]
    [SerializeField] private Image cursorMouseImage;
    [SerializeField] private Sprite cursorMouse;
    [SerializeField] private Sprite cursorMouseHighlight;
    [SerializeField] private Sprite cursorMouseClicked;
    [Header("Touchscreen settings")]
    [SerializeField] private float cursorStayTime = 0.5f;
    [SerializeField] private Image cursorTouchImage;
    [SerializeField] private Sprite cursorTouch;
    [SerializeField] private Sprite cursorTouchFade;
    private float cursorInactiveTime;
    private bool hidden;
    private bool mouseElseTouchCursor;

    public void CursorUpdate(Vector3 pos, bool button1, bool button2, bool mouseElseTouch, bool highlight)
    {
        transform.position = pos;
        mouseElseTouchCursor = mouseElseTouch;
        if (mouseElseTouch)
        {
            cursorMouseImage.enabled = true;
            cursorTouchImage.enabled = false;
            if (button1 || button2)
                cursorMouseImage.sprite = cursorMouseClicked;
            else if (highlight)
                cursorMouseImage.sprite = cursorMouseHighlight;
            else
                cursorMouseImage.sprite = cursorMouse;
        }
        else
        {
            cursorMouseImage.enabled = false;
            cursorTouchImage.enabled = true;
            cursorTouchImage.sprite = cursorTouch;
            cursorInactiveTime = 0;
        }
        if (hidden)
            hidden = false;
    }

    private void LateUpdate()
    {
        // touchscreen cursor will disappear after a delay
        if (!hidden)
        {
            if (!mouseElseTouchCursor)
            {
                cursorInactiveTime += Time.deltaTime;
                if (cursorInactiveTime > cursorStayTime)
                {
                    cursorTouchImage.enabled = false;
                    hidden = true;
                }
                else if (cursorInactiveTime > cursorStayTime * 0.5f)
                {
                    cursorTouchImage.sprite = cursorTouchFade;
                }
            }
        }
    }
}

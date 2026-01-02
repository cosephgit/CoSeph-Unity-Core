using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSCursorInterface
// an abstract class which takes input from CSCursorControl
// use as a parent class for your project's individual input handling needs
// created 8/11/23
// modified 9/11/23

public abstract class CSCursorInterface : MonoBehaviour
{
    // active means the cursor has done anything else this frame (e.g. a key is pressed, dragging is happening, etc)
    public virtual void CursorPos(Vector3 pos, bool mouseElseTouch, bool active) { }
    public virtual void CursorGone(Vector3 pos) { }
    public virtual void CursorPress(Vector3 pos, int key, bool mouseElseTouch, bool startOverUI) { }
    public virtual void CursorHold(Vector3 pos, int key, bool mouseElseTouch, bool startOverUI) { }
    public virtual void CursorDrag(Vector3 pos, Vector3 move, int key, bool mouseElseTouch, bool startOverUI) { }
    public virtual void CursorDragRelease(Vector3 pos, int key, bool mouseElseTouch, bool startOverUI) { }
    public virtual void CursorFastRelease(Vector3 pos, int key, bool mouseElseTouch, bool startOverUI) { }
    public virtual void CursorSlowRelease(Vector3 pos, int key, bool mouseElseTouch, bool startOverUI) { }
}

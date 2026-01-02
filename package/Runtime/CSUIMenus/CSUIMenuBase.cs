using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSUIMenuBase
// provides an interface for all menu options
// created 23/2/24
// modified 23/2/24

public class CSUIMenuBase : MonoBehaviour
{
    // inGame means in the actual gameplay screen (NOT the shuttle)
    public virtual void Initialise(bool inGame)
    {

    }

    public virtual void Active(bool active)
    {
        gameObject.SetActive(active);
    }
}

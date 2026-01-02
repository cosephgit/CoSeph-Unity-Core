using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSUICodeInterface
// generic interface for CS UI elements to connect with and receive inputs from interactions
// created 15/1/25
// modified 15/1/25

public abstract class CSUICodeInterface : MonoBehaviour
{
    public virtual void SelectValue(int value)
    {
        Debug.LogWarning("CSUICodeInterface.SelectIndex integer virtual accessed! This should be overriden if you want to use it!");
    }
    public virtual void SelectValue(bool value)
    {
        Debug.LogWarning("CSUICodeInterface.SelectIndex boolean virtual accessed! This should be overriden if you want to use it!");
    }
    public virtual void SelectValue(string value)
    {
        Debug.LogWarning("CSUICodeInterface.SelectIndex string virtual accessed! This should be overriden if you want to use it!");
    }
    public virtual void UIPing()
    {
        // play a sound with this if you want
    }
}

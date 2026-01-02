using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSUIRadioSelector : MonoBehaviour
{
    [SerializeField] private CSUIRadioButton[] toggles;
    [SerializeField] private CSUICodeInterface output;
    private int valueCurrent;
    private bool initDone = false;

    public void Initialise(int valueDefault)
    {
        for (int i = 0; i < toggles.Length; i++)
            toggles[i].Initialise(this, i);
        toggles[valueDefault].SetOn();
        //PressToggle(GameManager.instance.screenUIScale);
        initDone = true;
    }

    private void SelectToggle(int index)
    {
        valueCurrent = index;
        for (int i = 0; i < toggles.Length; i++)
        {
            if (index != i)
                toggles[i].SetOff();
        }
        //Debug.Log("Radioselector value is now " + valueCurrent);
        // TODO report new valueCurrent to game manager etc
        if (initDone)
            output.SelectValue(index);
    }

    public void PressToggle(System.Int32 index)
    {
        if (index < 0)
        {
            Debug.LogError("Toggle has PressToggle index " + index);
            return;
        }
        if (index >= toggles.Length)
        {
            Debug.LogError("Toggle has PressToggle index " + index);
            return;
        }
        SelectToggle(index);
        if (initDone)
            AudioManager.instance.PlayTogglePing();
    }
}

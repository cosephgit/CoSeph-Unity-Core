using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSUIRadioButton : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    private CSUIRadioSelector buttonSelector;
    private int buttonIndex;

    public void Initialise(CSUIRadioSelector selector, int index)
    {
        buttonSelector = selector;
        buttonIndex = index;
    }

    public void Pressed(System.Boolean on)
    {
        if (buttonSelector)
        {
            if (toggle.isOn)
            {
                buttonSelector.PressToggle(buttonIndex);
                toggle.interactable = false;
            }
        }
    }

    public void SetOn()
    {
        toggle.isOn = true;
        // this will trigger Pressed() above
    }
    public void SetOff()
    {
        toggle.isOn = false;
        toggle.interactable = true;
    }
}

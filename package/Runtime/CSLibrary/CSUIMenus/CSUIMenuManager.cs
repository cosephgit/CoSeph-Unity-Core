using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CSUIMenuManager
// generic handler for a selection of buttons and menu panes
// only handles the basic menu switching functions
// created 18/2/24
// modified 23/2/24

public abstract class CSUIMenuManager : CSUIMenuBase
{
    [Header("Main menu options and buttons for them - size and ordering must match")]
    [SerializeField] protected Button[] menuButtons;
    [SerializeField] protected CSUIMenuBase[] menuScreens;
    protected int screenCurrent;

    // Interface for button clicks - takes the index of the button (in the same order as the arrays of buttons defined here)
    public virtual void ButtonClick(System.Int32 index)
    {
        screenCurrent = index;
        for (int i = 0; i < menuButtons.Length; i++)
            menuButtons[i].interactable = (i != index);
        for (int i = 0; i < menuScreens.Length; i++)
            menuScreens[i].Active(i == index);
    }
}

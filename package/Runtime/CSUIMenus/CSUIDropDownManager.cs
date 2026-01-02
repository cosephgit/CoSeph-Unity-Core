using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// CSUIDropDownManager
// Extends functionality of the dropdown menu
// created 16/1/25
// modified 16/1/25
/*
Notes for usage:
https://discussions.unity.com/t/trigger-sounds-when-opening-closing-dropdown/181485

When you create a new Dropdown it comes preloaded with a lot of children UI elements that make up the actual Dropdown menu portion. OnValueChanged for the initial Dropdown only activates when you select a new value but these children UI elements have a little bit more going on with them:

Dropdown > Template > OnValueChanged triggers whenever you scroll the scrollbar.

Dropdown > Template > Viewport > Content > Item > OnValueChanged triggers whenever you open the dropdown, but for some reason it will trigger twice most of the time and as far as I can tell its random whether it does so.

Dropdown > Template > Scrollbar > OnValueChanged triggers whenever you open the dropdown OR scroll the scrollbar.

I couldn稚 find a single OnValueChanged which triggers when you exit the dropdown either by selecting the same option or just pressing the initial button again.

The sound I知 working with works for both the scrollbar and the dropdown open but if you need them to be different I知 afraid there isn稚 a good answer. I知 using Unity 5.6.0f3 so maybe what I can only assume is a bug with the Item OnValueChanges is fixed in the future. I知 not the most experienced Unity developer myself so I知 not entirely sure why these interactions work as they do. I merely experimented and wanted to give a weak answer so that a more seasoned Unity veteran could fill in the gaps.
*/
[RequireComponent(typeof(CSUICodeInterface))]
[RequireComponent(typeof(TMP_Dropdown))]
public class CSUIDropDownManager : MonoBehaviour
{
    [SerializeField] private CSUICodeInterface output;
    [SerializeField] private TMP_Dropdown dropdown;

    public void SetDropdown(string[] options, int value)
    {
        dropdown.options.Clear();
        foreach (string option in options)
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        dropdown.value = value;
        dropdown.RefreshShownValue();
    }

    public void SelectValue(System.Int32 value)
    {
        output.SelectValue(value);
        output.UIPing();
    }

    public void OpenDropdown(System.Boolean open)
    {
        output.UIPing();
    }
}

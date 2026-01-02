using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// CSUISliderIcon
// manages a slider which operates with icons instead of a bar
// simple bar which is just for showing data without extra animations/effects

public class CSUISliderIcon : MonoBehaviour
{
    [SerializeField] private Image[] icons;
    [SerializeField] private Sprite spriteOn;
    [SerializeField] private Sprite spriteHalf;
    [SerializeField] private Sprite spriteOff;
    [SerializeField] private TextMeshProUGUI textOverflow;
    public int value { set { SetValue(value); } }

    public void SetValue(float valueSet)
    {
        int iconFill = Mathf.CeilToInt(valueSet);
        bool half = (iconFill - valueSet >= 0.5f);

        for (int i = 0; i < icons.Length; i++)
        {
            if (half && i == iconFill - 1)
                icons[i].sprite = spriteHalf;
            else if (i < iconFill)
                icons[i].sprite = spriteOn;
            else
                icons[i].sprite = spriteOff;
        }
        if (textOverflow)
        {
            if (iconFill > icons.Length)
            {
                textOverflow.text = "+" + (iconFill - icons.Length);
                textOverflow.enabled = true;
            }
            else
                textOverflow.enabled = false;
        }
    }
    // set the value with a float in the range 0...1
    public void SetFloat(float valueFloat)
    {
        SetValue(icons.Length * valueFloat);
    }
    public void ResetTint()
    {
        for (int i = 0; i < icons.Length; i++)
            icons[i].color = Color.white;
    }
    // sets the icon tint on the indicated range
    public void SetTint(Color colorSet, int startInc, int endExc)
    {
        if (startInc < 0)
            return;
        for (int i = startInc; i < endExc; i++)
        {
            if (i < icons.Length)
                icons[i].color = colorSet;
        }
    }

    public int GetMax()
    {
        return icons.Length;
    }
}

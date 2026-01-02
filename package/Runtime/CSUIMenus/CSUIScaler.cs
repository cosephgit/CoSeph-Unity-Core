using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CSUIScaler
// detected and managed by the CSUIScaleController
// place this on the same gameobject as a CanvasScaler
// created 15/1/25
// modified 15/1/25

[RequireComponent(typeof(CanvasScaler))]
public class CSUIScaler : MonoBehaviour
{
    private CanvasScaler scaler;
    private float scaleBaseX;
    private float scaleBaseY;

    public void Initialise()
    {
        scaler = GetComponent<CanvasScaler>();
        scaleBaseX = scaler.referenceResolution.x;
        scaleBaseY = scaler.referenceResolution.y;
    }

    public void SetScale(float scale)
    {
        float scaleSet = 1f / Mathf.Clamp(scale, 0.1f, 10f);
        Vector2 resolutionNew = new Vector2(scaleBaseX * scaleSet, scaleBaseY * scaleSet);

        scaler.referenceResolution = resolutionNew;
    }
}

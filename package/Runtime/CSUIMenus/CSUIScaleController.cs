using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSUIScaleController
// finds and manages all CSUIScaler components in the scene
// singleton
// created 15/1/25
// modified 15/1/25

public class CSUIScaleController : MonoBehaviour
{
    public static CSUIScaleController instance;
    private CSUIScaler[] scalers;

    private void Awake()
    {
        if (instance)
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        instance = this;

        scalers = FindObjectsByType<CSUIScaler>(FindObjectsSortMode.None);
        for (int i = 0; i < scalers.Length; i++)
            scalers[i].Initialise();
    }

    public void SetCanvasScale(float scale)
    {
        for (int i = 0; i < scalers.Length; i++)
            scalers[i].SetScale(scale);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSSimpleToggle
// simple script for an object which needs to goggle on and off once triggered
// created 26/2/25
// modified 26/2/25

public class CSSimpleToggle : MonoBehaviour
{
    [SerializeField] private GameObject[] toggleObjects;
    [SerializeField] private float offTime = 1f;
    [SerializeField] private float onTime = 1f;
    [SerializeField] private bool startActive = false;
    private Coroutine toggleRoutine = null;

    private void Start()
    {
        if (startActive)
            StartToggle();
    }

    public void StartToggle()
    {
        if (toggleRoutine != null)
            return;

        toggleRoutine = StartCoroutine(Toggler());
    }

    public void StopToggle()
    {
        if (toggleRoutine != null)
        {
            StopCoroutine(toggleRoutine);
            toggleRoutine = null;
        }
    }

    private IEnumerator Toggler()
    {
        while (true)
        {
            for (int i = 0; i < toggleObjects.Length; i++)
                toggleObjects[i].SetActive(true);

            yield return new WaitForSeconds(onTime);

            for (int i = 0; i < toggleObjects.Length; i++)
                toggleObjects[i].SetActive(false);

            yield return new WaitForSeconds(offTime);
        }
    }
}

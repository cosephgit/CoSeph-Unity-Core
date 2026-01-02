using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class CSSplashScreenManager : MonoBehaviour
{
    [SerializeField] private CSSplashScreen[] screens;
    [FormerlySerializedAs("camera")][SerializeField] private Camera cameraSplash;
    [SerializeField] private float delayInitial = 0.1f;
    private float screenTimeTaken = 0f;
    private int screenCurrent = -1;

    private void StartVideo(int index)
    {
        if (screenCurrent >= 0 && screenCurrent < screens.Length)
            screens[screenCurrent].Hide();
        if (index < screens.Length)
        {
            screenTimeTaken = 0f;
            screenCurrent = index;
            cameraSplash.backgroundColor = screens[index].bgColor;
            screens[index].Show();
        }
        else
        {
            // videos finished, scene ends!
            SceneManager.LoadScene(Global.SCENEMENU);
        }
    }

    private void Update()
    {
        screenTimeTaken += Time.deltaTime;
        if (screenCurrent < 0)
        {
            if (screenTimeTaken > delayInitial)
                StartVideo(0);
        }
        else if (screenCurrent < screens.Length)
        {
            bool screenDone = false;
            if (Input.GetButtonDown("Submit"))
                screenDone = true;
            else if (Input.GetButtonDown("Cancel"))
                screenDone = true;
            else if (Input.GetMouseButtonDown(0))
                screenDone = true;
            else if (Input.GetMouseButtonDown(1))
                screenDone = true;
            else if (screens[screenCurrent].IsFinished(screenTimeTaken))
                screenDone = true;
            else if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.touches[i].phase == TouchPhase.Began)
                        screenDone = true;
                }
            }

            if (screenDone)
                StartVideo(screenCurrent + 1);
        }
    }
}

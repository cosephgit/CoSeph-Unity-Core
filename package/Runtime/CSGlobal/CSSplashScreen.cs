using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CSSplashScreen : MonoBehaviour
{
    [SerializeField] private VideoPlayer video;
    [SerializeField] private CSFMODVideoAudio videoAudio;
    [SerializeField] private GameObject content;
    [SerializeField] private float displayTime = 4f;
    public Color bgColor;

    public void Show()
    {
        if (content)
            content.SetActive(true);
        if (video)
        {
            if (videoAudio)
            {
                // need to let the FMOD audio system set up handle the playing
                videoAudio.Play();
            }
            else
            {
                video.Play();
            }
        }
    }

    public void Hide()
    {
        if (video)
        {
            if (videoAudio)
            {
                videoAudio.Stop();
            }
            else
            {
                video.Stop();
            }
        }    
        gameObject.SetActive(false);
    }

    private bool PlaybackActive()
    {
        if (video)
        {
            if (videoAudio && !videoAudio.IsPlayerReady())
                return true;
            if (video.isPlaying)
                return true;
        }
        return false;
    }

    public bool IsFinished(float timePassed)
    {
        if (timePassed > displayTime)
        {
            if (PlaybackActive()) return false;
            return true;
        }
        return false;
    }
}

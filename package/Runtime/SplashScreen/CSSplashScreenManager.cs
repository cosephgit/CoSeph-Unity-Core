using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoSeph.Core
{
    /// <summary>
    /// Place this class in a splash screen scene.
    /// It will iterate through a series of splash screens, then advance to the next scene when complete.
    /// Includes time delayed splash screens as well as a input triggered screen skipping.
    /// </summary>
    public class CSSplashScreenManager : MonoBehaviour
    {
        [Header("The sequence of screens to display")]
        [SerializeField] private CSSplashScreen[] _screens;
        [Header("Camera for background tinting")]
        [SerializeField] private Camera _cameraSplash;
        [Header("Sequence ending")]
        [SerializeField] private CSSceneReference _sceneNext;
        private float _screenTimeTaken = 0f;
        private int _screenCurrent = -1;

        public void StartSequence()
        {
            StartVideo(0);
        }

        public void ProgressSequence()
        {
            if (CanSkipCurrentScene())
            {
                StartVideo(_screenCurrent + 1);
            }
        }

        private void StartVideo(int index)
        {
            // close the current splash screen
            if (_screenCurrent >= 0 && _screenCurrent < _screens.Length)
                _screens[_screenCurrent].Hide();

            if (index < _screens.Length)
            {
                // show the next splash screen
                _screenTimeTaken = 0f;
                _screenCurrent = index;
                _cameraSplash.backgroundColor = _screens[index].BGColor;
                _screens[index].Show();
            }
            else
            {
                // videos finished, scene ends!
                if (_sceneNext)
                {
                    if (_sceneNext.IsDefined())
                        SceneManager.LoadScene(_sceneNext.SceneName);
                    else
                        Debug.LogWarning($"CSSceneReference {_sceneNext} is undefined");
                }
            }
        }

        public bool IsPlaying()
        {
            if (_screenCurrent >= 0
                && _screenCurrent < _screens.Length)
                return true;

            return false;
        }
        public bool IsFinished()
        {
            return (_screenCurrent >= _screens.Length);
        }

        private bool CanSkipCurrentScene()
        {
            return (_screens[_screenCurrent].CanContinue(_screenTimeTaken));
        }

        private void Update()
        {
            if (IsPlaying())
            {
                _screenTimeTaken += Time.deltaTime;

                if (_screens[_screenCurrent].AutoContinue(_screenTimeTaken))
                    StartVideo(_screenCurrent + 1);
            }
        }
    }
}
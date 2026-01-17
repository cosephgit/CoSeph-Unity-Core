using System.Collections;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Manages a sequence of splash screens (images or videos).
    /// Displays each screen in order, optionally allowing input-based skipping,
    /// then transitions to a target scene when complete.
    ///
    /// Call <see cref="StartSequence"/> to begin playback,
    /// or use _startAutomatically to begin on Start().
    /// </summary>
    public class CSSplashScreenManager : MonoBehaviour
    {
        [Header("The sequence of screens to display")]
        [SerializeField] private CSSplashScreen[] _screens;
        [Header("Camera for background tinting")]
        [SerializeField] private Camera _cameraSplash;
        [Header("Sequence starting and ending")]
        [SerializeField] private bool _startAutomatically;
        [SerializeField] private CSSceneReference _sceneNext;
        private float _screenTimeElapsed = 0f;
        private int _screenIndexCurrent = -1;
        private Coroutine _splashSequence;
        private bool _splashSkipRequested;

        private void Awake()
        {
            if (_screens == null)
            {
                // set an empty array defensively
                _screens = new CSSplashScreen[0];
                Debug.LogWarning("CSSplashScreenManager _screens is null", this);
            }
            else if (_screens.Length == 0)
            {
                Debug.LogWarning("CSSplashScreenManager _screens.Length is zero", this);
            }
            _screenIndexCurrent = -1;
            for (int i = 0; i < _screens.Length; i++)
            {
                if (_screens[i])
                    _screens[i].Hide();
            }
            _screenTimeElapsed = 0f;
        }

        private void Start()
        {
            if (_startAutomatically)
                StartSequence();
        }

        /// <summary>
        /// Starts the splash screen sequence from the first screen.
        /// Has no effect if the sequence is already playing or finished.
        /// Call <see cref="ResetSequence"/> to restart after completion.
        /// </summary>
        public void StartSequence()
        {
            if (gameObject.activeInHierarchy)
            {
                if (IsWaiting())
                {
                    for (int i = 0; i < _screens.Length; i++)
                    {
                        if (_screens[i])
                            _screens[i].Prepare();
                    }

                    _splashSkipRequested = false;
                    _splashSequence = StartCoroutine(SplashSequence());
                }
            }
            else
                Debug.LogError("CSSplashScreenManager.StartSequence called while inactive", this);
        }

        /// <summary>
        /// May be called externally (e.g. input handlers) to immediately
        /// advance the sequence if the current splash allows skipping.
        /// </summary>
        public void ProgressSequence()
        {
            if (IsPlaying())
                _splashSkipRequested = true;
        }

        public void ResetSequence()
        {
            if (IsFinished() || IsPlaying())
            {
                _splashSkipRequested = false;
                _screenIndexCurrent = -1;
                for (int i = 0; i < _screens.Length; i++)
                {
                    if (_screens[i])
                        _screens[i].Hide();
                }
                _screenTimeElapsed = 0f;
                if (_splashSequence != null)
                {
                    StopCoroutine(_splashSequence);
                    _splashSequence = null;
                }
            }
        }

        // returns true on invalid data to avoid getting stuck
        private bool IsSplashScreenReady(int index)
        {
            if (index < 0)
            {
                Debug.LogWarning($"CSSplashManager.IsSplashScreenReady called with invalid index {index}.");
                return true;
            }
            else if (index < _screens.Length)
            {
                if (_screens[index])
                {
                    return _screens[index].CanStart;
                }
                else
                {
                    // if null, return true to allow the sequence to continue
                    Debug.LogWarning($"CSSplashManager screen index {index} is null.");
                    return true;
                }
            }
            else
            {
                Debug.LogWarning($"CSSplashManager.IsSplashScreenReady called with invalid index {index}.");
                return true;
            }
        }

        private void ShowSplashScreen(int index)
        {
            _splashSkipRequested = false;
            if (IsFinished())
            {
                Debug.LogError("CSSplashScreenManager.ShowSplashScreen called after sequence is finished", this);
                return;
            }

            // close the current splash screen
            if (_screenIndexCurrent >= 0 && _screenIndexCurrent < _screens.Length
                && _screens[_screenIndexCurrent] != null)
                _screens[_screenIndexCurrent].Hide();

            if (index < 0)
            {
                Debug.LogWarning($"CSSplashScreenManager.ShowSplashScreen called with invalid index {index}", this);
            }
            else if (index < _screens.Length)
            {
                _screenIndexCurrent = index;

                if (_screens[index])
                {
                    // show the next splash screen
                    _screenTimeElapsed = 0f;
                    if (_cameraSplash)
                        _cameraSplash.backgroundColor = _screens[index].BGColor;
                    else
                        Debug.LogWarning("CSSsplashScreenManager _cameraSplash is null", this);
                    _screens[index].Show();
                }
                else
                    Debug.LogWarning($"CSSplashScreenManager _screens[{index}] is null", this);
            }
            else // any index >= _screens.Length is treated as sequence complete
            {
                SequenceComplete();
            }
        }

        // Allow custom completion behaviour is required
        public virtual void SequenceComplete()
        {
            // splash screens finished, progress to the next scene, if set
            _splashSkipRequested = false;
            _screenIndexCurrent = _screens.Length;
            if (_sceneNext)
            {
                if (!_sceneNext.LoadScene())
                    Debug.LogWarning($"CSSceneReference {_sceneNext} failed to load", this);
            }
        }

        /// <summary>
        /// Returns true when in initial state.
        /// </summary>
        public bool IsWaiting()
        {
            return (_screenIndexCurrent < 0
                && _splashSequence == null);
        }

        /// <summary>
        /// Returns true while a splash screen is currently playing.
        /// </summary>
        public bool IsPrepping()
        {
            return (_screenIndexCurrent < 0
                && _splashSequence != null);
        }
        /// <summary>
        /// Returns true while a splash screen is currently playing.
        /// </summary>
        public bool IsPlaying()
        {
            return (_screenIndexCurrent >= 0
                && _screenIndexCurrent < _screens.Length);
        }
        /// <summary>
        /// Returns true once all splash screens have completed.
        /// </summary>
        public bool IsFinished()
        {
            return (_screenIndexCurrent >= _screens.Length);
        }

        /// <summary>
        /// Returns true if if enough time has elapsed to allow immediate skipping.
        /// Always true if _screens[_screenIndexCurrent] is null.
        /// </summary>
        private bool CanSkipCurrentSplash()
        {
            if (IsPlaying())
            {
                if (_screenIndexCurrent + 1 < _screens.Length)
                {
                    if (!IsSplashScreenReady(_screenIndexCurrent + 1))
                        return false;
                }
                if (_screens[_screenIndexCurrent])
                    return (_screens[_screenIndexCurrent].CanSkipSplash(_screenTimeElapsed));
                else
                    return true;
            }
            return false;
        }

        private IEnumerator SplashSequence()
        {
            // wait for the first screen to be ready
            while (!IsSplashScreenReady(0))
                yield return new WaitForEndOfFrame();

            ShowSplashScreen(0);

            while (IsPlaying())
            {
                _screenTimeElapsed += Time.deltaTime;

                if (_splashSkipRequested && !CanSkipCurrentSplash())
                    _splashSkipRequested = false;

                if (!_screens[_screenIndexCurrent]
                    || _screens[_screenIndexCurrent].ShouldAutoAdvance(_screenTimeElapsed)
                    || _splashSkipRequested)
                {
                    if (_screenIndexCurrent + 1 < _screens.Length)
                    {
                        while (!IsSplashScreenReady(_screenIndexCurrent + 1))
                            yield return new WaitForEndOfFrame();

                        ShowSplashScreen(_screenIndexCurrent + 1);
                    }
                    else
                        SequenceComplete();
                }

                yield return null;
            }
            _splashSequence = null;
        }

        private void OnDisable()
        {
            ResetSequence();
        }
    }
}
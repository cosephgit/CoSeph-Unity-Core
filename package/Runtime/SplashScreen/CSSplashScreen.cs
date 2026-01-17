using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace CoSeph.Core
{
    /// <summary>
    /// A single splash screen with video and content references.
    /// Managed by the CSSplashScreenManager.
    /// </summary>
    public class CSSplashScreen : MonoBehaviour
    {
        private enum SplashState
        {
            Idle,
            Preparing,
            Prepared,
            Failed
        }

        [Header("Components are optional and combinable")]
        [SerializeField] private VideoPlayer _video;
        [SerializeField] private GameObject _content;
        [SerializeField, Tooltip("The color setting for the camera background")] private Color _bgColor;
        [Header("Timing for this screen")]
        [SerializeField, Tooltip("Minimum time that must pass before the screen can be skipped manually.")]
        private float _minContinue = 0f;
        [SerializeField, Tooltip("Minimum time before auto-continuing. " +
    "If a video is present, auto-continue will also wait for playback to finish.")]
        private float _autoContinue = 4f;
        [SerializeField] private float _failTimeout = 5f;
        private SplashState _splashState;
        //private bool _isPreparing;
        //private bool _isPrepared;
        //private bool _isFailed;
        private Coroutine _timeOut;
        public Color BGColor => _bgColor;
        // treat failed as ready to allow sequence to progress
        public bool CanStart => (_splashState == SplashState.Prepared || _splashState == SplashState.Failed);


        /// <summary>
        /// Call in advance to warm up video
        /// Static images will immediately count as prepared
        /// </summary>
        public virtual void Prepare()
        {
            if (!isActiveAndEnabled)
            {
                Debug.LogError($"Screens {name} can not prepare - inactive.");
                return;
            }
            if (_splashState == SplashState.Preparing)
            {
                Debug.Log($"Screens {name} is already preparing.");
                return;
            }
            if (_splashState == SplashState.Prepared)
            {
                Debug.Log($"Screens {name} is already prepared.");
                return;
            }
            if (_video)
            {
                _video.enabled = true;
                _video.prepareCompleted += VideoReady;
                _video.errorReceived += VideoFailed;
                _video.Prepare();
                if (_timeOut != null)
                    StopCoroutine(_timeOut);
                _timeOut = StartCoroutine(VideoTimeout());
                _splashState = SplashState.Preparing;
            }
            else
                _splashState = SplashState.Prepared;
        }

        /// <summary>
        /// Show this splash page.
        /// If there is video and it is not prepared, it will play anyway - may cause stuttering.
        /// </summary>
        public virtual void Show()
        {
            if (_content)
                _content.SetActive(true);
            if (_video)
            {
                _video.enabled = true;
                if (_splashState == SplashState.Preparing)
                {
                    Debug.LogWarning($"Screen {name} video is still preparing, playing anyway");
                    _video.prepareCompleted -= VideoReady;
                    _video.errorReceived -= VideoFailed;
                    _splashState = SplashState.Failed;
                }

                _video.Play();
            }
        }
        public virtual void Hide()
        {
            _splashState = SplashState.Idle;
            if (_content && _content.activeSelf)
                _content.SetActive(false);
            if (_video)
            {
                _video.prepareCompleted -= VideoReady;
                _video.errorReceived -= VideoFailed;
                if (_video.isPlaying)
                    _video.Stop();
                _video.enabled = false;
            }
            if (_timeOut != null)
            {
                StopCoroutine(_timeOut);
                _timeOut = null;
            }
        }

        /// <summary>
        /// Returns true if the splash screen may be skipped manually.
        /// </summary>
        /// <param name="timePassed">Elapsed time since the screen was shown.</param>
        public bool CanSkipSplash(float timePassed)
        {
            return (timePassed >= _minContinue);
        }

        /// <summary>
        /// Returns true when this splash screen should automatically advance.
        /// Auto-continue requires the minimum auto-continue time to elapse and,
        /// if a video is present, for playback to finish.
        /// </summary>
        public virtual bool ShouldAutoAdvance(float timePassed)
        {
            if (IsVideoPlaying())
                return false;

            if (timePassed < _autoContinue)
                return false;

            return true;
        }

        // Be aware that disabling the gameObject will stop video and hide content automatically
        private void OnDisable()
        {
            Hide();
        }

        public bool IsVideoPlaying()
        {
            if (_video)
            {
                if (_splashState == SplashState.Preparing || _video.isPlaying)
                    return true;
            }
            return false;
        }

        // Detect if video preparation takes too long
        private IEnumerator VideoTimeout()
        {
            yield return new WaitForSeconds(_failTimeout);
            _timeOut = null;
            if (_splashState == SplashState.Preparing)
                VideoFailed(_video, "timed out");
        }

        protected virtual void VideoReady(VideoPlayer videoReady)
        {
            _splashState = SplashState.Prepared;
            if (_video)
            {
                _video.prepareCompleted -= VideoReady;
                _video.errorReceived -= VideoFailed;
            }
            if (_timeOut != null)
            {
                StopCoroutine(_timeOut);
                _timeOut = null;
            }
        }

        protected virtual void VideoFailed(VideoPlayer videoReady, string message)
        {
            Debug.LogWarning($"CSSplashScreen screen {name} failed: {message}");
            _splashState = SplashState.Failed;
            if (_video)
            {
                _video.prepareCompleted -= VideoReady;
                _video.errorReceived -= VideoFailed;
            }
            if (_timeOut != null)
            {
                StopCoroutine(_timeOut);
                _timeOut = null;
            }
        }
    }
}
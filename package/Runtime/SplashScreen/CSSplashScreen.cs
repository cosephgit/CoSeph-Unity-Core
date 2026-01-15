using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// A single splash screen with video and content references.
    /// Managed by the CSSplashScreenManager.
    /// </summary>
    public class CSSplashScreen : MonoBehaviour
    {
        [Header("Components are optional and combinable")]
        [SerializeField] private CSVideoPlayer _video;
        [SerializeField] private GameObject _content;
        [SerializeField, Tooltip("The color setting for the camera background")] private Color _bgColor;
        [Header("Timing for this screen")]
        [SerializeField] private float _minContinue = 0f;
        [SerializeField] private float _autoContinue = 4f;
        public Color BGColor => _bgColor;

        public virtual void Show()
        {
            if (_content)
                _content.SetActive(true);
            if (_video)
                _video.Play();
        }

        public void Hide()
        {
            if (_video)
                _video.Stop();
            gameObject.SetActive(false);
        }

        private bool PlaybackActive()
        {
            if (_video)
            {
                if (_video.IsPlaying())
                    return true;
            }
            return false;
        }

        public bool CanContinue(float timePassed)
        {
            if (timePassed > _minContinue)
                return true;

            return false;
        }

        /// <summary>
        /// Check if this screen can auto-continue
        /// Requires both _autoContinue time to have passed
        /// and (if present) the video playback to have finished
        /// </summary>
        /// <param name="timePassed"></param>
        /// <returns></returns>
        public bool AutoContinue(float timePassed)
        {
            if (_video && timePassed <= _video.Duration)
                return false;

            if (timePassed > _autoContinue)
                return true;

            return false;
        }
    }
}
using UnityEngine;
using UnityEngine.Video;

namespace CoSeph.Core
{
    /// <summary>
    /// Class for managing video playback
    /// Allows for easy extensions for different playback media
    /// </summary>
    [CreateAssetMenu(fileName = "Achievement", menuName = "CS/VideoPlayer", order = 0)]
    public class CSVideoPlayer : ScriptableObject
    {
        [SerializeField] private VideoPlayer _video;
        [SerializeField] private float _duration;
        public float Duration => _duration;

        public virtual bool IsPlaying()
        {
            if (_video)
                return _video.isPlaying;
            return false;
        }

        public virtual void Play()
        {
            if (_video)
                _video.Play();
        }

        public virtual void Stop()
        {
            if (_video)
                _video.Stop();
        }
    }
}

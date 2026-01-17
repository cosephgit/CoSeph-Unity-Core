using UnityEngine;

namespace CoSeph.Core
{
    public class CSGUISampleSplash : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Rect _buttonRectStart = new Rect(20, 20, 160, 40);
        [SerializeField] private Rect _buttonRectProgress = new Rect(200, 20, 160, 40);
        [SerializeField] private Rect _buttonRectReset = new Rect(380, 20, 160, 40);
        [SerializeField] private Rect _textRectStatus = new Rect(20, 200, 160, 40);
        [Header("Splash screen manager")]
        [SerializeField] private CSSplashScreenManager _sampleSplashManager;

#if UNITY_EDITOR
        private void OnGUI()
        {
            bool isWaiting = false;
            bool isPlaying = false;
            bool isPrepping = false;

            GUI.enabled = true;

            if (_sampleSplashManager)
            {
                isWaiting = _sampleSplashManager.IsWaiting();
                isPrepping = _sampleSplashManager.IsPrepping();
                isPlaying = _sampleSplashManager.IsPlaying();

                if (isWaiting)
                    GUI.TextField(_textRectStatus, "Ready");
                else if (isPrepping)
                    GUI.TextField(_textRectStatus, "Preparing");
                else if (isPlaying)
                    GUI.TextField(_textRectStatus, "Playing");
                else
                    GUI.TextField(_textRectStatus, "Finished");
            }
            else
                GUI.TextField(_textRectStatus, "No splash manager");


            GUI.enabled = isWaiting;
            if (GUI.Button(_buttonRectStart, "Start CSSplashScreenManager"))
            {
                if (_sampleSplashManager)
                    _sampleSplashManager.StartSequence();
                else
                    Debug.LogWarning("CSGUISampleSplash _sampleSplashManager is null");
            }
            GUI.enabled = isPlaying;
            if (GUI.Button(_buttonRectProgress, "Progress CSSplashScreenManager"))
            {
                if (_sampleSplashManager)
                    _sampleSplashManager.ProgressSequence();
                else
                    Debug.LogWarning("CSGUISampleSplash _sampleSplashManager is null");
            }
            GUI.enabled = !isWaiting;
            if (GUI.Button(_buttonRectReset, "Reset CSSplashScreenManager"))
            {
                if (_sampleSplashManager)
                    _sampleSplashManager.ResetSequence();
                else
                    Debug.LogWarning("CSGUISampleSplash _sampleSplashManager is null");
            }
        }
#endif
    }
}

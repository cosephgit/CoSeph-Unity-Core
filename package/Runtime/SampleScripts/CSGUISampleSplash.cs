using UnityEngine;

namespace CoSeph.Core
{
    public class CSGUISampleSplash : MonoBehaviour
    {
        [SerializeField] private CSSplashScreenManager _sampleSplashManager;


#if UNITY_EDITOR
        private void OnGUI()
        {
            
        }
#endif
    }
}

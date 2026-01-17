using UnityEngine;

namespace CoSeph.Core.Editor
{
    /// <summary>
    /// Simple GUI script for activating a CSTimedEffect class
    /// Shows a button to activate it, and a button to progress the timer
    /// </summary>
    public class CSGUISampleTimedEffect : MonoBehaviour
    {
        // Button position and size
        [SerializeField] private Rect _buttonRectStart = new Rect(20, 20, 160, 40);
        [SerializeField] private Rect _buttonRectProgress = new Rect(200, 20, 160, 40);
        [SerializeField] private CSEffectTimed _effect;

#if UNITY_EDITOR
        void OnGUI()
        {
            if (GUI.Button(_buttonRectStart, "Start CSEffectTimed"))
            {
                if (_effect)
                    _effect.PlayEffect();
                else
                    Debug.LogWarning("CSGizmoTimedEffect effect is null");
            }
            if (GUI.Button(_buttonRectProgress, "Progress CSEffectTimed"))
            {
                if (_effect)
                    _effect.TurnEnd();
                else
                    Debug.LogWarning("CSGizmoTimedEffect effect is null");
            }
        }
#endif
    }
}

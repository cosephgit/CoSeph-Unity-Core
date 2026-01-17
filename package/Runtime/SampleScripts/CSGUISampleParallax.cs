using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Simple gizmo script to control camera position with buttons for parallax demo.
    /// </summary>
    public class CSGUISampleParallax : MonoBehaviour
    {
        [SerializeField] private Rect _buttonRectLeft = new Rect(20, 20, 160, 40);
        [SerializeField] private Rect _buttonRectRight = new Rect(20, 20, 160, 40);
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Vector3 _cameraStepRight;

#if UNITY_EDITOR
        void OnGUI()
        {
            Rect buttonRectRightAlign = _buttonRectRight;
            buttonRectRightAlign.x = Screen.width - _buttonRectRight.x - _buttonRectRight.width;
            if (GUI.Button(buttonRectRightAlign, "Move Right"))
            {
                if (_cameraTransform)
                    _cameraTransform.Translate(_cameraStepRight);
                else
                    Debug.LogWarning("CSGizmoParallax cameraTransform is null");
            }
            if (GUI.Button(_buttonRectLeft, "Move Left"))
            {
                if (_cameraTransform)
                    _cameraTransform.Translate(-_cameraStepRight);
                else
                    Debug.LogWarning("CSGizmoParallax cameraTransform is null");
            }
        }
#endif
    }
}

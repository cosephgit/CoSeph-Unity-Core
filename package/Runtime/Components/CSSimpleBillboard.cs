using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// A simple billboard class to make this object face towards the camera
    /// 
    /// Will face the specified _cameraTarget, if present
    /// Otherwise it will face Camera.main
    /// </summary>
    public class CSSimpleBillboard : MonoBehaviour
    {
        [SerializeField, Tooltip("If null, faces Camera.main")] private Camera _cameraTarget;

        private void FaceCamera(Camera target)
        {
            transform.LookAt(
                transform.position + target.transform.rotation * Vector3.forward,
                target.transform.rotation * Vector3.up
            );
        }

        private void LateUpdate()
        {
            if (_cameraTarget)
                FaceCamera(_cameraTarget);
            else if (Camera.main)
                FaceCamera(Camera.main);
        }
    }
}

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CoSeph.Core.Editor
{
    public class CSBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var sceneRefs = Resources.FindObjectsOfTypeAll<CSSceneReference>();

            foreach (var sceneRef in sceneRefs)
            {
                if (sceneRef.IsDefined())
                {
                    sceneRef.Validate();
                    if (!sceneRef.IsInBuildSettings())
                    {
                        Debug.LogWarning($"CSBuildValidator scene {sceneRef.SceneName} is not in build settings");
                    }
                }
            }
        }
    }
}
#endif
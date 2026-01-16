#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CoSeph.Core.Editor
{
    /// <summary>
    /// Will validate all CSSceneReference objects to ensure names are set correctly.
    /// Any which fail validation and are flagged as MustBeValid will cause build to fail.
    /// </summary>
    public class CSBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            CSSceneReference[] sceneRefs = Resources.FindObjectsOfTypeAll<CSSceneReference>();
            List<CSSceneReference> sceneInvalidRefs = new List<CSSceneReference>();

            foreach (CSSceneReference sceneRef in sceneRefs)
            {
                sceneRef.ValidateSceneReference();
                EditorUtility.SetDirty(sceneRef); // make sure validation data is saved

                if (sceneRef.MustBeValid 
                    && !sceneRef.IsDefined())
                {
                    // if any flagged scene references are undefined, the build will fail
                    sceneInvalidRefs.Add(sceneRef);
                }
            }

            // save scene references which have been validated
            AssetDatabase.SaveAssets();

            if (sceneInvalidRefs.Count > 0)
            {
                throw new BuildFailedException(
                    $"Build failed: {sceneInvalidRefs.Count} invalid CSSceneReference(s). {sceneInvalidRefs.ToCSV()}"
                );
            }
        }
    }
}
#endif
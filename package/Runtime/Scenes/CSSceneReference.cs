#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CoSeph.Core
{
    public class CSSceneReference : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;
#endif
        [SerializeField] private string sceneName;
        public string SceneName => sceneName;

#if UNITY_EDITOR
        public bool IsDefined()
        {
            return sceneAsset != null;
        }

        public bool IsInBuildSettings()
        {
            if (!IsDefined()) return false;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path.EndsWith($"{sceneName}.unity"))
                    return true;
            }
            return false;
        }

        private void OnValidate()
        {
            Validate();
        }

        public void Validate()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
                if (!IsInBuildSettings())
                    Debug.LogWarning($"Scene {sceneName} is not in build settings");
            }
        }
#endif
    }
}

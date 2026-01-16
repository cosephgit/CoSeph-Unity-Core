#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoSeph.Core
{
    /// <summary>
    /// Resilient scene references which allow automatically updating
    /// a scene reference string from the scene asset.
    /// CSBuildValidator will check for any invalid CSSceneReference during build.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneReference", menuName = "CS/Scenes/SceneReference", order = 0)]
    public class CSSceneReference : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset _sceneAsset;
#endif
        [SerializeField] private string _sceneName;
        [SerializeField, Tooltip("If true, build will fail if this asset fails validation")] private bool _mustBeValid;
        public string SceneName => _sceneName;
        public bool MustBeValid => _mustBeValid;

        /// <summary>
        /// Returns true if this is valid.
        /// In the editor, this requires a valid SceneAsset.
        /// In builds, this requires a non-empty scene name.
        /// </summary>
        public bool IsDefined()
        {
#if UNITY_EDITOR
            return _sceneAsset != null;
#else
            return !string.IsNullOrEmpty(_sceneName);
#endif
        }

        public bool LoadScene(LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                Debug.LogError("Scene reference is not defined", this);
                return false;
            }

#if UNITY_EDITOR
            // checking and warning - in case this scene should also be available in build
            if (!IsInBuildSettings())
                Debug.LogWarning($"Scene {_sceneName} is not in build settings", this);
#endif

            SceneManager.LoadScene(_sceneName, mode);
            return true;
        }
        public bool LoadSceneAsync(out AsyncOperation asyncLoad, bool allowSceneActivation = true, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                Debug.LogError("Scene reference is not defined", this);
                asyncLoad = null;
                return false;
            }

#if UNITY_EDITOR
            // checking and warning - in case this scene should also be available in build
            if (!IsInBuildSettings())
                Debug.LogWarning($"Scene {_sceneName} is not in build settings", this);
#endif

            asyncLoad = SceneManager.LoadSceneAsync(_sceneName, mode);

            if (asyncLoad == null)
            {
                Debug.LogError($"LoadSceneAsync failed with scene {_sceneName}", this);
                return false;
            }

            asyncLoad.allowSceneActivation = allowSceneActivation;

            return true;
        }

#if UNITY_EDITOR
        public bool IsInBuildSettings()
        {
            if (!IsDefined()) return false;

            string scenePath = AssetDatabase.GetAssetPath(_sceneAsset);

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == scenePath)
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            ValidateSceneReference();
        }

        public void ValidateSceneReference()
        {
            if (_sceneAsset == null)
            {
                _sceneName = string.Empty;
            }
            else
            {
                _sceneName = _sceneAsset.name;
                if (!IsInBuildSettings())
                    Debug.LogWarning($"CSSceneReference {_sceneName} is not in build settings", this);
            }
        }
#endif
    }
}

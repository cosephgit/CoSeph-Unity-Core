#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace CoSeph.Core.Editor
{
    /// <summary>
    /// Editor utility that replaces the currently selected GameObjects
    /// with instances of a specified prefab.
    /// 
    /// - Preserves parent hierarchy
    /// - Preserves local transform (position, rotation, scale)
    /// - Operates per-scene (supports multi-scene editing)
    /// - Fully undoable as a single operation
    /// </summary>
    public class CSReplaceSelection : EditorWindow
    {
        // cap on number of error messages permitted before truncating
        private const int ERRORFLOODCAP = 5;
        // Hard cap to prevent editor freezes from massive selections
        private const int REPLACEMENTCAP = 100;
        [SerializeField] private GameObject prefab;

        [MenuItem("Tools/Replace Selection")]
        static void CreateReplaceWithPrefab()
        {
            EditorWindow.GetWindow<CSReplaceSelection>("Replace selection");
        }

        // Editor GUI loop.
        // Replacement is disabled while the editor is in Play Mode to avoid runtime state corruption.
        private void OnGUI()
        {
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

            //GUI.enabled = (EditorApplication.isPlaying == false) && (prefab != null) && (Selection.gameObjects.Length > 0);

            bool canReplace = (!EditorApplication.isPlaying)
                && (prefab != null)
                && (Selection.gameObjects.Length > 0);

            using (new EditorGUI.DisabledScope(!canReplace))
            {
                if (GUILayout.Button("Replace"))
                {
                    GameObject[] selection = Selection.gameObjects;
                    PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefab);

                    // Only allow asset prefabs (Regular or Variant).
                    // Scene instances or missing assets are rejected.
                    if (prefabType != PrefabAssetType.Regular
                        && prefabType != PrefabAssetType.Variant)
                    {
                        Debug.LogWarning("CSReplaceSelection.OnGUI prefabType is not a prefab", this);
                    }
                    else if (selection.Length > REPLACEMENTCAP)
                    {
                        Debug.LogWarning($"More than {REPLACEMENTCAP} objects selected - this is probably not a good idea, replacement cancelled", this);
                    }
                    else
                    {
                        Undo.IncrementCurrentGroup();
                        Undo.SetCurrentGroupName("Replace Selection With Prefab");
                        int undoGroup = Undo.GetCurrentGroup();
                        int errors = 0; // Caps error logging to avoid flooding the console

                        void ErrorLog(string message)
                        {
                            if (errors < ERRORFLOODCAP)
                                Debug.LogError(message, this);
                            else if (errors == ERRORFLOODCAP)
                                Debug.LogError("More than 5 errors found - error logs truncated", this);
                            errors++;
                        }

                        for (int i = selection.Length - 1; i >= 0; --i)
                        {
                            if (selection[i] == null)
                            {
                                ErrorLog($"Selection {i} is null");
                                continue;
                            }
                            if (!selection[i].scene.IsValid())
                            {
                                ErrorLog($"Selection {i} ({selection[i].name}) has invalid scene");
                                continue;
                            }

                            GameObject selected = selection[i];

                            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, selected.scene);
                            newObject.name = prefab.name;

                            if (newObject == null)
                            {
                                ErrorLog("Error instantiating prefab");
                                continue;
                            }

                            Undo.RegisterCreatedObjectUndo(newObject, "Replace Selection With Prefab");

                            // Copy local transform values to ensure consistent hierarchy-relative placement.
                            // World-space copying is avoided due to unreliable scale reconstruction
                            // when parent transforms are non-uniform.
                            newObject.transform.SetParent(selected.transform.parent, false);
                            newObject.transform.localPosition = selected.transform.localPosition;
                            newObject.transform.localRotation = selected.transform.localRotation;
                            newObject.transform.localScale = selected.transform.localScale;
                            newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                            Undo.DestroyObjectImmediate(selected);
                        }

                        Undo.CollapseUndoOperations(undoGroup);
                    }
                }
            }

            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
        }
    }
}
#endif
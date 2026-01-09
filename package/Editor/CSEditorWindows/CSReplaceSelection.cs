#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace CoSeph.Core.Editor
{
    /// <summary>
    /// TODO
    /// </summary>
    public class CSReplaceSelection : EditorWindow
    {
        [SerializeField] private GameObject prefab;

        [MenuItem("Tools/Replace Selection")]
        static void CreateReplaceWithPrefab()
        {
            EditorWindow.GetWindow<CSReplaceSelection>();
        }

        private void OnGUI()
        {
            prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

            if (GUILayout.Button("Replace"))
            {
                var selection = Selection.gameObjects;

                for (var i = selection.Length - 1; i >= 0; --i)
                {
                    var selected = selection[i];

                    var prefabType = PrefabUtility.GetPrefabAssetType(prefab);

                    //var prefabType = PrefabUtility.GetPrefabType(prefab);
                    GameObject newObject;

                    //if (prefabType == PrefabType.Prefab)
                    if (prefabType != PrefabAssetType.MissingAsset)
                        continue;

                    if (prefabType != PrefabAssetType.NotAPrefab)
                    {
                        newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    }
                    else
                    {
                        newObject = Instantiate(prefab);
                        newObject.name = prefab.name;
                    }

                    if (newObject == null)
                    {
                        Debug.LogError("Error instantiating prefab");
                        break;
                    }

                    Undo.RegisterCreatedObjectUndo(newObject, "Replace Selection");
                    newObject.transform.parent = selected.transform.parent;
                    newObject.transform.localPosition = selected.transform.localPosition;
                    newObject.transform.localRotation = selected.transform.localRotation;
                    newObject.transform.localScale = selected.transform.localScale;
                    newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                    Undo.DestroyObjectImmediate(selected);
                }
            }

            GUI.enabled = false;
            EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
        }
    }
}
#endif
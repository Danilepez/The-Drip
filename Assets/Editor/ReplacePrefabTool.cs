using UnityEditor;
using UnityEngine;

public class ReplacePrefabTool : EditorWindow
{
    private GameObject _prefab;

    [MenuItem("Tools/Replace Selection with Prefab")]
    private static void Open()
    {
        GetWindow<ReplacePrefabTool>("Replace with Prefab");
    }

    private void OnGUI()
    {
        GUILayout.Label("1. Selecciona los GameObjects en la Hierarchy");
        GUILayout.Label("2. Arrastra el prefab aquí:");
        _prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefab, typeof(GameObject), false);

        EditorGUI.BeginDisabledGroup(_prefab == null || Selection.gameObjects.Length == 0);
        if (GUILayout.Button($"Reemplazar {Selection.gameObjects.Length} objeto(s)"))
            ReplaceSelection();
        EditorGUI.EndDisabledGroup();

        if (_prefab == null)
            EditorGUILayout.HelpBox("Asigna un prefab.", MessageType.Warning);
        else if (Selection.gameObjects.Length == 0)
            EditorGUILayout.HelpBox("Selecciona objetos en la Hierarchy.", MessageType.Warning);
    }

    private void ReplaceSelection()
    {
        GameObject[] selected = Selection.gameObjects;
        Undo.SetCurrentGroupName("Replace with Prefab");
        int group = Undo.GetCurrentGroup();

        foreach (GameObject old in selected)
        {
            GameObject replacement = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, old.transform.parent);
            Undo.RegisterCreatedObjectUndo(replacement, "Replace with Prefab");

            replacement.transform.SetPositionAndRotation(old.transform.position, old.transform.rotation);
            replacement.transform.localScale = old.transform.localScale;
            replacement.name = old.name;

            Undo.DestroyObjectImmediate(old);
        }

        Undo.CollapseUndoOperations(group);
    }
}

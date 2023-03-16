using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneEditor))]
[CanEditMultipleObjects]
public class SceneEditorInspector : Editor
{
    SerializedProperty sceneName;
    SerializedProperty pathProperty;
    SerializedProperty guidProperty;
    SerializedProperty isStartProperty;
    SerializedProperty linkProperty;
    SerializedProperty dialogues;
    bool isSceneNameHighlighted = false;
    DialogueListEditor dialogueEditor = new DialogueListEditor();
    Rect guiRect;
    float height = -1;

    private void OnEnable()
    {
        sceneName = serializedObject.FindProperty("SceneName");
        pathProperty = serializedObject.FindProperty("_path");
        guidProperty = serializedObject.FindProperty("_guid");
        isStartProperty = serializedObject.FindProperty("isStart");
        linkProperty = serializedObject.FindProperty("linkedScene");
        dialogues = serializedObject.FindProperty("dialogues");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //GUI Definition
        guiRect = EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent()
        {
            text = "Scene Details"
        },EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUI.SetNextControlName("Scene Name");
        EditorGUILayout.PropertyField(sceneName);
        GUI.enabled = false;
        EditorGUILayout.PropertyField(isStartProperty);
        EditorGUILayout.PropertyField(linkProperty);
        GUI.enabled = true;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent()
        {
            text = "Dialogues"
        }, EditorStyles.boldLabel);
        dialogueEditor.Show(serializedObject, dialogues, Event.current);
        Repaint();
        EditorGUILayout.EndVertical();
        if (height == -1 && guiRect.height != 0)
            height = guiRect.height;
        EditorGUILayout.Space((Screen.height - height)/2.0f - 7.5f);
        //Update the scene name in the scene graph
        string name = GUI.GetNameOfFocusedControl();
        TestIfSceneNameChanged(name);
        
        serializedObject.ApplyModifiedProperties();
    }
    public void TestIfSceneNameChanged(string name)
    {
        if (name == "Scene Name")
        {
            isSceneNameHighlighted = true;
        }
        else
        {
            if (isSceneNameHighlighted)
            {
                isSceneNameHighlighted = false;
                ChangeAssetName();
            }
        }
    }
    void ChangeAssetName()
    {
        string path = pathProperty.stringValue;
        AssetDatabase.RenameAsset(path, sceneName.stringValue + ".asset");
        int lastIndex = path.LastIndexOf('/');
        path = path.Remove(lastIndex + 1);
        path += sceneName.stringValue + ".asset";
        pathProperty.stringValue = path;
        serializedObject.ApplyModifiedProperties();
        serializedObject.targetObject.name = sceneName.stringValue;
    }
}

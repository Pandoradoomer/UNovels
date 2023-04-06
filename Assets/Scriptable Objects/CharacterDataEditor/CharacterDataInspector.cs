using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CharacterData))]
[CanEditMultipleObjects]
public class CharacterDataInspector : Editor
{
    SerializedProperty characterImage;
    SerializedProperty characterName;
    SerializedProperty nameColor;
    SerializedProperty dialogueColor;

    private bool isCharacterNameHighlighted = false;

    private void OnEnable()
    {
        characterImage = serializedObject.FindProperty("characterImage");
        characterName = serializedObject.FindProperty("characterName");
        nameColor = serializedObject.FindProperty("nameColor");
        dialogueColor = serializedObject.FindProperty("dialogueColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(characterImage);
        GUI.SetNextControlName("Character Name");
        EditorGUILayout.PropertyField(characterName);
        EditorGUILayout.PropertyField(nameColor);
        EditorGUILayout.PropertyField(dialogueColor);

        string name = GUI.GetNameOfFocusedControl();
        TestIfCharacterNameChanged(name);
        serializedObject.ApplyModifiedProperties();
    }

    public void TestIfCharacterNameChanged(string name)
    {
        if (name == "Character Name")
        {
            isCharacterNameHighlighted = true;
        }
        else
        {
            if (isCharacterNameHighlighted)
            {
                isCharacterNameHighlighted = false;
                ChangeAssetName();
            }
        }
    }

    void ChangeAssetName()
    {
        string path = AssetDatabase.GetAssetPath(serializedObject.targetObject.GetInstanceID());
        AssetDatabase.RenameAsset(path, characterName.stringValue + ".asset");
        serializedObject.ApplyModifiedProperties();
        serializedObject.targetObject.name = characterName.stringValue;
    }
}


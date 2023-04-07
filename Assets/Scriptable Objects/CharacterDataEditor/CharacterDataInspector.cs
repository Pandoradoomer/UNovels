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
    SerializedProperty emotions;
    SerializedProperty isNarrator;

    private bool isCharacterNameHighlighted = false;

    private void OnEnable()
    {
        characterImage = serializedObject.FindProperty("characterImage");
        characterName = serializedObject.FindProperty("characterName");
        nameColor = serializedObject.FindProperty("nameColor");
        dialogueColor = serializedObject.FindProperty("dialogueColor");
        emotions = serializedObject.FindProperty("emotions");
        isNarrator = serializedObject.FindProperty("isNarrator");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("MUST SET!", EditorStyles.boldLabel);
        if(!isNarrator.boolValue)
        EditorGUILayout.PropertyField(characterImage, new GUIContent()
        { 
            text = "Character Sprite",
            tooltip = "Default character image.\n" +
            "If no emotions are specified, this image will be used."
        });
        
        GUI.SetNextControlName("Character Name");
        if(isNarrator.boolValue)
        {
            GUI.enabled = false;
        }
        EditorGUILayout.PropertyField(characterName);
        if(isNarrator.boolValue)
        {
            GUI.enabled = true;
        }
        if(!isNarrator.boolValue)
            EditorGUILayout.PropertyField(nameColor);
        EditorGUILayout.PropertyField(dialogueColor);
        if(!isNarrator.boolValue)
        EditorGUILayout.PropertyField(emotions, new GUIContent()
        { 
            text = "Character Emotions",
            tooltip = "A list of a character's emotions and the sprites associated to them.\n" +
            "If the list is left empty, the character's default sprite will be used."
        });

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


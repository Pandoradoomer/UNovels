using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DialogueListEditor 
{
    public static void Show(SerializedProperty list)
    {
        //EditorGUILayout.PropertyField(list);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if(list.arraySize == 0)
        {
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = "There are no dialogues"
            }, EditorStyles.label);
        }
        else
        {
            for (int i = 0; i < list.arraySize; i++)
            {
                //EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Say");
                GUILayout.Space(20);
                string s = list.GetArrayElementAtIndex(i).FindPropertyRelative("dialogueText").stringValue;
                EditorGUILayout.LabelField(s);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button(new GUIContent()
        {
            text = "+"
        }, EditorStyles.miniButton))
        {
            list.arraySize++;
            SerializedProperty dialogue = list.GetArrayElementAtIndex(list.arraySize - 1);
            SerializedProperty character = dialogue.FindPropertyRelative("Character");
            //character.objectReferenceValue = new CharacterData();
            //list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = null;
        }
        if(GUILayout.Button(new GUIContent()
        {
            text = "-"
        }, EditorStyles.miniButton))
        {
            list.arraySize--;
        }
        EditorGUILayout.EndHorizontal();
    }
}

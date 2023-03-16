using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class DialogueListEditor 
{
    int highlightedIndex = -1;
    List<Rect> dialoguesRect = new List<Rect>();
    public void Show(SerializedProperty list, Event e)
    {
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
                DrawBoxes(list);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawButtons(list);
        ListenForEvents(list, e);
        EditorGUILayout.EndHorizontal();
        
    }

    private void DrawBoxes(SerializedProperty list)
    {
        List<Rect> boxes = new List<Rect>();
        for(int i = 0; i < list.arraySize; i++)
        {
            boxes.Add(EditorGUILayout.BeginVertical(EditorStyles.helpBox));
            EditorGUILayout.BeginHorizontal();
            if(highlightedIndex == i)
            {
                EditorGUILayout.LabelField("hehe");
            }
            else
                EditorGUILayout.LabelField("Say");

            GUILayout.Space(20);
            string s = list.GetArrayElementAtIndex(i).FindPropertyRelative("dialogueText").stringValue;
            EditorGUILayout.LabelField(s);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        dialoguesRect = boxes;
    }

    private void DrawButtons(SerializedProperty list)
    {

        if (GUILayout.Button(new GUIContent()
        {
            text = "+"
        }, EditorStyles.miniButton))
        {
            list.arraySize++;
            SerializedProperty dialogue = list.GetArrayElementAtIndex(list.arraySize - 1);
            SerializedProperty character = dialogue.FindPropertyRelative("Character");
            character.objectReferenceValue = SearchForNarrator();
        }
        if (GUILayout.Button(new GUIContent()
        {
            text = "-"
        }, EditorStyles.miniButton))
        {
            if (highlightedIndex == list.arraySize - 1)
                highlightedIndex = -1;
            list.arraySize--;
        }
    }
    private void ListenForEvents(SerializedProperty list, Event e)
    {
        bool found = false;
        if(e.type == EventType.MouseDown && e.button == 0)
        {
            for(int i = 0; i < dialoguesRect.Count; i++)
            {
                if(CheckBoxCollision(e.mousePosition, dialoguesRect[i]))
                {
                    highlightedIndex = i;
                    found = true;
                }
            }
            if (found == false)
                highlightedIndex = -1;
        }
    }

    private bool CheckBoxCollision(Vector2 pos, Rect buttonRect)
    {
        if (pos.x > buttonRect.x && pos.y > buttonRect.y &&
            pos.x < buttonRect.x + buttonRect.width &&
            pos.y < buttonRect.y + buttonRect.height)
            return true;
        return false;
    }

    private CharacterData SearchForNarrator()
    {
        var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
        List<CharacterData> characters = new List<CharacterData>();
        foreach(var asset in assets)
        {
            characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
        }
        return characters.FirstOrDefault(x => x.name == "");
    }
}

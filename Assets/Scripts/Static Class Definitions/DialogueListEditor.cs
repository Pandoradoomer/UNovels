using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public enum CommandType
{
    SAY,
    WAIT
}
public class DialogueListEditor 
{
    int highlightedIndex = -1;
    List<Rect> dialoguesRect = new List<Rect>();
    private Texture iconUp;
    private Texture iconDown;
    public void Show(SerializedObject obj, SerializedProperty list, Event e)
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
        DrawButtons(obj, list);
        ListenForEvents(list, e);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        if (highlightedIndex != -1)
        {
            DrawHighlightedDialogueEditor(list);
        }
        EditorGUILayout.EndVertical();
        
    }

    
    private void DrawBoxes(SerializedProperty list)
    {
        GUIStyle CommandTypeLabelStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState() {textColor = Color.white }
        };
        GUIStyle CharacterStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState() { textColor = Color.white }

        };
        GUIStyle DialogueStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Italic,
            normal = new GUIStyleState() { textColor = Color.white }
        };
        List<Rect> boxes = new List<Rect>();
        GUILayout.Space(4);
        for(int i = 0; i < list.arraySize; i++)
        {
            var characterProperty = list.GetArrayElementAtIndex(i).FindPropertyRelative("Character");
            var character = new SerializedObject(characterProperty.objectReferenceValue);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            boxes.Add(EditorGUILayout.BeginHorizontal());
            GUILayout.Space(10);
            if (i == highlightedIndex)
            {
                Rect last = boxes.Last();
                last.x--;
                last.y--;
                last.width+=2;
                last.height+=2;
                EditorGUI.DrawRect(last, Color.green);
            }
            EditorGUI.DrawRect(boxes.Last(), Color.grey);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Say", CommandTypeLabelStyle, new[] { GUILayout.Width(30)});
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{character.FindProperty("characterName").stringValue}", CharacterStyle, new[] {GUILayout.Width(50)});
            string s = list.GetArrayElementAtIndex(i).FindPropertyRelative("dialogueText").stringValue;
            bool truncated = s.Length > 15;
            string toShow = "";
            if (truncated)
            {
                toShow = s.Substring(0, 15) + "...";
            }
            else toShow = s;
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(toShow, DialogueStyle, new[] {GUILayout.Width(100)});
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }
        GUILayout.Space(2);
        dialoguesRect = boxes;
    }

    private void DrawButtons(SerializedObject obj, SerializedProperty list)
    {
        if(GUILayout.Button(new GUIContent()
        {
            text = "↑"
        },EditorStyles.miniButton))
        {
            if(highlightedIndex != -1 && highlightedIndex != 0 && list.arraySize > 1)
            {
                list.MoveArrayElement(highlightedIndex, highlightedIndex - 1);
                highlightedIndex--;
            }
        }
        if (GUILayout.Button(new GUIContent()
        {
            text = "↓"
        }, EditorStyles.miniButton))
        {
            if(highlightedIndex != -1 && highlightedIndex != list.arraySize - 1 && list.arraySize > 1)
            {

                list.MoveArrayElement(highlightedIndex, highlightedIndex + 1);
                highlightedIndex++;
            }
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent()
        {
            text = "+"
        }, EditorStyles.miniButton))
        {
            list.arraySize++;
            SerializedProperty dialogue = list.GetArrayElementAtIndex(list.arraySize - 1);
            SerializedProperty character = dialogue.FindPropertyRelative("Character");
            character.objectReferenceValue = SearchForNarrator();
            obj.ApplyModifiedProperties();
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
        if(e.type == EventType.MouseDown && e.button == 0)
        {
            for(int i = 0; i < dialoguesRect.Count; i++)
            {
                if(CheckBoxCollision(e.mousePosition, dialoguesRect[i]))
                {
                    GUI.FocusControl(null);
                    highlightedIndex = i;
                }
            }
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

    float boxRectWidth = -1;
    private void DrawHighlightedDialogueEditor(SerializedProperty list)
    {
        string text = "";
        string[] options = { };

        var dialogue = list.GetArrayElementAtIndex(highlightedIndex);
        var characterProperty = dialogue.FindPropertyRelative("Character");
        var character = new SerializedObject(dialogue.FindPropertyRelative("Character").objectReferenceValue);   
        var dialogueText = dialogue.FindPropertyRelative("dialogueText");

        text = dialogueText.stringValue;
        var characters = GetAllCharacters();
        List<string> characterNames = characters.Select(character => character.characterName).ToList();
        characterNames[characterNames.FindIndex(x => x == "")] = "<None>";
        options = characterNames.ToArray();
        var selectedCharacterName = character.FindProperty("characterName").stringValue;
        int selectedIndex = -1;
        if (selectedCharacterName == "")
            selectedIndex = characterNames.IndexOf("<None>");
        else 
            selectedIndex = characterNames.IndexOf(selectedCharacterName);

        Rect boxRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(5);
        EditorGUILayout.LabelField(new GUIContent()
        {
            text = "Say",
            tooltip = "Display a text box on screen with the specified text"
        }, EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Character", GUILayout.Width(100));
        selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Story text", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(5);
        text = EditorGUILayout.TextArea(text, new[]
        {
            //GUILayout.Width(boxRectWidth - 20),
            GUILayout.MinHeight(200),
            GUILayout.ExpandHeight(true),
        });
        GUILayout.Space(5);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();
        if(boxRectWidth == -1 && boxRect.width != 0)
        {
            boxRectWidth = boxRect.width;
        }

        characterProperty.objectReferenceValue = GetCharacterByName(options[selectedIndex]);
        dialogueText.stringValue = text;
    }

    private CharacterData SearchForNarrator()
    {
        var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
        List<CharacterData> characters = new List<CharacterData>();
        foreach(var asset in assets)
        {
            characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
        }
        return characters.FirstOrDefault(x => x.characterName == "");
    }

    private List<CharacterData> GetAllCharacters()
    {
        var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
        List<CharacterData> characters = new List<CharacterData>();
        foreach (var asset in assets)
        {
            characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
        }
        return characters;
    }

    private CharacterData GetCharacterByName(string name)
    {

        var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
        List<CharacterData> characters = new List<CharacterData>();
        foreach (var asset in assets)
        {
            characters.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData)) as CharacterData);
        }
        if (name == "<None>")
            return characters.FirstOrDefault(x => x.characterName == "");
        return characters.FirstOrDefault(x => x.characterName == name);
    }
}

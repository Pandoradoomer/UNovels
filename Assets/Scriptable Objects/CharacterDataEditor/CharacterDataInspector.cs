using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

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
    SerializedProperty imgSize;
    SerializedProperty debugShowRight;
    SerializedProperty debugShowCentre;
    SerializedProperty debugShowLeft;

    private bool isCharacterNameHighlighted = false;

    Canvas canvas;

    private void OnEnable()
    {
        characterImage = serializedObject.FindProperty("characterImage");
        characterName = serializedObject.FindProperty("characterName");
        nameColor = serializedObject.FindProperty("nameColor");
        dialogueColor = serializedObject.FindProperty("dialogueColor");
        emotions = serializedObject.FindProperty("emotions");
        isNarrator = serializedObject.FindProperty("isNarrator");
        imgSize = serializedObject.FindProperty("imgSize");
        debugShowRight = serializedObject.FindProperty("ShowDebugRight");
        debugShowCentre = serializedObject.FindProperty("ShowDebugCentre");
        debugShowLeft = serializedObject.FindProperty("ShowDebugLeft");
        SceneView.duringSceneGui += OnSceneGUI;
        canvas = FindObjectOfType<Canvas>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void OnSceneGUI(SceneView sv)
    {
        CharacterData cd = target as CharacterData;
        if (cd != null)
        {
            if (cd.ShowDebugLeft)
            {
                Rect r = new Rect();
                Vector2 pos = canvas.pixelRect.center;
                pos.x += -250 * canvas.scaleFactor;
                pos.y += -26 * canvas.scaleFactor;

                pos.x -= cd.imgSize.x * canvas.scaleFactor / 2.0f;
                pos.y -= cd.imgSize.y * canvas.scaleFactor / 2.0f;
                r.position = pos;
                r.size = cd.imgSize * canvas.scaleFactor;
                float sizeMult = 192/SceneView.currentDrawingSceneView.camera.orthographicSize;
                Handles.Label(r.position + Vector2.up * r.size.y /2 + Vector2.right * r.size.x / 2, new GUIContent()
                {
                    image = cd.characterImage.texture
                }, new GUIStyle()
                {
                    fixedHeight = r.size.y * sizeMult,
                    fixedWidth = r.size.x * sizeMult,
                    stretchWidth = false,
                    stretchHeight = false,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.FloorToInt(200/sizeMult),
                    wordWrap = true,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.green,
                    }
                    
                });
            }
            if (cd.ShowDebugCentre)
            {
                Rect r = new Rect();
                Vector2 pos = canvas.pixelRect.center;
                pos.x += 0;
                pos.y += -26 * canvas.scaleFactor;

                pos.x -= cd.imgSize.x * canvas.scaleFactor / 2.0f;
                pos.y -= cd.imgSize.y * canvas.scaleFactor / 2.0f;
                r.position = pos;
                r.size = cd.imgSize * canvas.scaleFactor;
                float sizeMult = 192 / SceneView.currentDrawingSceneView.camera.orthographicSize;
                Handles.Label(r.position + Vector2.up * r.size.y / 2 + Vector2.right * r.size.x / 2, new GUIContent()
                {
                    image = cd.characterImage.texture
                }, new GUIStyle()
                {
                    fixedHeight = r.size.y * sizeMult,
                    fixedWidth = r.size.x * sizeMult,
                    stretchWidth = false,
                    stretchHeight = false,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.FloorToInt(200 / sizeMult),
                    wordWrap = true,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.green,
                    }

                });
            }

            if (cd.ShowDebugRight)
            {
                Rect r = new Rect();
                Vector2 pos = canvas.pixelRect.center;
                pos.x += 250 * canvas.scaleFactor;
                pos.y += -26 * canvas.scaleFactor;

                pos.x -= cd.imgSize.x * canvas.scaleFactor / 2.0f;
                pos.y -= cd.imgSize.y * canvas.scaleFactor / 2.0f;
                r.position = pos;
                r.size = cd.imgSize * canvas.scaleFactor;
                float sizeMult = 192 / SceneView.currentDrawingSceneView.camera.orthographicSize;
                Handles.Label(r.position + Vector2.up * r.size.y / 2 + Vector2.right * r.size.x / 2, new GUIContent()
                {
                    image = cd.characterImage.texture
                }, new GUIStyle()
                {
                    fixedHeight = r.size.y * sizeMult,
                    fixedWidth = r.size.x * sizeMult,
                    stretchWidth = false,
                    stretchHeight = false,
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.FloorToInt(200 / sizeMult),
                    wordWrap = true,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.green,
                    }

                });
            }
        }
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
        if(!isNarrator.boolValue)
        {
            EditorGUILayout.PropertyField(imgSize, new GUIContent()
            {
                text = "Image Size",
                tooltip = "Change the size of the image displayed. Default is 300x400"
            });
            EditorGUILayout.PropertyField(debugShowLeft);
            EditorGUILayout.PropertyField(debugShowCentre);
            EditorGUILayout.PropertyField(debugShowRight);
        }

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


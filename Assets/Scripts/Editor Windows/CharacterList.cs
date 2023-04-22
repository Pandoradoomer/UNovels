using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterList : EditorWindow
{
    List<CharacterData> characters;
    List<Rect> currentDrawnRects;
    int highlightedIndex = -1;

    [MenuItem("VN_Engine/Open Character List", priority = 2)]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<CharacterList>();
    }
    private void OnEnable()
    {
        characters = GetAllCharacters();
        currentDrawnRects =new List<Rect>();
    }
    private void OnFocus()
    {
        if(highlightedIndex != -1)
            OpenCharacterByIndex(highlightedIndex);
    }
    private void OnGUI()
    {
        float elemWidth = 100.0f;
        float elemHeight = 40.0f;
        DrawButtons();
        currentDrawnRects.Clear();
        Rect lastRect = new Rect(-elemWidth/2, elemHeight, elemWidth, elemHeight);
        for(int i = 0; i < characters.Count; i++)
        {
            DrawCharacter(ref lastRect, elemWidth, elemHeight, characters[i], i);
        }
        ListenForEvents();
    }

    private void ListenForEvents()
    {
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector2 mousePos = Event.current.mousePosition;
            bool hasFound = false;
            //on left mouse click
            for(int i = 0; i < currentDrawnRects.Count; i++)
            {
                Rect rect = currentDrawnRects[i];
                if(mousePos.x > rect.x && mousePos.x < rect.x + rect.width &&
                    mousePos.y > rect.y && mousePos.y < rect.y + rect.height)
                {
                    OpenCharacterByIndex(i);
                    hasFound = true;
                    highlightedIndex = i;
                }
            }
            if (!hasFound)
                highlightedIndex = -1;
            Repaint();
        }
    }

    private void DrawButtons()
    {

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent()
        {
            text = "Remove Character",
            tooltip = "Removes the highlighted character"
        }))
        {
            if(highlightedIndex != -1)
            {
                CharacterData charData = characters[highlightedIndex];
                if (charData.name == "_Narrator")
                {
                    Debug.LogError("Cannot remove the narrator!");

                }
                else
                {
                    RemoveCharacter(charData);
                    characters = GetAllCharacters();
                    highlightedIndex = -1;
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent()
        {
            text = "Add Character",
            tooltip = "Adds another character to the list"
        }))
        {
            CharacterData.Create();
            characters = GetAllCharacters();
            OpenCharacterByIndex(characters.Count - 1);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void DrawCharacter(ref Rect lastRect, float elemWidth, float elemHeight, CharacterData charData, int index)
    {
        DrawCharacterImage(ref lastRect, elemWidth, elemHeight, charData);
        DrawCharacterRect(lastRect, index);
        DrawCharacterLabel(lastRect, charData.characterName);

    }

    private void DrawCharacterImage(ref Rect lastRect, float elemWidth, float elemHeight, CharacterData charData)
    {

        Rect newRect = new Rect(lastRect);
        newRect.x += elemWidth + elemWidth / 4;
        if (newRect.x > position.width - elemWidth)
        {
            newRect.x = elemWidth / 2 + elemWidth / 4;
            newRect.y += 3 * elemHeight / 2;
        }
        currentDrawnRects.Add(newRect);
        if (charData.characterImage != null)
        {
            Rect drawRect = new Rect(newRect);
            drawRect.height = 3 * elemHeight / 2;
            drawRect.width = elemWidth / 10;
            drawRect.x -= elemWidth / 8;
            drawRect.y -= elemHeight / 2;
            Rect texCoords = new Rect();
            texCoords.x = charData.characterImage.textureRect.x / charData.characterImage.textureRect.width;
            texCoords.y = charData.characterImage.textureRect.y / charData.characterImage.textureRect.height;
            texCoords.width = charData.characterImage.textureRect.width / charData.characterImage.textureRect.width;
            texCoords.height = charData.characterImage.textureRect.height / charData.characterImage.textureRect.height;
            //GUI.DrawTextureWithTexCoords(drawRect, charData.characterImage.texture, texCoords);
        }
        lastRect = newRect;
    }
    private void DrawCharacterLabel(Rect rect, string name)
    {
        GUI.Label(rect, name, new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                textColor = Color.black
            },
            alignment = TextAnchor.MiddleCenter
        });
    }
    private void DrawCharacterRect(Rect lastRect, int index)
    {
        if(index == highlightedIndex)
            Handles.DrawSolidRectangleWithOutline(lastRect, new Color(0.82f, 1.0f, 0.74f), Color.red);
        else

            Handles.DrawSolidRectangleWithOutline(lastRect, new Color(0.82f, 1.0f, 0.74f), Color.white);
    }

    private void RemoveCharacter(CharacterData charData)
    {
        characters.Remove(charData);
        AssetDatabase.DeleteAsset($"Assets/Scriptable Objects/Characters/{charData.characterName}.asset");
        AssetDatabase.Refresh();
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
    private void OpenCharacterByIndex(int index)
    {

        ActiveEditorTracker.sharedTracker.isLocked = false;
        AssetDatabase.OpenAsset(characters[index].GetInstanceID());
        ActiveEditorTracker.sharedTracker.isLocked = true;
    }
    private void OpenCharacterByName(string name)
    {
        var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
        foreach (var asset in assets)
        {
            var cd = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(CharacterData));
            if (name == "<None>")
            {
                if (cd.name == "")
                    AssetDatabase.OpenAsset(cd.GetInstanceID());
            }
            else if (name == cd.name)
                AssetDatabase.OpenAsset(cd.GetInstanceID());

        }
    }
}

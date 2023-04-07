using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CharacterList : EditorWindow
{
    List<CharacterData> characters;

    [MenuItem("VN_Engine/Show Character List")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<CharacterList>();
    }
    private void OnEnable()
    {
        characters = GetAllCharacters();
    }
    private void OnGUI()
    {
        float elemWidth = 100.0f;
        float elemHeight = 40.0f;
        DrawButtons();
        Rect lastRect = new Rect(-elemWidth/2, elemHeight, elemWidth, elemHeight);
        for(int i = 0; i < characters.Count; i++)
        {
            DrawCharacter(ref lastRect, elemWidth, elemHeight, characters[i]);
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
            CharacterData charData = characters.Last();
            if(charData.name == "Narrator")
            {
                Debug.LogError("Cannot remove the narrator!");
                
            }
            else
            {
                RemoveCharacter(charData);
                characters = GetAllCharacters();
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
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void DrawCharacter(ref Rect lastRect, float elemWidth, float elemHeight, CharacterData charData)
    {
        DrawRect(ref lastRect, elemWidth, elemHeight);
        DrawCharacterLabel(lastRect, charData.characterName);

    }
    private void DrawCharacterLabel(Rect rect, string name)
    {
        GUI.Label(rect, name, new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                textColor = Color.white
            },
            alignment = TextAnchor.MiddleCenter
        });
    }
    private void DrawRect(ref Rect lastRect, float elemWidth, float elemHeight)
    {
        Rect newRect = new Rect(lastRect);
        newRect.x += elemWidth + elemWidth/4;
        if(newRect.x > position.width)
        {
            newRect.x = elemWidth / 2;
            newRect.y += 3 * elemHeight / 2;
        }
        Handles.DrawSolidRectangleWithOutline(newRect, Color.clear, Color.white);
        
        lastRect = newRect;
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

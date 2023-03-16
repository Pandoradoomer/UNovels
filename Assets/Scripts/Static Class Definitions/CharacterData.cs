using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CharacterData : ScriptableObject
{
    public Sprite characterImage;
    public string characterName;
    public Color nameColor;
    public Color dialogueColor;

    [MenuItem("VN_Engine/Create Character")]
    public static void Create()
    {
        string name = "NewCharacter";
        Create(name);
    }

    public static void Create(string name)
    {
        var assets = AssetDatabase.FindAssets("", new[] { "Assets/Scriptable Objects/Characters/" });
        CharacterData buffer = ScriptableObject.CreateInstance<CharacterData>();
        string path = $"Assets/Scriptable Objects/Characters/{name}{assets.Length}.asset";
        AssetDatabase.CreateAsset(buffer, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = buffer;
    }

    public static void CreateNarrator()
    {
        CharacterData buffer = ScriptableObject.CreateInstance<CharacterData>();
        string path = $"Assets/Scriptable Objects/Characters/Narrator.asset";
        buffer.name = "Narrator";
        buffer.characterName = "";
        buffer.dialogueColor = Color.white;
        AssetDatabase.CreateAsset(buffer, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = buffer;
    }
}

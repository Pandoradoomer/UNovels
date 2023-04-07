using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;


public class CharacterData : ScriptableObject
{
    public Sprite characterImage;
    public string characterName;
    public Color nameColor = Color.white;
    public Color dialogueColor = Color.white;
    public List<EmotionPair> emotions;

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
        buffer.characterName = name + assets.Length.ToString();
        buffer.name = name + assets.Length.ToString();
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
        buffer.emotions = new List<EmotionPair>();
        AssetDatabase.CreateAsset(buffer, path);
        AssetDatabase.SaveAssets();
        //Selection.activeObject = buffer;
    }

    public static void DeleteNarrator()
    {
        AssetDatabase.DeleteAsset("Assets/Scriptable Objects/Characters/Narrator.asset");
    }
}
[Serializable]
public struct EmotionPair
{
    public string emotion;
    public Sprite sprite;
}



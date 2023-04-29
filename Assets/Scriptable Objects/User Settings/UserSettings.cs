using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu()]
public class UserSettings : ScriptableObject
{
    [Header("Text Box Attributes")]
    [Header("Universal attributes")]
    public Color TextBoxColor = new Color(50, 50, 50, 170);
    public float DialogueFontSize = 22;
    public float CharacterNameFontSize = 18;
    [Tooltip("Text speed defines how many characters are shown every second. A text speed of 1 shows a character every second.")]
    public float TextSpeed = 20;
    [Header("Character Text Box Attributes")]
    public Vector2 CharacterTextBoxSize = new Vector2(700, 130);
    public Vector2 CharacterTextBoxPosition = new Vector2(0, -140);
    [Header("Narrator Text Box Attributes")]
    public Vector2 NarratorTextBoxSize = new Vector2(700, 400);
    public Vector2 NarratorTextBoxPosition = Vector2.zero;
    [Header("Debug Only")]
    [Tooltip("Shows the narrator text box in the Scene Tab")]
    public bool ShowNarratorTextBox;
    [Tooltip("Shows the character text box in the Scene Tab")]
    public bool ShowCharacterTextBox;
    [HideInInspector]
    public Camera camera;
    [HideInInspector]
    public Canvas canvas;

    private void Reset()
    {
        TextBoxColor = new Color(50, 50, 50, 170);
    }
    private void OnValidate()
    {
        if (camera == null)
            camera = GameObject.FindObjectOfType<Camera>();
        if(canvas == null)
            canvas = GameObject.FindObjectOfType<Canvas>();
    }

    public static void CreateUserSettings()
    {
        UserSettings buffer = ScriptableObject.CreateInstance<UserSettings>();
        string path = $"Assets/Scriptable Objects/User Settings/Objects/_UserSettings.asset";
        buffer.name = "_UserSettings";
        AssetDatabase.CreateAsset(buffer, path);
        AssetDatabase.SaveAssets();
    }

    public static void DeleteUserSettings()
    {
        AssetDatabase.DeleteAsset("Assets/Scriptable Objects/User Settings/Objects/_UserSettings.asset");
    }
}


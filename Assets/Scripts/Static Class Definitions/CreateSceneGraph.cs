using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateSceneGraph : MonoBehaviour
{

    private GameObject go = null;

    GameObject sceneGo, messageManagerGo;
    [MenuItem("VN_Engine/Initialise", priority = 1)]
    [ExecuteInEditMode]
    // Start is called before the first frame update
    static void Start()
    {
        var SceneClass = FindObjectOfType<SceneClassContainer>();
        if (SceneClass != null)
        {
            Debug.Log("Already initalised!");
            return;
        }
        var scenePrefab = Resources.Load("Prefabs/Scene");
        var messageManagerPrefab = Resources.Load("Prefabs/MessageManager");
        if (scenePrefab == null)
        {
            Debug.LogError("Found no scene object!");
            return;
        }
        if(messageManagerPrefab == null)
        {
            Debug.LogError("Found no message manager object!");
        }
        var sceneGo = Instantiate(scenePrefab);
        var messageManagerGo = Instantiate(messageManagerPrefab);
        sceneGo.name = sceneGo.name.Replace("(Clone)","");
        messageManagerGo.name = messageManagerGo.name.Replace("(Clone)", "");
        CharacterData.CreateNarrator();
        UserSettings.CreateUserSettings();
    }
    [MenuItem("VN_Engine/Reset", priority = 1)]
    [ExecuteInEditMode]
    static void ResetVNEngine()
    {
        ActiveEditorTracker.sharedTracker.isLocked = false;
        if (EditorWindow.HasOpenInstances<SceneGraph>())
            EditorWindow.GetWindow<SceneGraph>(null, false).Close();
        DestroyImmediate(FindObjectOfType<SceneClassContainer>().gameObject, false);
        DestroyImmediate(FindObjectOfType<MessageManager>().gameObject, false);
        CharacterData.DeleteNarrator();
        UserSettings.DeleteUserSettings();
        BlockFactory.HardReset();
    }
    [MenuItem("VN_Engine/Reset",true)]
    static bool ValidateResetVNEngine()
    {
        var SceneClass = FindObjectOfType<SceneClassContainer>();
        return SceneClass != null;
    }

    [MenuItem("VN_Engine/Open Scene Structure", priority = 2)]
    static void OpenSceneGraph()
    {
        var window = EditorWindow.GetWindow<SceneGraph>();
        if (!System.IO.File.Exists(Application.dataPath + "/Stored Data/blockData.json"))
        {
            window.FirstTimeInit();
        }
    }

    [MenuItem("VN_Engine/Open Scene Structure", true)]
    static bool ValidateOpenSceneGraph()
    {
        var SceneClass = FindObjectOfType<SceneClassContainer>();
        return SceneClass != null;
    }

    [MenuItem("VN_Engine/Open User Settings", priority = 2)]
    static void OpenUserSettings()
    {
        ActiveEditorTracker.sharedTracker.isLocked = false;
        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath("Assets/Scriptable Objects/User Settings/Objects/_UserSettings.asset", typeof(UserSettings)));
        ActiveEditorTracker.sharedTracker.isLocked = true;
    }
    [MenuItem("VN_Engine/Open User Settings", true)]
    static bool ValidateOpenUserSettings()
    {
        var SceneClass = FindObjectOfType<SceneClassContainer>();
        return SceneClass != null;
    }
}

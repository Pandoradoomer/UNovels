using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateSceneGraph : MonoBehaviour
{

    private GameObject go = null;

    GameObject sceneGo, messageManagerGo;
    [MenuItem("VN_Engine/Initialise")]
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
            Debug.Log("Found no scene object!");
            return;
        }
        if(messageManagerPrefab == null)
        {
            Debug.Log("Found no message manager object!");
        }
        var sceneGo = Instantiate(scenePrefab);
        var messageManagerGo = Instantiate(messageManagerPrefab);
        sceneGo.name = sceneGo.name.Replace("(Clone)","");
        messageManagerGo.name = messageManagerGo.name.Replace("(Clone)", "");
        CharacterData.CreateNarrator();
    }
    [MenuItem("VN_Engine/Reset")]
    [ExecuteInEditMode]
    static void ResetVNEngine()
    {
        ActiveEditorTracker.sharedTracker.isLocked = false;
        DestroyImmediate(FindObjectOfType<SceneClassContainer>().gameObject, false);
        DestroyImmediate(FindObjectOfType<MessageManager>().gameObject, false);
        if (EditorWindow.HasOpenInstances<SceneGraph>())
            EditorWindow.GetWindow<SceneGraph>().Close();
        CharacterData.DeleteNarrator();
    }
    [MenuItem("VN_Engine/Open Scene Structure")]
    static void OpenSceneGraph()
    {
        var window = EditorWindow.GetWindow<SceneGraph>();
        if (!System.IO.File.Exists(Application.dataPath + "/Stored Data/blockData.json"))
        {
            window.FirstTimeInit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

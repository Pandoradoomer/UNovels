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
        DestroyImmediate(FindObjectOfType<SceneClassContainer>().gameObject, false);
        DestroyImmediate(FindObjectOfType<MessageManager>().gameObject, false);
        CharacterData.DeleteNarrator();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

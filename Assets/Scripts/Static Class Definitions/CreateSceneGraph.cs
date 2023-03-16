using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateSceneGraph : MonoBehaviour
{

    private GameObject go = null;

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
        if (scenePrefab == null)
        {
            Debug.Log("Found no object!");
            return;
        }
        Debug.Log("Instantiated!");
        var go = Instantiate(scenePrefab);
        go.name = go.name.Replace("(Clone)","");
        CharacterData.CreateNarrator();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

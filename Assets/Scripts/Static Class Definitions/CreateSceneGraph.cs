using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateSceneGraph : MonoBehaviour
{

    private GameObject go = null;

    [MenuItem("VN_Engine/Create Scene Graph")]
    [ExecuteInEditMode]
    // Start is called before the first frame update
    static void Start()
    {
        var SceneClass = FindObjectOfType<SceneClassContainer>();
        if (SceneClass != null)
        {
            Debug.Log("There's already a Scene in the hierarchy!");
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SceneClassContainer : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(SceneClassContainer))]
public class SceneClassEditor : Editor
{
    SceneClassContainer _obj;

    private void OnEnable()
    {
        _obj = (SceneClassContainer)target;
        _obj.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        if(GUILayout.Button("Edit Scene Graph"))
        {
            SceneGraph sceneGraph = EditorWindow.GetWindow<SceneGraph>();
        }
        EditorGUILayout.EndVertical();
    }
}

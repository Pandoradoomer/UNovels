using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneClassContainer : MonoBehaviour
{
    public bool wasInit = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupInitialSceneGraph(SceneGraph sceneGraph)
    {
        wasInit = true;
        sceneGraph.FirstTimeInit();
        sceneGraph.classContainer = this;
    }

    //[ExecuteInEditMode]
    private void OnDestroy()
    {
        BlockFactory.HardReset();
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
            if(!_obj.wasInit)
            {
                _obj.SetupInitialSceneGraph(sceneGraph);
            }
        }
        EditorGUILayout.EndVertical();
    }
}

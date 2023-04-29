using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneClassContainer : MonoBehaviour
{
    public bool wasInit = false;
    SceneGraph sG = null;
    // Start is called before the first frame update
    void Start()
    {
        //if(Application.isPlaying)
        //{
        //    var go = Resources.Load("Prefabs/SceneManager");
        //    if(go == null)
        //    {
        //        Debug.LogError($"Couldn't find SceneManager in the Resources/Prefabs folder!");
        //        return;
        //    }
        //    Instantiate(go,this.transform);
        //}
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
    public void SetSceneGraph(SceneGraph sceneGraph)
    {
        sG = sceneGraph;
    }

    //[ExecuteInEditMode]
    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
        {
            if (sG != null)
            {
                sG.Close();
            }
            BlockFactory.HardReset();
        }
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
        /*
        EditorGUILayout.BeginVertical();
        if(GUILayout.Button("Edit Scene Structure"))
        {
            SceneGraph sceneGraph = EditorWindow.GetWindow<SceneGraph>();
            if(!_obj.wasInit)
            {
                _obj.SetupInitialSceneGraph(sceneGraph);
            }
            _obj.SetSceneGraph(sceneGraph);
        }
        EditorGUILayout.EndVertical();*/
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SceneEditor : ScriptableObject
{
    public Guid guid { get; private set; }
    [SerializeField]
    private string _guid;
    public string SceneName = "New Scene";
    public TransitionTypes entryTransition = TransitionTypes.FADE;
    public float entryTransitionValue = 0.0f;
    public TransitionTypes exitTransition = TransitionTypes.FADE;
    public float exitTransitionValue = 0.0f;
    public Sprite backgroundImage;
    public string path { get; private set; }
    [SerializeField]
    private string _path;
    public bool isStart = false;
    public SceneEditor linkedScene;
    public List<CommandData> Commands = new List<CommandData>();

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }
    public void SetGuid()
    {
        guid = Guid.NewGuid();
        _guid = guid.ToString();
    }
    public string GetGuid()
    {
        return _guid;
    }
    public void SetPath(string path)
    {
        this.path = path;
        _path = path;
    }

    public void SetName(string name)
    {
        SceneName = name;
    }

}



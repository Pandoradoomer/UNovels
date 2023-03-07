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
    public string path { get; private set; }
    [SerializeField]
    private string _path;
    public bool isStart = false;

    private void OnEnable()
    {
        //SceneName = "New Scene";
    }

    private void OnDisable()
    {
    }
    public void SetGuid()
    {
        guid = Guid.NewGuid();
        _guid = guid.ToString();
    }
    public void SetPath(string path)
    {
        this.path = path;
        _path = path;
    }

}



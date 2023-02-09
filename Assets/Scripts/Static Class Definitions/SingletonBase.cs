using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBase
{
    private static SingletonBase _instance;

    private GenericGraph<BasicSceneInfo> sceneInfos;
    private SingletonBase()
    {
        sceneInfos = new GenericGraph<BasicSceneInfo>();
    }

    public static SingletonBase Instance
    {
        get
        {
            if(_instance == null )
                _instance = new SingletonBase();
            return _instance;
        }
    }

    public GenericGraph<BasicSceneInfo> SceneInfos { get => sceneInfos; }
}

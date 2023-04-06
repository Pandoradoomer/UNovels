using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    List<SceneEditor> sceneEditors;

    public SceneEditor currScene = null;
    public bool isScenePlaying = false;
    public bool hasEnded = false;

    public static SceneManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        BlockFactory.LoadAllBlockAssets();
        sceneEditors = new List<SceneEditor>(BlockFactory.GetSceneEditors());
        currScene = sceneEditors.FirstOrDefault(x => x.isStart == true);
    }

    // Update is called once per frame
    void Update()
    {
        if (hasEnded)
            return;
        if(!isScenePlaying)
        {
            if(currScene == null)
            {
                hasEnded = true;
                return;
            }
            else
            {
                isScenePlaying = true;
                MessageManager.Instance.PlayScene(currScene);
            }
        }
    }
}

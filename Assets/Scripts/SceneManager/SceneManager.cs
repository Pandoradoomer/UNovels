using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    List<SceneEditor> sceneEditors;
    void Start()
    {
        BlockFactory.LoadAllBlockAssets();
        sceneEditors = new List<SceneEditor>(BlockFactory.GetSceneEditors());
        var startScene = sceneEditors.FirstOrDefault(x => x.isStart == true);
        StartCoroutine(MessageManager.Instance.LoadScene(startScene));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

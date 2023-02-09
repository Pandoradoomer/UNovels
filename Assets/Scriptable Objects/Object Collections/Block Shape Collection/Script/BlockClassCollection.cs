using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewBlockClassCollection", menuName = "VN_Engine/Blocks/BlockShapeCollection")]
public class BlockClassCollection : ScriptableObject
{
    public Dictionary<string,BlockClass> blocks;
    [SerializeField]
    List<string> keys;
    [SerializeField]
    List<BlockClass> values;

    private void OnValidate()
    {
        if(keys.Count != values.Count)
        {
            return;
        }
        blocks = new Dictionary<string, BlockClass>();
        for(int i = 0; i < keys.Count; i++)
        {
            blocks.Add(keys[i], values[i]);
        }
    }
}

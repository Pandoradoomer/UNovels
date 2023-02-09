using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum BlockShape
{
    Rect,
    Diamond
}

public delegate void BlockDrawerCallback(Vector2 pos, BlockClass blockClass);
public delegate bool BlockCollisionCallback(Vector2 mousePos, Vector2 objPos, BlockClass blockClass);

[CreateAssetMenu(fileName = "BlockShape", menuName = "VN_Engine/Blocks/BlockShape")]
public class BlockClass : ScriptableObject
{
    [Tooltip("The string displayed inside the name")]
    public string defaultLabelText = string.Empty;
    public BlockShape blockShape = BlockShape.Rect;
    public Vector2 size = Vector2.zero;
}

public class BlockDrawer
{
    public BlockClass blockClass;
    public Vector2 pos;
    public BlockDrawerCallback callback;
    public BlockCollisionCallback collisionCallback;

}

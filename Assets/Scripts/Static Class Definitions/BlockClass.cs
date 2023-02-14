using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
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
public delegate void BlockLabelDrawCallback(Vector2 pos, string text, BlockClass blockClass);
public delegate void BlockHighlightDrawCallback(Vector2 pos, BlockClass blockClass);

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
    public string labelText;
    public BlockDrawerCallback callback;
    public BlockCollisionCallback collisionCallback;
    public BlockLabelDrawCallback labelDrawCallback;
    public BlockHighlightDrawCallback highlightDrawCallback;

}

[Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    [JsonConstructor]
    public SerializableVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public SerializableVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }
    public Vector2 Vector2()
    {
        return new Vector2(x, y);
    }
}

[Serializable]
public class SerializableBlockClass
{
    public string defaultLabelText;
    public BlockShape blockShape;
    public SerializableVector2 size;

    [JsonConstructor]
    public SerializableBlockClass(string defaultLabelText, BlockShape blockShape, SerializableVector2 size)
    {
        this.defaultLabelText = defaultLabelText;
        this.blockShape = blockShape;
        this.size = size;
    }
    public SerializableBlockClass(BlockClass bc)
    {
        this.defaultLabelText = bc.defaultLabelText;
        this.blockShape = bc.blockShape;
        this.size = new SerializableVector2(bc.size);
    }

    public BlockClass BlockClass()
    {
        BlockClass bc = new BlockClass()
        {
            size = this.size.Vector2(),
            blockShape = this.blockShape,
            defaultLabelText = this.defaultLabelText
        };
        return bc;
    }
}

[Serializable]
public class SerializableBlockDrawer
{
    public SerializableBlockClass blockClass;
    public SerializableVector2 pos;
    public string text;

    [JsonConstructor]
    public SerializableBlockDrawer(SerializableBlockClass blockClass, SerializableVector2 pos, string text)
    {
        this.blockClass = blockClass;
        this.pos = pos;
        this.text = text;
    }

    public SerializableBlockDrawer(BlockDrawer bd)
    {
        blockClass = new SerializableBlockClass(bd.blockClass);
        pos = new SerializableVector2(bd.pos);
        text = bd.labelText;
    }

    public static implicit operator SerializableBlockDrawer(BlockDrawer bd)
    {
        return new SerializableBlockDrawer(bd);
    }
}

[Serializable]
public class BlockContents
{
    public SerializableBlockDrawer drawer;
    public List<string> dialogue;

}

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BlockFactory 
{
    public static BlockDrawer CreateBlockDrawer(BlockShape type, BlockClassCollection collection, Vector2 pos, bool isStart)
    {
        BlockDrawer bd = new BlockDrawer();
        switch(type)
        {
            case BlockShape.Rect:
                bd = CreateRectDrawer(collection, pos, isStart);
                break;
            case BlockShape.Diamond:
                bd = CreateDiamondDrawer(collection, pos, isStart);
                break;
        }
        return bd;
    }
    #region Block Drawer Factory Functions
    private static BlockDrawer CreateRectDrawer(BlockClassCollection collection, Vector2 pos, bool isStart)
    {
        BlockClass bc = collection.blocks["Rect"];
        BlockDrawer bd = new BlockDrawer()
        {
            blockClass = bc,
            pos = pos,
            labelText = bc.defaultLabelText,
            isStart = isStart,
            callback = DrawRectBlock,
            collisionCallback = CollideWithRect,
            highlightDrawCallback = DrawHighlightRectBlock,
            labelDrawCallback = DrawLabelRect,
            blockCentre = GetRectCentre
        };
        return bd;
    }

    private static BlockDrawer CreateDiamondDrawer(BlockClassCollection collection, Vector2 pos, bool isStart)
    {
        BlockClass bc = collection.blocks["Diamond"];
        BlockDrawer bd = new BlockDrawer()
        {
            blockClass = bc,
            pos = pos,
            labelText = bc.defaultLabelText,
            isStart = isStart,
            callback = DrawDiamondBlock,
            collisionCallback = CollideWithDiamond,
            highlightDrawCallback = DrawHighlightDiamondBlock,
            labelDrawCallback = DrawLabelDiamond,
            blockCentre = GetDiamondCentre
        };
        return bd;
    }

    #endregion

    #region Collider functions
    static bool CollideWithRect(Vector2 pos, Vector2 rectTopleft, BlockClass bc, float scale)
    {
        //pos is mouse position
        //rectTopLeft is top left of the rect, because that's how Handles draws it
        if (pos.x > rectTopleft.x && pos.x < rectTopleft.x + bc.size.x * scale &&
           pos.y > rectTopleft.y && pos.y < rectTopleft.y + bc.size.y * scale)
            return true;
        return false;
    }

    static bool CollideWithDiamond(Vector2 pos, Vector2 diaPos, BlockClass bc, float scale)
    {
        /*
        //pos is mouse position
        //diaPos is the centre of the diamond (intersection of the 2 diagonals)
        //bc.size gives the half-sizes of the bounding rectangle

        //Heuristic 1: perfect precision, high computation time:
        //Split the diamond in two triangles; test if the mouse is in either triangle

        var v1 = diaPos + Vector2.up * bc.size.y; //top of the diamond
        var v2 = diaPos + Vector2.right * bc.size.x; // right of the diamond
        var v3 = diaPos + Vector2.down * bc.size.y; //bottom of the diamond
        var v4 = diaPos + Vector2.left * bc.size.x; //left of the diamond

        //luckily, the two triangles have the same area: half the product of the x,y of bc.size

        float area1 = Mathf.Abs((v1.x - pos.x) * (v3.y - pos.y) - (v3.x - pos.x) * (v1.y - pos.y));
        float area2 = Mathf.Abs((v3.x - pos.x) * (v4.y - pos.y) - (v4.x - pos.x) * (v3.y - pos.y));
        float area3 = Mathf.Abs((v4.x - pos.x) * (v1.y - pos.y) - (v1.x - pos.x) * (v4.y - pos.y));

        bool tri1 = ((area1 + area2 + area3) == ((bc.size.x * bc.size.y) / 2.0f));

        area1 = Mathf.Abs((v1.x - pos.x) * (v2.y - pos.y) - (v2.x - pos.x) * (v1.y - pos.y));
        area2 = Mathf.Abs((v2.x - pos.x) * (v3.y - pos.y) - (v3.x - pos.x) * (v2.y - pos.y));
        area3 = Mathf.Abs((v3.x - pos.x) * (v1.y - pos.y) - (v1.x - pos.x) * (v3.y - pos.y));

        bool tri2 = ((area1 + area2 + area3) == ((bc.size.x * bc.size.y) / 2.0f));

        return tri1 || tri2;*/

        //Heuristic 2: lower precision, lower computation time:
        //collide with the diamond's bounding box instead of the diamond

        Vector2 topLeft = diaPos - new Vector2(bc.size.x, bc.size.y);

        if (pos.x > topLeft.x && pos.x < topLeft.x + bc.size.x * 2 &&
           pos.y > topLeft.y && pos.y < topLeft.y + bc.size.y * 2)
            return true;
        return false;



    }
    #endregion

    #region Drawer Functions
    static void DrawRectBlock(Vector2 pos, BlockClass blockClass)
    {
        Handles.color = Color.yellow;
        Handles.DrawSolidRectangleWithOutline(
            new Rect(pos, blockClass.size),
            Color.yellow, Color.clear);
    }

    static void DrawDiamondBlock(Vector2 pos, BlockClass blockClass)
    {
        Vector3[] points =
        {
            new Vector2(pos.x - blockClass.size.x, pos.y),
            new Vector2(pos.x, pos.y + blockClass.size.y),
            new Vector2(pos.x + blockClass.size.x, pos.y),
            new Vector2(pos.x, pos.y - blockClass.size.y)
        };
        Handles.color = Color.blue;
        Handles.DrawAAConvexPolygon(points);
    }

    static void DrawHighlightRectBlock(Vector2 pos, BlockClass blockClass)
    {
        Vector2 offset = Vector2.one * 2.0f;
        Handles.color = Color.green;
        Handles.DrawSolidRectangleWithOutline(
            new Rect(pos - offset, blockClass.size + offset * 2.0f),
            Color.green, Color.clear);
    }

    static void DrawHighlightDiamondBlock(Vector2 pos, BlockClass blockClass)
    {
        float highlightOffset = 2.0f;
        Vector3[] points =
        {
            new Vector2(pos.x - (blockClass.size.x + highlightOffset), pos.y),
            new Vector2(pos.x, pos.y + (blockClass.size.y + highlightOffset)),
            new Vector2(pos.x + (blockClass.size.x + highlightOffset), pos.y),
            new Vector2(pos.x, pos.y - (blockClass.size.y + highlightOffset))
        };
        Handles.color = Color.green;
        Handles.DrawAAConvexPolygon(points);
    }

    static void DrawLabelRect(Vector2 pos, string text, Color c, BlockClass blockClass)
    {
        GUIStyle labelStyle = new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = c },
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fixedHeight = blockClass.size.y,
            fixedWidth = blockClass.size.x
        };
        Vector2 offset = new Vector2(blockClass.size.x / 2.0f, blockClass.size.y / 2.0f);
        Handles.Label(pos + offset, text, labelStyle);
    }

    static void DrawLabelDiamond(Vector2 pos, string text, Color c, BlockClass blockClass)
    {
        GUIStyle labelStyle = new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = c },
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fixedHeight = blockClass.size.y * 2.0f + 2,
            fixedWidth = blockClass.size.x * 2.0f + 2
        };
        Handles.Label(pos, text, labelStyle);
    }
    static Vector3 GetRectCentre(Vector2 pos, BlockClass blockClass)
    {
        return pos + blockClass.size / 2;
    }
    static Vector3 GetDiamondCentre(Vector2 pos, BlockClass blockClass)
    {
        return pos;
    }
    #endregion

    #region Block Callback Getters
    static BlockDrawerCallback GetBlockDrawerCallback(BlockContents bc)
    {
        switch(bc.drawer.blockClass.blockShape)
        {
            case BlockShape.Rect:
                return DrawRectBlock;
            case BlockShape.Diamond:
                return DrawDiamondBlock;

        }
        return null;
    }

    static BlockHighlightDrawCallback GetBlockDrawerHighlightCallback(BlockContents bc)
    {
        switch(bc.drawer.blockClass.blockShape)
        {
            case BlockShape.Rect:
                return DrawHighlightRectBlock;
            case BlockShape.Diamond:
                return DrawHighlightDiamondBlock;
        }
        return null;
    }

    static BlockLabelDrawCallback GetBlockLabelDrawCallback(BlockContents bc)
    {
        switch (bc.drawer.blockClass.blockShape)
        {
            case BlockShape.Rect:
                return DrawLabelRect;
            case BlockShape.Diamond:
                return DrawLabelDiamond;
        }
        return null;
    }

    static BlockCollisionCallback GetBlockCollisionCallback(BlockContents bc)
    {
        switch(bc.drawer.blockClass.blockShape)
        {
            case BlockShape.Rect:
                return CollideWithRect;
            case BlockShape.Diamond:
                return CollideWithDiamond;
        }
        return null;
    }
    static GetBlockCentre GetBlockCentre(BlockContents bc)
    {
        switch(bc.drawer.blockClass.blockShape)
        {
            case BlockShape.Rect:
                return GetRectCentre;
            case BlockShape.Diamond:
                return GetDiamondCentre;
        }
        return null;
    }
    #endregion

    #region Creator Functions
    public static BlockContents CreateBlockContent(BlockDrawer bd, List<BlockDrawer> blockDrawers)
    {
        SerializableBlockDrawer d = new SerializableBlockDrawer()
        {
            blockClass = bd.blockClass,
            pos = new SerializableVector2(bd.pos),
            isStart = bd.isStart,
            text = bd.labelText,
            blockLink = bd.blockLink == null ? -1 : blockDrawers.IndexOf(bd.blockLink)

        };
        BlockContents bc = new BlockContents()
        {
            drawer = d,
            dialogue = new List<string>()

        };
        return bc;
    }
    public static BlockDrawer CreateBlockDrawer(BlockContents bc)
    {
        BlockDrawerCallback bdc = GetBlockDrawerCallback(bc);
        BlockCollisionCallback bcc = GetBlockCollisionCallback(bc);
        BlockHighlightDrawCallback bhdc = GetBlockDrawerHighlightCallback(bc);
        BlockLabelDrawCallback bldc = GetBlockLabelDrawCallback(bc);
        GetBlockCentre bct = GetBlockCentre(bc);
        BlockDrawer bc2 = new BlockDrawer()
        {
            blockClass = bc.drawer.blockClass.BlockClass(),
            pos = bc.drawer.pos.Vector2(),
            blockLink = null,
            labelText = bc.drawer.text,
            isStart = bc.drawer.isStart,
            callback = bdc,
            collisionCallback = bcc,
            highlightDrawCallback  = bhdc,
            labelDrawCallback = bldc,
            blockCentre = bct
        };
        return bc2;
    }

    public static void CreateLinks(List<BlockDrawer> drawers, List<BlockContents> contents)
    {
        for(int i = 0; i < drawers.Count; i++)
        {
            int index = contents[i].drawer.blockLink;
            drawers[i].blockLink = index == -1 ? null : drawers[index];
        }
    }
    #endregion

    public static List<BlockContents> MakeBlockContentsFromJSON()
    {
        var data = File.ReadAllText(Application.dataPath + "/Stored Data/blockData.json");
        List<BlockContents> blockContents = JsonConvert.DeserializeObject<List<BlockContents>>(data);
        return blockContents;
    }
    public static WorldData MakeWorldDataFromJSON()
    {
        var data = File.ReadAllText(Application.dataPath + "/Stored Data/worldData.json");
        WorldData worldData = JsonConvert.DeserializeObject<WorldData>(data);
        return worldData;
    }

    public static void WriteBlocksToJSON(List<BlockContents> blockContents)
    {
        var outputString = JsonConvert.SerializeObject(blockContents);
        File.WriteAllText(Application.dataPath + "/Stored Data/blockData.json", outputString);

    }

    public static void WriteWorldDataToJSON(WorldData data)
    {
        var outputString = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.dataPath + "/Stored Data/worldData.json", outputString);
    }

}

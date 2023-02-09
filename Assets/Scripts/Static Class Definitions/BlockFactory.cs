using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BlockFactory 
{
    public static BlockDrawer CreateBlockDrawer(BlockShape type, BlockClassCollection collection)
    {
        BlockDrawer bd = new BlockDrawer();
        switch(type)
        {
            case BlockShape.Rect:
                bd = CreateRectDrawer(collection);
                break;
            case BlockShape.Diamond:
                bd = CreateDiamondDrawer(collection);
                break;
        }
        return bd;
    }
    #region Create Drawer Factory Functions
    private static BlockDrawer CreateRectDrawer(BlockClassCollection collection)
    {
        BlockDrawer bd = new BlockDrawer()
        {
            blockClass = collection.blocks["Rect"],
            pos = new Vector2(200, 200),
            callback = DrawRectBlock,
            collisionCallback = CollideWithRect
        };
        return bd;
    }

    private static BlockDrawer CreateDiamondDrawer(BlockClassCollection collection)
    {
        BlockDrawer bd = new BlockDrawer()
        {
            blockClass = collection.blocks["Diamond"],
            pos = new Vector2(200, 200),
            callback = DrawDiamondBlock,
            collisionCallback = CollideWithDiamond
        };
        return bd;
    }

    #endregion

    #region Collider functions
    static bool CollideWithRect(Vector2 pos, Vector2 rectTopleft, BlockClass bc)
    {
        //pos is mouse position
        //rectTopLeft is top left of the rect, because that's how Handles draws it
        if (pos.x > rectTopleft.x && pos.x < rectTopleft.x + bc.size.x &&
           pos.y > rectTopleft.y && pos.y < rectTopleft.y + bc.size.y)
            return true;
        return false;
    }

    static bool CollideWithDiamond(Vector2 pos, Vector2 diaPos, BlockClass bc)
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
            Color.yellow, Color.red);
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
    #endregion

    public static BlockContents CreateBlockContent(BlockDrawer bd)
    {
        BlockContents bc = new BlockContents()
        {
            labelText = bd.blockClass.defaultLabelText,
            drawer = bd,
            dialogue = new List<string>()

        };
        return bc;
    }

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

    public static BlockDrawer CreateBlockDrawer(BlockContents bc)
    {
        BlockDrawerCallback bdc = GetBlockDrawerCallback(bc);
        BlockCollisionCallback bcc = GetBlockCollisionCallback(bc);
        BlockDrawer bc2 = new BlockDrawer()
        {
            blockClass = bc.drawer.blockClass.BlockClass(),
            pos = bc.drawer.pos.Vector2(),
            callback = bdc,
            collisionCallback = bcc
        };
        return bc2;
    }

    public static List<BlockContents> MakeBlockContentsFromJSON()
    {
        var data = File.ReadAllText(Application.dataPath + "/Stored Data/data.json");
        List<BlockContents> blockContents = JsonConvert.DeserializeObject<List<BlockContents>>(data);
        return blockContents;
    }

    public static void WriteToJSON(List<BlockContents> blockContents)
    {
        var outputString = JsonConvert.SerializeObject(blockContents);
        File.WriteAllText(Application.dataPath + "/Stored Data/data.json", outputString);

    }

}

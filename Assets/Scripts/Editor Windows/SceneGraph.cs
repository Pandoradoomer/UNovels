using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class SceneGraph : EditorWindow
{

    [MenuItem("VN_Engine/Scene Graph")]
    static void ShowEditor()
    {
        SceneGraph sceneGraph = EditorWindow.GetWindow<SceneGraph>();
    }

    [SerializeField]
    BlockClassCollection collection;
    List<BlockDrawer> blockDrawers;
    
    private void OnEnable()
    {
        blockDrawers = new List<BlockDrawer>();
    }
    #region Drawer Functions
    void DrawRectBlock(Vector2 pos, BlockClass blockClass)
    {
        Handles.color = Color.yellow;
        Handles.DrawSolidRectangleWithOutline(
            new Rect(pos, blockClass.size),
            Color.yellow, Color.red);
    }

    void DrawDiamondBlock(Vector2 pos, BlockClass blockClass)
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

    int selectedObjIndex = -1;
    int highlightedObjIndex = -1;
    Vector2 selectMousePos = Vector2.zero;
    Vector2 selectObjPos = Vector2.zero;


    private void OnGUI()
    {

        if(GUILayout.Button("Create Rect Node"))
        {
            BlockDrawer bd = BlockFactory.CreateBlockDrawer(BlockShape.Rect, collection);
            blockDrawers.Add(bd);
        }
        if(GUILayout.Button("Create Diamond Node"))
        {
            BlockDrawer bd = BlockFactory.CreateBlockDrawer(BlockShape.Diamond, collection);
            blockDrawers.Add(bd);
        }


        foreach(var bd in blockDrawers)
        {
            bd.callback.Invoke(bd.pos, bd.blockClass);
        }
        if(Event.current.type == EventType.MouseDown && Event.current.button ==0) 
        {
            Event e = Event.current;
            bool clickedOnVoid = true;
            for(int i = blockDrawers.Count - 1; i >=0; i--)
            {
                BlockDrawer bd = blockDrawers[i];
                if (bd.collisionCallback(e.mousePosition, bd.pos, bd.blockClass))
                {
                    clickedOnVoid = false; 
                    SelectBlock(i, e.mousePosition, bd.pos);
                    Repaint();
                    break;

                }
            }
            if(clickedOnVoid)
                highlightedObjIndex = -1;
        }
        if(Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            //Debug.Log("HIT!");
            if(selectedObjIndex != -1)
            {
                blockDrawers[selectedObjIndex].pos = selectObjPos + Event.current.mousePosition - selectMousePos;
                Repaint();
            }
        }
        if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            if(selectedObjIndex != -1)
            {
                UnselectBlock();
            }
        }
        if(Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace))
        {
            if(highlightedObjIndex != -1)
            {
                blockDrawers.RemoveAt(highlightedObjIndex);
                highlightedObjIndex= -1;
                Repaint();
            }
        }
    }

    void SelectBlock(int index, Vector2 mousePos, Vector2 objPos)
    {
        selectMousePos = mousePos;
        selectObjPos = objPos;
        //selecting an object has it be drawn on top;
        var block = blockDrawers[index];
        blockDrawers.RemoveAt(index);
        blockDrawers.Add(block);
        selectedObjIndex = blockDrawers.Count - 1;
        highlightedObjIndex = selectedObjIndex;
    }

    void UnselectBlock()
    {
        selectedObjIndex = -1;
        selectMousePos = Vector2.zero;
        selectObjPos = Vector2.zero;
    }
    #region Collider functions
    bool CollideWithRect(Vector2 pos, Vector2 rectTopleft, BlockClass bc)
    {
        //pos is mouse position
        //rectTopLeft is top left of the rect, because that's how Handles draws it
        if (pos.x > rectTopleft.x && pos.x < rectTopleft.x + bc.size.x &&
           pos.y > rectTopleft.y && pos.y < rectTopleft.y + bc.size.y)
            return true;
        return false;
    }

    bool CollideWithDiamond(Vector2 pos, Vector2 diaPos, BlockClass bc)
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
}

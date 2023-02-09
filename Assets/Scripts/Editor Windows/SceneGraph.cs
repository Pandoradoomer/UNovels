using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;  
public class SceneGraph : EditorWindow
{

    List<Vector2> rectPos = new List<Vector2>();
    List<Vector2> diamondPos = new List<Vector2>();


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

    void DrawRectBlock(Vector2 pos, BlockClass blockClass)
    {
        Handles.DrawSolidRectangleWithOutline(
            new Rect(pos, blockClass.size),
            Color.yellow, Color.red);
    }
    bool hasObjectSelected = false;
    int selectedObjIndex = -1;
    Vector2 selectMousePos = Vector2.zero;
    Vector2 selectObjPos = Vector2.zero;


    private void OnGUI()
    {

        if(GUILayout.Button("Create Rect Node"))
        {
            blockDrawers.Add(new BlockDrawer()
            {
                blockClass = collection.blocks["Rect"],
                pos = new Vector2(200, 200),
                callback = DrawRectBlock
            });
        }
        if(GUILayout.Button("Create Diamond Node"))
        {
            diamondPos.Add(new Vector2(350, 100));
        }


        foreach(var bd in blockDrawers)
        {
            bd.callback.Invoke(bd.pos, bd.blockClass);
        }
        if(Event.current.type == EventType.MouseDown && Event.current.button ==0) 
        {
            Event e = Event.current;
            for(int i = 0; i < rectPos.Count; i++)
            {
                var rp = rectPos[i];
                if (CollideWithRect(e.mousePosition, rp, new Vector2(100, 40)))
                {
                    hasObjectSelected = true;
                    selectedObjIndex = i;
                    selectMousePos = e.mousePosition;
                    selectObjPos = rp;
                }
            }
        }
        if(Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            Debug.Log("HIT!");
            if(hasObjectSelected)
            {
                rectPos[selectedObjIndex] = selectObjPos + Event.current.mousePosition - selectMousePos;
                Repaint();
            }
        }
        if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            if(hasObjectSelected)
            {
                hasObjectSelected = false;
                selectedObjIndex = -1;
                selectMousePos = Vector2.zero;
                selectObjPos = Vector2.zero;
            }
        }
    }

    bool CollideWithRect(Vector2 pos, Vector2 rectTopleft, Vector2 rectSize)
    {
        if (pos.x > rectTopleft.x && pos.x < rectTopleft.x + rectSize.x &&
           pos.y > rectTopleft.y && pos.y < rectTopleft.y + rectSize.y)
            return true;
        return false;
    }

    int diaWidth = 50;
    int diaHeight = 20;
    private void DrawDiamond(Vector2 pos)
    {
        Vector3[] points =
        {
            new Vector2(pos.x - diaWidth, pos.y),
            new Vector2(pos.x, pos.y + diaHeight),
            new Vector2(pos.x + diaWidth, pos.y),
            new Vector2(pos.x, pos.y - diaHeight)
        };
        Handles.color = Color.blue;
        Handles.DrawAAConvexPolygon(points);
    }
}

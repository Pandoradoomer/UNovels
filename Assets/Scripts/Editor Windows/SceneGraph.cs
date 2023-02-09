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
    
}

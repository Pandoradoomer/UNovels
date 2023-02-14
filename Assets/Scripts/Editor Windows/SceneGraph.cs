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
    //for now, until a better solution is found, let's keep both lists ordered by the same index
    //i.e. blockDrawers[2] will draw info from blockContents[2];
    List<BlockDrawer> blockDrawers;
    List<BlockContents> blockContents;
    
    private void OnEnable()
    {
        //blockDrawers = new List<BlockDrawer>();
        blockContents = BlockFactory.MakeBlockContentsFromJSON();
        blockDrawers = new List<BlockDrawer>();
        foreach(var bc in blockContents)
        {
            blockDrawers.Add(BlockFactory.CreateBlockDrawer(bc));
        }
        
    }

    int selectedObjIndex = -1;
    int highlightedObjIndex = -1;
    int toLinkObjIndex = -1;
    Vector2 selectMousePos = Vector2.zero;
    Vector2 selectObjPos = Vector2.zero;

    

    private void OnGUI()
    {
        //TODO: simplify/modularise
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
            if (bd.blockLink != -1)
            {
                Handles.color = Color.white;
                Handles.DrawLine(bd.pos, blockDrawers[bd.blockLink].pos);
            }
            if (highlightedObjIndex != -1)
            {
                if (blockDrawers.IndexOf(bd) == highlightedObjIndex)
                    bd.highlightDrawCallback(bd.pos, bd.blockClass);
            }
            bd.callback.Invoke(bd.pos, bd.blockClass);
            bd.labelDrawCallback.Invoke(bd.pos, bd.labelText, bd.blockClass);
            
        }
        if(Event.current.type == EventType.MouseDown && Event.current.button ==1)
        {
            
            int index = CheckBlockCollision(Event.current.mousePosition);
            if(index != -1)
            {
                highlightedObjIndex = index;
                Repaint();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Link Block"), false, LinkCallback, index);
                if (blockDrawers[index].blockLink == -1)
                {
                    menu.AddDisabledItem(new GUIContent("Remove link"), false);
                }
                else
                {
                    menu.AddItem(new GUIContent("Remove link"), false, UnlinkCallback, index);
                }

                menu.ShowAsContext();
            }
        }
        if(Event.current.type == EventType.MouseDown && Event.current.button ==0) 
        {
            Event e = Event.current;
            bool clickedOnVoid = true;
            int index = CheckBlockCollision(e.mousePosition);
            if (index != -1)
            {
                clickedOnVoid = false;
                SelectBlock(index, e.mousePosition, blockDrawers[index].pos);
                if(toLinkObjIndex != -1)
                {
                    LinkBlocks(toLinkObjIndex, index);
                }
                Repaint();
            }
            if (clickedOnVoid)
            {
                highlightedObjIndex = -1;
                toLinkObjIndex = -1;
                Repaint();
            }
        }
        if(Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
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

    void LinkCallback(object userData)
    {
        int? selectedIndex = userData as int?;

        toLinkObjIndex = selectedIndex.Value;
    }
    void UnlinkCallback(object userData)
    {
        int? selectedIndex = userData as int?;
        blockDrawers[selectedIndex.Value].blockLink = -1;
    }
    
    void LinkBlocks(int firstPart, int lastPart)
    {
        blockDrawers[firstPart].blockLink = lastPart;
    }

    int CheckBlockCollision(Vector2 mousePos)
    {
        int collisionIndex = -1;
        for(int i = blockDrawers.Count - 1; i >=0; i--)
        {
            BlockDrawer bd = blockDrawers[i];
            if(bd.collisionCallback(mousePos, bd.pos, bd.blockClass))
            {
                collisionIndex = i;
                break;
            }
        }
        return collisionIndex;
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

    private void OnDestroy()
    {
        blockContents.Clear();
        foreach(var bd in blockDrawers)
        {
            var bc = BlockFactory.CreateBlockContent(bd);
            blockContents.Add(bc);
        }
        BlockFactory.WriteToJSON(blockContents);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class SceneGraph : EditorWindow
{

    #region Init Functions
    [MenuItem("VN_Engine/Scene Graph")]
    static void ShowEditor()
    {
        SceneGraph sceneGraph = EditorWindow.GetWindow<SceneGraph>();
        sceneGraph.minSize = new Vector2(600.0f, 600.0f);
    }

    [SerializeField]
    BlockClassCollection collection;
    //for now, until a better solution is found, let's keep both lists ordered by the same index
    //i.e. blockDrawers[2] will draw info from blockContents[2];
    List<BlockDrawer> blockDrawers;
    List<BlockContents> blockContents;
    #region Zoom functions

    private const float kZoomMin = 0.1f;
    private const float kZoomMax = 10.0f;

    private Rect _zoomArea = new Rect(0.0f, 75.0f, 600.0f, 300.0f - 100.0f);
    private float _zoom = 1.0f;
    private Vector2 _zoomCoordsOrigin = Vector2.zero;

    Rect TopMenuRect = new Rect();
    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
    }

    private Vector2 ConvertZoomCoordsToScreenCoords(Vector2 zoomCoords)
    {
        return (zoomCoords - _zoomCoordsOrigin) * _zoom + _zoomArea.TopLeft();
    }
    private void DrawZoomArea()
    {
        // Within the zoom area all coordinates are relative to the top left corner of the zoom area
        // with the width and height being scaled versions of the original/unzoomed area's width and height.
        EditorZoomArea.Begin(_zoom, _zoomArea);
        foreach (var bd in blockDrawers)
        {
            if (bd.blockLink != null)
            {
                Handles.color = Color.white;
                DrawLink(bd, bd.blockLink);
            }
        }

        foreach (var bd in blockDrawers)
        {
            if (highlightedObjIndex != -1)
            {
                if (blockDrawers.IndexOf(bd) == highlightedObjIndex)
                    bd.highlightDrawCallback(bd.pos + currentWorldOrigin, bd.blockClass);
            }
            bd.callback.Invoke(bd.pos + currentWorldOrigin, bd.blockClass);
            bd.labelDrawCallback.Invoke(bd.pos + currentWorldOrigin, bd.labelText, bd.blockClass);

        }
        EditorZoomArea.End();
    }

    private void DrawNonZoomArea()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Reset World Origin"))
        {
            currentWorldOrigin = Vector2.zero;
            _zoomCoordsOrigin = currentWorldOrigin;
        }
        if(GUILayout.Button("Save"))
        {
            Save();
        }
        GUILayout.Label("Zoom: " + _zoom);
        _zoom = GUILayout.HorizontalSlider(_zoom, kZoomMin, kZoomMax);
        _zoom = Mathf.Round(_zoom * 10) / 10;

        GUILayout.EndVertical();
        if (Event.current.type == EventType.Repaint)
        {
            TopMenuRect = GUILayoutUtility.GetLastRect();
            Event.current.Use();
        }
        _zoomArea = position;
        _zoomArea.xMin = 0.0f;
        _zoomArea.yMin = TopMenuRect.height + 10.0f;
    }
    #endregion

    private void OnEnable()
    {
        //blockDrawers = new List<BlockDrawer>();
        blockContents = BlockFactory.MakeBlockContentsFromJSON();
        blockDrawers = new List<BlockDrawer>();
        foreach(var bc in blockContents)
        {
            blockDrawers.Add(BlockFactory.CreateBlockDrawer(bc));
        }

        BlockFactory.CreateLinks(blockDrawers, blockContents);
        WorldData worldData = BlockFactory.MakeWorldDataFromJSON();
        currentWorldOrigin = worldData.currentWorldOrigin.Vector2();
        _zoom = worldData.currentZoomValue;
        _zoomArea = position;
        
    }

    #endregion

    int selectedObjIndex = -1;
    int highlightedObjIndex = -1;
    int toLinkObjIndex = -1;
    BlockDrawer bdToLink = null;
    Vector2 selectMousePos = Vector2.zero;
    Vector2 selectObjPos = Vector2.zero;

    Vector2 currentWorldOrigin = Vector2.zero;
    Vector2 clickStartPos = Vector2.zero;

    bool controlPressed = false;


    

    private void OnGUI()
    {
        DrawZoomArea();
        DrawNonZoomArea();
        

        //right-click
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            
            int index = CheckBlockCollision(Event.current.mousePosition);
            if(index != -1)
            {
                highlightedObjIndex = index;
                Repaint();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Link Block"), false, LinkCallback, index);
                if (blockDrawers[index].blockLink == null)
                {
                    menu.AddDisabledItem(new GUIContent("Remove link"), false);
                }
                else
                {
                    menu.AddItem(new GUIContent("Remove link"), false, UnlinkCallback, index);
                }

                menu.ShowAsContext();
            }
            else
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Story Node"), false, AddRectNode, Event.current.mousePosition);
                menu.AddItem(new GUIContent("Add Choice Node"), false, AddDiamondNode, Event.current.mousePosition);
                menu.ShowAsContext();
            }
        }
        //left click
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftControl)
        {
            controlPressed = true;
        }
        if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
        {
            controlPressed = false;
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0) 
        {
            Event e = Event.current;
            bool clickedOnVoid = true;
            int index = CheckBlockCollision(e.mousePosition);
            if (index != -1)
            {
                BlockDrawer bd = blockDrawers[index];
                clickedOnVoid = false;
                SelectBlock(index, e.mousePosition + currentWorldOrigin, bd.pos + currentWorldOrigin);
                if(bdToLink != null)
                {
                    LinkBlocks(bdToLink, bd);
                    bdToLink = null;
                    toLinkObjIndex = -1;
                }
                Repaint();
            }
            if (clickedOnVoid)
            {
                highlightedObjIndex = -1;
                toLinkObjIndex = -1;
                if(controlPressed)
                {
                    clickStartPos = e.mousePosition - currentWorldOrigin;
                }
                Repaint();
            }
        }
        if(Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            if(selectedObjIndex != -1)
            {
                blockDrawers[selectedObjIndex].pos = selectObjPos + ConvertScreenCoordsToZoomCoords(Event.current.mousePosition) - selectMousePos;
                Repaint();
            }
            if(controlPressed)
            {
                currentWorldOrigin = -(clickStartPos - Event.current.mousePosition);
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
                DeleteBlockLinks(highlightedObjIndex);
                blockDrawers.RemoveAt(highlightedObjIndex);
                highlightedObjIndex= -1;
                Repaint();
            }
        }

    }

    void AddRectNode(object data)
    {
        Vector2? pos = data as Vector2?;

        BlockDrawer bd = BlockFactory.CreateBlockDrawer(BlockShape.Rect, collection, pos.Value);
        blockDrawers.Add(bd);
    }

    void AddDiamondNode(object data)
    {
        Vector2? pos = data as Vector2?;

        BlockDrawer bd = BlockFactory.CreateBlockDrawer(BlockShape.Diamond, collection, pos.Value);
        blockDrawers.Add(bd);
    }

    void DrawLink(BlockDrawer source, BlockDrawer destination)
    {
        Vector2 sourcePos = source.blockCentre.Invoke(source.pos, source.blockClass);
        Vector2 destPos = destination.blockCentre.Invoke(destination.pos, destination.blockClass);
        sourcePos += currentWorldOrigin;
        destPos += currentWorldOrigin;

        //draw the line
        Handles.DrawLine(sourcePos, destPos);

        //draw the little arrow centred on the middle of the line;

        Vector2 centre = (sourcePos + destPos) / 2;
        Vector2 dir = (centre - sourcePos).normalized;
        Vector2 dirRot45 = (Quaternion.AngleAxis(45, Vector3.forward) * dir).normalized;
        Vector2 dirRotm45 = (Quaternion.AngleAxis(-45, Vector3.forward) * dir).normalized;

        Vector2 offsetCentre = centre - dirRot45 * 10;
        Vector2 moffsetCentre = centre - dirRotm45 * 10;

        Handles.DrawLine(centre, offsetCentre);
        Handles.DrawLine(centre, moffsetCentre);
    }

    void LinkCallback(object userData)
    {
        int? selectedIndex = userData as int?;

        toLinkObjIndex = selectedIndex.Value;
        bdToLink = blockDrawers[selectedIndex.Value];
    }

    void UnlinkCallback(object userData)
    {
        int? selectedIndex = userData as int?;
        blockDrawers[selectedIndex.Value].blockLink = null;
    }
    
    void LinkBlocks(BlockDrawer source, BlockDrawer link)
    {
        source.blockLink = link;
    }

    void DeleteBlockLinks(int index)
    {
        blockDrawers[index].blockLink = null;
        foreach(BlockDrawer bd in blockDrawers)
        {
            if (bd.blockLink == blockDrawers[index])
                bd.blockLink = null;
        }
    }

    int CheckBlockCollision(Vector2 mousePos)
    {
        int collisionIndex = -1;
        for(int i = blockDrawers.Count - 1; i >=0; i--)
        {
            BlockDrawer bd = blockDrawers[i];
            var screenPos = ConvertZoomCoordsToScreenCoords(bd.pos + currentWorldOrigin);
            if(bd.collisionCallback(mousePos, screenPos, bd.blockClass, _zoom))
            {
                collisionIndex = i;
                break;
            }
        }
        return collisionIndex;
    }
    void SelectBlock(int index, Vector2 mousePos, Vector2 objPos)
    {
        selectMousePos = ConvertScreenCoordsToZoomCoords(mousePos);
        selectObjPos = objPos;
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

    private void Save()
    {
        blockContents.Clear();
        foreach (var bd in blockDrawers)
        {
            var bc = BlockFactory.CreateBlockContent(bd, blockDrawers);
            blockContents.Add(bc);
        }
        BlockFactory.WriteBlocksToJSON(blockContents);
        BlockFactory.WriteWorldDataToJSON(new WorldData(
            new SerializableVector2(currentWorldOrigin), _zoom));
    }
    private void OnDestroy()
    {
        Save();
    }

}

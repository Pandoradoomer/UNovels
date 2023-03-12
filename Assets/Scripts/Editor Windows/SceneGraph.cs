using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Linq;

public class SceneGraph : EditorWindow
{

    #region Init Functions
    public SceneClassContainer classContainer;
    static void ShowEditor()
    {
        SceneGraph sceneGraph = EditorWindow.GetWindow<SceneGraph>();
        sceneGraph.minSize = new Vector2(600.0f, 600.0f);
    }

    [SerializeField]
    BlockClassCollection collection;
    List<BlockDrawer> blockDrawers;
    List<BlockContents> blockContents;


    int selectedObjIndex = -1;
    int highlightedObjIndex = -1;
    int toLinkObjIndex = -1;
    BlockDrawer bdToLink = null;
    Vector2 selectMousePos = Vector2.zero;
    Vector2 selectObjPos = Vector2.zero;

    Vector2 currentWorldOrigin = Vector2.zero;
    Vector2 clickStartPos = Vector2.zero;

    bool controlPressed = false;
    private void OnEnable()
    {

        BlockFactory.LoadAllBlockAssets();
        blockContents = BlockFactory.MakeBlockContentsFromJSON();
        WorldData worldData = BlockFactory.MakeWorldDataFromJSON();
        if (blockContents == null || worldData == null)
            return;
        blockDrawers = new List<BlockDrawer>();
        foreach (var bc in blockContents)
        {
            blockDrawers.Add(BlockFactory.CreateBlockDrawer(bc));
        }

        BlockFactory.CreateLinks(blockDrawers, blockContents);
        currentWorldOrigin = worldData.currentWorldOrigin.Vector2();
        _zoom = worldData.currentZoomValue;
        _zoomArea = position;
    }

    public void FirstTimeInit()
    {
        blockContents = new List<BlockContents>();
        blockDrawers = new List<BlockDrawer>();
        AddRectNode(new Vector2(300.0f, 300.0f));
        currentWorldOrigin = Vector2.zero;
        _zoom = 1.0f;
        _zoomArea = position;
        Save();

    }
    #endregion

    #region Zoom functions

    private const float kZoomMin = 0.5f;
    private const float kZoomMax = 1.5f;

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
        DrawGrid();
        UpdateBlockDrawers();
        if (toLinkObjIndex != -1)
        {
            Vector2 centre = bdToLink.blockCentre.Invoke(bdToLink.pos, bdToLink.blockClass);
            Color c = Handles.color;
            Handles.color = Color.white;
            DrawArrow(centre + currentWorldOrigin, Event.current.mousePosition);
            Handles.color = c;
        }
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
            bd.labelDrawCallback.Invoke(bd.pos + currentWorldOrigin, bd.labelText, Color.black, bd.blockClass);
            if(bd.isStart)
            {
                bd.labelDrawCallback.Invoke(bd.pos + currentWorldOrigin - Vector2.up * bd.blockClass.size.y, "Start Node", Color.white, bd.blockClass);
            }

        }

        EditorZoomArea.End();
    }

    private void DrawNonZoomArea()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Recentre view"))
        {
            currentWorldOrigin = Vector2.zero;
            _zoomCoordsOrigin = currentWorldOrigin;
        }
        if(GUILayout.Button("Save"))
        {
            Save();
        }
        if(GUILayout.Button(new GUIContent()
        {
            text = "Rearrange Blocks",
            tooltip = "Brings all blocks on the screen and arranges them in a grid"
        }))
        {
            RearrangeBlocks();
        }
        GUILayout.EndHorizontal();
        GUILayout.Label($"MousePos x: {Event.current.mousePosition.x} y: {Event.current.mousePosition.y}");
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

    private void OnGUI()
    {
        DrawZoomArea();
        DrawNonZoomArea();
        

        //right-click
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            Vector2 mousePos = Event.current.mousePosition;
            int index = CheckBlockCollision(Event.current.mousePosition);
            if(index != -1)
            {
                highlightedObjIndex = index;
                Repaint();
                GenericMenu menu = new GenericMenu();
                if (blockDrawers[highlightedObjIndex].isStart)
                    menu.AddDisabledItem(new GUIContent("Delete Block"));
                else
                    menu.AddItem(new GUIContent("Delete Block"), false, DeleteBlock);
                menu.AddItem(new GUIContent("Link Block"), false, LinkCallback, index);
                menu.AddItem(new GUIContent("Edit Block"), false, EditCallback, index);
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
                Vector2 offset = ConvertScreenCoordsToZoomCoords(Event.current.mousePosition) - selectMousePos;
                Vector2 pos = selectObjPos + offset;
                pos.x = Mathf.Round(pos.x / 20.0f) * 20.0f;
                pos.y = Mathf.Round(pos.y / 20.0f) * 20.0f;
                blockDrawers[selectedObjIndex].pos = pos;
                //blockDrawers[selectedObjIndex].pos = selectObjPos + offset;
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

        }

    }

    void EditCallback(object data)
    {
        int? index = data as int?;
        BlockFactory.OpenBlockAsset(blockDrawers[index.Value]);
    }

    void DeleteBlock()
    {
        if (blockDrawers.Count == 1)
            return;
        DeleteBlockLinks(highlightedObjIndex);
        BlockFactory.DeleteBlockAsset(blockDrawers[highlightedObjIndex]);
        blockDrawers.RemoveAt(highlightedObjIndex);
        highlightedObjIndex = -1;
        Repaint();
        Save();
    }
    void AddRectNode(object data)
    {
        Vector2? pos = data as Vector2?;
        bool isStart = blockDrawers.Count == 0;
        Vector2 realPos = pos.Value - Vector2.up * _zoomArea.yMin;
        realPos.x = Mathf.Round(realPos.x / 20.0f) * 20.0f;
        realPos.y = Mathf.Round(realPos.y / 20.0f) * 20.0f;
        BlockDrawer bd = BlockFactory.CreateBlockDrawer(BlockShape.Rect, collection, realPos, isStart);
        blockDrawers.Add(bd);
        BlockFactory.CreateBlockAsset(blockDrawers.Last(), blockDrawers.Count == 1);
        Save();
    }

    void AddDiamondNode(object data)
    {
        Vector2? pos = data as Vector2?;
        bool isStart = blockDrawers.Count == 0;
        BlockDrawer bd = BlockFactory.CreateBlockDrawer(BlockShape.Diamond, collection, pos.Value, isStart);
        blockDrawers.Add(bd);
    }

    void DrawArrow(Vector2 sourcePos, Vector2 destPos)
    {
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
    void DrawLink(BlockDrawer source, BlockDrawer destination)
    {
        Vector2 sourcePos = source.blockCentre.Invoke(source.pos, source.blockClass);
        Vector2 destPos = destination.blockCentre.Invoke(destination.pos, destination.blockClass);
        sourcePos += currentWorldOrigin;
        destPos += currentWorldOrigin;

        DrawArrow(sourcePos, destPos);
    }

    void LinkCallback(object userData)
    {
        int? selectedIndex = userData as int?;

        toLinkObjIndex = selectedIndex.Value;
        bdToLink = blockDrawers[selectedIndex.Value];
        Save();
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
        selectObjPos = objPos;// + currentWorldOrigin;
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

    public void HardReset()
    {
        BlockFactory.HardReset();
        foreach (BlockDrawer bd in blockDrawers)
            BlockFactory.DeleteBlockAsset(bd);
    }
    public void UpdateBlockDrawers()
    {
        foreach(BlockDrawer bd in blockDrawers)
        {
            SceneEditor se = BlockFactory.GetBlockAsset(bd);
            if (se == null)
                continue;
            bd.labelText = se.SceneName;
        }
    }
    void DrawGrid()
    {
        var width = position.width;
        var height = position.height;
        Color c = Handles.color;
        Handles.color = new Color(1, 1, 1, 0.25f);
        for (int i = (int)currentWorldOrigin.x % 20; i < width * 2; i += 20)
        {
            Vector2 start = new Vector2(i, 0);
            Vector2 finish = new Vector2(i, height * 2);
            Handles.DrawLine(start, finish, 0.01f * _zoom);
        }
        for(int i = (int)currentWorldOrigin.y % 20; i < height * 2; i+= 20)
        {

            Vector2 start = new Vector2(0, i);
            Vector2 finish = new Vector2(width * 2, i);
            Handles.DrawLine(start, finish, 0.01f * _zoom);
        }
        Handles.color = c;
    }

    void RearrangeBlocks()
    {
        Vector2 InitialPos = new Vector2(60, Mathf.Ceil(_zoomArea.yMin / 20) * 20 + 40);
        float initialY = InitialPos.y;
        int index = -1;
        for (int i = 0; i < blockDrawers.Count; i++)
        {
            var bd = blockDrawers[i];
            if(bd.isStart)
            {
                index = i;
                break;
            }
        }
        BlockDrawer startBlock = blockDrawers[index];
        blockDrawers.RemoveAt(index);
        blockDrawers.Insert(0, startBlock);
        foreach(BlockDrawer bd in blockDrawers)
        {
            bd.pos = InitialPos;
            InitialPos.y += bd.blockClass.size.y * 1.5f;
            if (InitialPos.y + bd.blockClass.size.y > position.height - 2 * bd.blockClass.size.y)
            {
                InitialPos.y = initialY;
                InitialPos.x += bd.blockClass.size.x * 1.5f;
            }
        }
        Save();
    }

}

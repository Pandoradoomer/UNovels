using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(UserSettings))]
public class UserSettingsEditor : Editor
{
    void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }
    public void OnSceneGUI(SceneView sceneView)
    {
        var t = target as UserSettings;
        Camera cam = t.camera;
        Canvas canvas = t.canvas;
        if (t.ShowCharacterTextBox)
        {
            Rect rect = new Rect();
            Vector2 pos = canvas.pixelRect.center;
            //reposition the top-left corner first to be in the middle of the rect;
            pos.y += t.CharacterTextBoxPosition.y * canvas.scaleFactor;
            pos.x += t.CharacterTextBoxPosition.x * canvas.scaleFactor;
            Handles.DrawWireDisc(pos, Vector3.forward, 10);
            pos.y -= t.CharacterTextBoxSize.y * canvas.scaleFactor/ 2;
            pos.x -= t.CharacterTextBoxSize.x * canvas.scaleFactor / 2;
            rect.position = pos;
            rect.size = t.CharacterTextBoxSize * canvas.scaleFactor;
            Handles.DrawSolidRectangleWithOutline(rect, t.TextBoxColor, Color.yellow);
        }
        if (t.ShowNarratorTextBox)
        {
            Rect rect = new Rect();
            Vector2 pos = canvas.pixelRect.center;
            //reposition the top-left corner first to be in the middle of the rect;
            pos.y += t.NarratorTextBoxPosition.y * canvas.scaleFactor;
            pos.x += t.NarratorTextBoxPosition.x * canvas.scaleFactor;
            Handles.DrawWireDisc(pos, Vector3.forward, 10);
            pos.y -= t.NarratorTextBoxSize.y * canvas.scaleFactor / 2;
            pos.x -= t.NarratorTextBoxSize.x * canvas.scaleFactor / 2;
            rect.position = pos;
            rect.size = t.NarratorTextBoxSize * canvas.scaleFactor;
            Handles.DrawSolidRectangleWithOutline(rect, t.TextBoxColor, Color.yellow);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSceneInfo : UID
{
    string sceneName;
    List<Dialogue> dialogueList;

    public BasicSceneInfo(Dialogue d)
    {
        sceneName = "";
        dialogueList = new List<Dialogue>();
        dialogueList.Add(d);
    }
    public BasicSceneInfo()
    {
        sceneName = "";
        dialogueList = new List<Dialogue>();
    }
    public BasicSceneInfo(string sceneName) : base()
    {
        this.sceneName = sceneName;
        dialogueList = new List<Dialogue>();
    }

}

public struct Dialogue
{
    string dialogue;
    float speed;

    public Dialogue(string dialogue, float speed)
    {
        this.dialogue = dialogue;
        this.speed = speed;
    }
}

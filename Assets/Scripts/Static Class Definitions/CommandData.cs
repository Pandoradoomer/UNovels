using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ScreenPosition
{
    LEFT,
    CENTRE,
    RIGHT
}
public enum CommandType
{
    SAY,
    WAIT,
    MOVE,
    SHOW,
    SPRITE
}
[Serializable]
public class CommandData
{
    public CommandType type;
    public string dialogueText;
    public CharacterData Character;
    public ScreenPosition LocationTo;
    public TransitionTypes TransitionType;
    public string emotion;
    public float Time;
    public bool IsShow;
    public bool Refresh;

}

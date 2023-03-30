using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CommandData
{
    [Tooltip("Accepts HTML tags for formatting")]
    [TextAreaAttribute(15, 20)]
    public string dialogueText;
    [Tooltip("The character assigned to the dialogue piece")]
    public CharacterData Character;
    public CommandType type;

}

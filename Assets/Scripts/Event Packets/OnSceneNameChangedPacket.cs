using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSceneNameChangedPacket : IEventPacket
{
    public Guid guid;
    public string name;
}

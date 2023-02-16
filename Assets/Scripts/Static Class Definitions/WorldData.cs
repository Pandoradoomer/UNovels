using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
    public SerializableVector2 currentWorldOrigin;

    [JsonConstructor]
    public WorldData(SerializableVector2 currentWorldOrigin)
    {
        this.currentWorldOrigin = currentWorldOrigin;
    }
}

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
    public SerializableVector2 currentWorldOrigin;
    public float currentZoomValue;

    [JsonConstructor]
    public WorldData(SerializableVector2 currentWorldOrigin, float currentZoomValue)
    {
        this.currentWorldOrigin = currentWorldOrigin;
        this.currentZoomValue = currentZoomValue;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;


public class UID 
{
    public Guid uid;

    public UID()
    {
        uid = Guid.NewGuid();
    }
}

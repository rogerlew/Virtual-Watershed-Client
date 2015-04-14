﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class GeoReference
{
    // Fields
    String type;
    public List<DataRecord> records;

    // We will have to rebuild this or come up with a different serialization mechanism -- Unity's Serializer
    [NonSerializedAttribute]
    public GameObject gameObject;
    [NonSerializedAttribute]
    Texture2D texture;

    private DataRecord rec;

    // Constructor
    public GeoReference()
    {

    }
    public GeoReference(string refType)
    {
        type = refType;
    }

    public GeoReference(DataRecord rec)
    {
        if(records == null)
        {
            records = new List<DataRecord>();
        }
        records.Add(rec);
    }

    // Methods
    public void updateRecords() 
    { 
        // TODO
    }

    public void build()
    {
        // TODO
    }

    // TODO getters? setters?
}

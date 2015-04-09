using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeoReference
{
    // Fields
    String type;
    List<DataRecord> recs;
    GameObject obj;
    Texture2D texture;
    private DataRecord rec;

    // Constructor
    public GeoReference(string refType)
    {
        type = refType;
    }

    public GeoReference(DataRecord rec)
    {
        recs.Add(rec);
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

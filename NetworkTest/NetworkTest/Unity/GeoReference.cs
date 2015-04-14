using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GeoReference
{
    // Fields
    String type;
    public List<DataRecord> recs;

    // We will have to rebuild this or come up with a different serialization mechanism -- Unity's Serializer
    [NonSerializedAttribute]
    GameObject obj;
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
        if(recs == null)
        {
            recs = new List<DataRecord>();
        }
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

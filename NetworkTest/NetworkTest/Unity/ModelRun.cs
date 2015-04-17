using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// ModelRun class
/// </summary>
class ModelRun
{
    // Fields
    private string name;
    private Dictionary<string, GeoReference> references = new Dictionary<string, GeoReference>();

    public DateTime Start;
    public DateTime End;

    public string ModelName;
    public string ModelDataSetType;
    public string ModelRunUUID;
    // Constructors
    public ModelRun(string modelRunName,string modelRunUUID)
    {
        ModelName = modelRunName;
        ModelRunUUID = modelRunUUID;
    }

    // Methods
    public void addToModel(string label, GeoReference toAdd)
    {
        // Check if already in the model
        if( references.ContainsKey(label) )
        {
            // Label already exists
            // Handle situation?
        }
        else
        {
            // Add to the model run
            references.Add(label, toAdd);
        }
    }

    /// <summary>
    /// Determines whether a DataRecord belongs to this particular ModelRun
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    public bool BelongsTo(DataRecord record)
    {
        return record.modelname == ModelName && record.modelRunUUID == ModelRunUUID;
    }

    /// <summary>
    /// Inserts a Records into this ModelRun if it belongs..
    /// </summary>
    /// <param name="records"></param>
    /// <returns></returns>
    public bool Insert(DataRecord record)
    {
        if(!BelongsTo(record))
        {
            return false;
        }
        // Determine whether or not we need to create a new georef object...

        if (!references.ContainsKey(record.variableName))
        {
            references[record.variableName] = new GeoReference();
        }

        // Insert data record into appopritate georef object --- if it doesn't already exist in this model run..
        if (!references[record.variableName].records.Contains(record))
        {
            references[record.variableName].records.Add(record);
        }
        
        return false;
    }

    public GeoReference getReference(string label)
    {
        return references[label];
    }

    public int Count()
    {
        return references.Count;
    }
}

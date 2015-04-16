using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ModelRun
{
    // Fields
    private string name;
    private Dictionary<string, GeoReference> references = new Dictionary<string, GeoReference>();

    // Constructors
    public ModelRun(string modelRunName)
    {
        name = modelRunName;
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

    public GeoReference getReference(string label)
    {
        return references[label];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// ModelRun class
/// </summary>
public class ModelRun
{
    // Fields
    private string Name;
    public string ModelName;
    public string ModelDataSetType;
    public string ModelRunUUID;

    public DateTime Start;
    public DateTime End;

    public ModelRunManager MRM;
    // private Dictionary<string, GeoReference> references = new Dictionary<string, GeoReference>();
    private Dictionary<string, List<DataRecord>> references = new Dictionary<string, List<DataRecord>>();

    // Constructors
    public ModelRun(string modelRunName,string modelRunUUID)
    {
        ModelName = modelRunName;
        ModelRunUUID = modelRunUUID;
    }

    public ModelRun(string modelRunName, string modelRunUUID,ModelRunManager GM)
    {
        ModelName = modelRunName;
        ModelRunUUID = modelRunUUID;
        MRM = GM;
    }

    // Methods
    //public void addToModel(string label, GeoReference toAdd)
    public void Add(string label, List<DataRecord> toAdd)
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
        // Check if the record belongs to the model run
        if(!BelongsTo(record)) { return false; }

        // Determine whether or not we need to create a new List<DR>
        if( ! references.ContainsKey(record.variableName) )
        {
            references[record.variableName] = new List<DataRecord>();
        }

        // Insert data record into appopritate georef object --- if it doesn't already exist in this model run..
        if( ! references[record.variableName].Contains(record) )
        {
            references[record.variableName].Add(record);
        }
        
        // Return
        return true;
    }

    public List<DataRecord> Get(string label)
    {
        return references[label];
    }

    public int Count()
    {
        return references.Count;
    }

    // Other params
    public void DownloadDatasets(bool all=true,string operation="wcs",SystemParameters param=null)
    {
        // Create a default system parameter object
        if(param == null) { param = new SystemParameters(); }

        // Ensure the GRM reference is valid
        if(MRM == null) { return; }

        // TODO Need to fill out the parameters

        // If all records are requests
        if(all)
        {
            // Simple Download Operation...
            foreach(var i in references)
            {
                MRM.Download(i.Value, null, "vwc", operation, param);
            }
        }

        // We can change the order of the downloads as opposed to downloading all.
    }

    public void FetchAll(string ModelVar,string service="wms",SystemParameters param=null)
    {
        // Let the downloading begin!
        foreach (var i in references[ModelVar])
        {
            List<DataRecord> Record = new List<DataRecord>();
            Record.Add(i);
            MRM.Download(Record, null, "vwc", service, param);
        }
    }

    // For filebased simulations .... This is just case we don't have any data in June.
    public void FileBasedFetch()
    {

    }
    
    public List<DataRecord> Query(bool usingOR=true, int number=0, string name="", string Type="", string starttime="", string endtime="", string state="", string modelname="")
    {
        // Initialize variables
        int count = 0;
        List<DataRecord> records = new List<DataRecord>();

        // iterate through the list of datarecords __ variables in this case
        foreach (var variable in references)
        {
            // iterate through the list of datarecords contain in this paricular variable
            foreach(var record in variable.Value)
            {
                // Check if the case where everything is desired
                if(number <= 0)
                {
                    // Add the record
                    records.Add(record);
                }
                else if(count == number)
                {
                    // Return what you have
                    return records;
                }
                else if(usingOR)
                {
                    // Check record using OR-Query
                    if(record.modelname == modelname || record.name == name || record.Type == Type || 
                        record.start.ToString() == starttime || record.end.ToString() == endtime || 
                        record.state == state )
                    {
                        // Add the record
                        records.Add(record);
                        count++;
                    }
                }
                else if(!usingOR)
                {
                    // Check record using AND-Query
                    if ( (modelname == "" || modelname == this.ModelName) &&
                         (name == "" || name == record.name ) &&
                         (Type == "" || Type == record.Type ) &&
                         (starttime == "" || starttime == record.start.ToString()) &&
                         (endtime == "" || endtime == record.end.ToString()) &&
                         (state == "" || state == record.state)
                        )
                    {
                        // Add the record
                        records.Add(record);
                        count++;
                    }
                }
            }
        }
        return records;
    }

    public DataRecord FetchNearestDataPoint(string VariableName, DateTime pointOfInterest)
    {
        // Check if ModelRun exists here
        if(!references.ContainsKey(VariableName))
        {
            return null;
        }

        // For now a linear search -- This can be improved
        foreach(var i in references[VariableName])
        {
            if(i.start != null && i.end != null && pointOfInterest <= i.end && pointOfInterest >= i.start)
            {
                return i;
            }
        }

        return null;
    }
}

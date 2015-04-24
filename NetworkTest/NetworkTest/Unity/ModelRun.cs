using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// ModelRun class
/// </summary>
public class ModelRun
{
    // We need a lock for the model run class due to the curse of caching
    readonly object LOCK; // Probably 

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
    /// Used by the Simulation class to fetch data.
    /// </summary>
    /// <param name="ModelVar"></param>
    /// <param name="CurrentTimeFrame"></param>
    public bool HasData(string ModelVar, int CurrentTimeFrame,bool texture=false)
    {
        //Need to determine the type of download.
        if (!texture)
        {

        }
        else
        {

        }
        // Returns whether it has the data...
        return false;
    }

    public void DownloadData(string ModelVar, int CurrentTimeFrame,bool texture=false)
    {
        //Need to determine the type of download.
        if(!texture)
        {

        }
        else
        {

        }
        // Handle the download here
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


    // These can be replaced with inserts from datarecords.
    public DateTime GetBeginModelTime()
    {
        DateTime time = DateTime.MaxValue;
        Logger.WriteLine(time.ToString());
        foreach (var i in references)
        {
            Logger.WriteLine(i.Value.Count.ToString());
            Logger.WriteLine("ORIGINAL: " + references[i.Key][0].start);
            foreach(var j in i.Value)
            {
                Logger.WriteLine(j.start.ToString());
                if(time > j.start)
                {
                    time = j.start;
                }
            }
        }
        Logger.WriteLine("BEGINNING OF TIME!!!!!!! " + time);
        return time;
    }

    public DateTime GetEndModelTime()
    {
        DateTime time = DateTime.MinValue;
        Logger.WriteLine(time.ToString());
        foreach (var i in references)
        {
            Logger.WriteLine(i.Value.Count.ToString());
            Logger.WriteLine("ORIGINAL: " + i.Value[0].end);
            foreach (var j in i.Value)
            {
                Logger.WriteLine(j.end.ToString());
                if (j.end > time)
                {
                    time = j.end;
                }
            }
        }
        Logger.WriteLine("END OF TIME!!!!!!! " + time);
        return time;
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


    // Use the default timestep contained within this class

    /// <summary>
    /// An update function that is to give the next step in a simulation.
    /// </summary>
    /// <param name="ModelVar"></param>
    /// <param name="CurrentTimeStep"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    public int Update(string ModelVar, int CurrentTimeStep, DateTime current)
    {
        //Logger.WriteLine("LOG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        //if (SetToStepThrough == null || previousRecord < 0 || SetToStepThrough.Count <= previousRecord)
        {
            // Throw Error
            //return;
        }


        // Creating a compartor
        DataRecordComparers.StartDateDescending compare = new DataRecordComparers.StartDateDescending();

        // Sort the list..
        if (!references.ContainsKey(ModelVar))
            return -1;
        references[ModelVar].Sort(compare);

        // Find next record -- Assuming that the list is in order at this point.
        for (int i = CurrentTimeStep + 1; i < references[ModelVar].Count; i++)
        {
            //Logger.WriteLine(SetToStepThrough[previousRecord].start.ToString() + " " + SetToStepThrough[previousRecord].end.ToString());
            //  Logger.WriteLine(SetToStepThrough[i].start.ToString() + " " + SetToStepThrough[i].end.ToString());
            //Logger.WriteLine(current.ToString());
            if (references[ModelVar][i].start >= current)
            {
                //TimeSpan delta = start - NextTime;
                return i;
            }
        }
        return -1;
    }
}

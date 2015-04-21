using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// This for notifying other objects that the georeference list has been populated.
/// This may be replaced with a observer that notifies objects.... maybe.
/// </summary>
/// <param name="RecievedGeoRefs"></param>
public delegate void GeoRefMessage(List<string> RecievedGeoRefs);

public class GeoRefManager
{
    // Fields
    VWClient client;
    Dictionary<string, GeoReference> storedGeoRefs = new Dictionary<string, GeoReference>();

    // TODO When a georef added, check which model run it belongs to and save the model run here
    // Questions that need to be answered: 
    // Should ModelRuns (a reference to) be stored in GeoRefs? (This will create a cycle of references GeoRef_1 -> ModelRun -> GeoRef_1 
    // ~ Half Answered use the GeoRefManager to get the original model run
    Dictionary<string, ModelRun> storedModelRuns = new Dictionary<string, ModelRun>();
    

    // Filebased Cache entry.
    string cacheBackupEntry = "backup";
    string cacheRestoreEntry = "restore";

    // Utilities Utilities where for art thou Utilites 
    Utilities utilities = new Utilities();

    // NOTE: Some way of sorting and returning back a sorted list based on metadata
    // NOTE: On download, a GeoRef might already exist. No code has been written to handle that case other than a check.

    public void Start()
    {
        // Load up cache?
        if(FileBasedCache.Exists(cacheRestoreEntry))
        {
            Logger.WriteLine("Restoring Previous Session");
            storedGeoRefs = FileBasedCache.Get<Dictionary<string, GeoReference>>(cacheRestoreEntry);
        }

        // Initialize everything.....

    }

    // Constructors
    public GeoRefManager(VWClient refToClient)
    {
        client = refToClient;
    }

    // Methods
    public GeoReference request(string key)
    {
        return storedGeoRefs[key];
    }

    public void buildObject(string recordname,string buildType="")
    {
        // TODO
        // We need the utilities class here. -- This is in the Unity code right now...
        GeoReference obj = storedGeoRefs[recordname];
        
        // We need to use this obj to determine what to build.

        // if terrain build terrain
        if(buildType == "terrain")
        {
            obj.gameObject = utilities.buildTerrain(obj.records[0]);
        }
        // else if texture build texture
        else if( buildType == "texture")
        {
            // To be done 
        }

        // else if shape build shape
        else if( buildType == "shape")
        {
            obj.gameObject = utilities.buildShape(obj.records[0]);
        }
    }

    // NOTE: Populating the data inside a datarecord. Something like building the texture.
    // Gonna need a parameter object for this ----- for now just defaults
    public void download(List<DataRecord> records, DataRecordSetter SettingTheRecord , string service = "vwc", string operation = "wms",SystemParameters param=null)
    {
        if(param == null)
        {
            param = new SystemParameters();
        }
        // TODO 
        if(service== "vwc")
        {
            
            if(operation=="wms")
            {
                param.width = 100;
                param.height = 100;
                client.getMap(SettingTheRecord,records[0], param);
            }
            else if(operation=="wcs")
            {
                client.getCoverage(SettingTheRecord,records[0], param);
            }
            else if(operation=="wfs")
            {
                client.getFeatures(SettingTheRecord,records[0], param);
            }
            else if(operation=="fgdc")
            {
                client.GetMetaData(SettingTheRecord,records);
            }
        }
    }

    // NOTE: Look inside own dictionary for georef that matches the parameters
    public List<string> query(int number=0, string name="", string TYPE="", string starttime="", string endtime="", string state="", string modelname="")
    {
        // Everything in here will be a or operation.....
        List<string> georefs = new List<string>();
        int count = 0;
        if(number != 0)
        {
            foreach (var i in storedGeoRefs)
            {
                foreach (var j in i.Value.records)
                {
                    count++;
                    georefs.Add(i.Key);
                    break;
                }
                if(count == number)
                {
                    return georefs;
                }
            }
        }
        // need the metadata module here.....
        foreach (var i in storedGeoRefs)
        {
            foreach(var j in i.Value.records)
            {
                if(j.name == name || j.TYPE == TYPE || starttime == j.start.ToString() || endtime == j.start.ToString() || state == j.state || modelname == j.modelname )
                {
                    georefs.Add(i.Key);
                    break;
                }
            }
        }

        // Return the Georeferences
        return georefs;
    }

    // NOTE: Build a parameter struct (name of struct = ServiceParameters)
    public void getAvailable(SystemParameters param, GeoRefMessage Message=null)
    {
        // TODO
        client.RequestRecords(((List<DataRecord> records) =>onGetAvailableComplete(records,Message)), param);
    }
    /// <summary>
    /// This needs to be tested.
    /// </summary>
    /// <param name="Records"></param>
    /// <param name="message"></param>
    private void onGetAvailableComplete(List<DataRecord> Records,GeoRefMessage message)
    {
        List<string> RecievedRefs = new List<string>();
        foreach(DataRecord rec in Records)
        {
            Logger.WriteLine(rec.modelRunUUID);
            // We should play with the other of the if statements...
            // Normal Case
            if(storedModelRuns.ContainsKey(rec.modelRunUUID))
            {
                // Call insert operation
                Logger.WriteLine("ADDED");
                storedModelRuns[rec.modelRunUUID].Insert(rec);
            }
            // Cache Case
            else if(FileBasedCache.Exists(rec.modelRunUUID))
            {
                // Handle it
            }
            // Normal Case
            else if(!storedModelRuns.ContainsKey(rec.modelRunUUID))
            {
                // Cache Case -- Check if cache has a georef

                // Normal Case -- Insert it into storedModelRuns
                Logger.WriteLine("ADDED");
                storedModelRuns.Add(rec.modelRunUUID,new ModelRun(rec.modelname,rec.modelRunUUID,this));

                // Call the insert
                storedModelRuns[rec.modelRunUUID].Insert(rec);

                //  Testing Variable
                if(!RecievedRefs.Contains(rec.modelRunUUID))
                {
                    RecievedRefs.Add(rec.modelRunUUID);
                }
            }
        }
        foreach(var i in storedModelRuns)
        {
            Logger.WriteLine(i.Value.Count().ToString());
        }
        if(message != null)
        {
            message(RecievedRefs);
        }
        Logger.WriteLine("CREATED THIS MANY MODEL RUNS: " + storedModelRuns.Count);
        foreach(var i in storedModelRuns)
        {
            i.Value.DownloadDatasets();
        }
    }

    public void OnClose()
    {
        // Save things to cache
        FileBasedCache.Insert<Dictionary<string, GeoReference>>(cacheRestoreEntry, storedGeoRefs);
        // or should we clear the cache!!!!!
    }

    /// Closing on terrain 
    /// load cache on startup ---- check
    /// Closing on Program --- check
    /// Cache Model runs
    /// Cache User parameters
}

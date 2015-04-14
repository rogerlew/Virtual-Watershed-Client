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
public delegate void georefMessage(List<string> RecievedGeoRefs);

public class GeoRefManager 
{
    // Fields
    VWClient client;
    Dictionary<string, GeoReference> stored = new Dictionary<string, GeoReference>();
    

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
            Console.WriteLine("Restoring Previous Session");
            stored = FileBasedCache.Get<Dictionary<string, GeoReference>>(cacheRestoreEntry);
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
        return stored[key];
    }

    public void buildObject(string recordname,string buildType="")
    {
        // TODO
        // We need the utilities class here. -- This is in the Unity code right now...
        GeoReference obj = stored[recordname];
        
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
    public void download(List<DataRecord> records, DataRecordSetter SettingTheRecord , string recordname, string service = "vwc", string operation = "wms")
    {
        // TODO 
        if(service== "vwc")
        {
            if(operation=="wms")
            {
                client.getMap(SettingTheRecord,records[0]);
            }
            else if(operation=="wcs")
            {
                client.getCoverage(SettingTheRecord,records[0]);
            }
            else if(operation=="wfs")
            {
                client.getFeatures(SettingTheRecord,records[0]);
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
            foreach (var i in stored)
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
        foreach (var i in stored)
        {
            foreach(var j in i.Value.records)
            {
                if(j.name == name | j.TYPE == TYPE | starttime == j.start.ToString() | endtime == j.start.ToString() | state == j.state | modelname == j.modelname )
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
    public void getAvailable(int offset, int limit, string model_set_type = "vis", string service = "", string query = "", string starttime = "", string endtime = "", string location = "", string state = "", string modelname = "", string timestamp_start = "", string timestamp_end = "", string model_vars = "", DownloadType type = DownloadType.Record, string OutputPath = "", string OutputName = "", georefMessage Message=null)
    {
        // TODO
        client.RequestRecords(((List<DataRecord> records) =>onGetAvailableComplete(records,Message)), offset, limit, model_set_type, service, query, starttime, endtime, location, state, modelname, timestamp_start, timestamp_end, model_vars, type, OutputPath, OutputName);
    }

    private void onGetAvailableComplete(List<DataRecord> Records,georefMessage message)
    {
        List<string> RecievedRefs = new List<string>();
        foreach(DataRecord rec in Records)
        {
            if( stored.ContainsKey(rec.name + rec.id) )
            {
                // GeoRef already exists! Do something about it!
                // Ignore for now
                continue;
            }
            else if(FileBasedCache.Exists(rec.name + rec.id))
            {
                // overwrite or ignore
                continue;
                //stored[rec.name + rec.id] = FileBasedCache.Get<GeoReference>(rec.name + rec.id));
            }
            else
            {
                Console.WriteLine("ADDED");
                // Add the record
                stored.Add(rec.name + rec.id,
                    new GeoReference(rec));
                RecievedRefs.Add(rec.name + rec.id);

                // Should we write a class that keeps a record of everything in the cache at the moment -- so we don't have to do constant file I/O...
                // This class would act like a sudo in-memory cache for both standalone and web player. 
                FileBasedCache.Insert<GeoReference>(rec.name + rec.id, stored[rec.name + rec.id]);
            }
        }
        if(message != null)
        {
            message(RecievedRefs);
        }
    }

    public void OnClose()
    {
        // Save things to cache
        FileBasedCache.Insert<Dictionary<string, GeoReference>>(cacheRestoreEntry, stored);
        // or should we clear the cache!!!!!
    }

    /// Closing on terrain 
    /// load cache on startup ---- check
    /// Closing on Program --- check
    /// Cache Model runs
    /// Cache User parameters
}

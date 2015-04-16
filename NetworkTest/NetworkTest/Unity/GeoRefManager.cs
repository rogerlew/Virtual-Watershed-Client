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
    Dictionary<string, GeoReference> storedGeoRefs = new Dictionary<string, GeoReference>();

    // TODO When a georef added, check which model run it belongs to and save the model run here
    // Questions that need to be answered: 
    // Should we have an addModelRun() function? (And then just check the string field of the GeoRef to know which ModelRun it belongs to)
    // Should ModelRuns (a reference to) be stored in GeoRefs? (This will create a cycle of references GeoRef_1 -> ModelRun -> GeoRef_1
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
    public void download(List<DataRecord> records, DataRecordSetter SettingTheRecord , string recordname, string service = "vwc", string operation = "wms")
    {
        // TODO 
        if(service== "vwc")
        {
            SystemParameters param = new SystemParameters();
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
    public void getAvailable(SystemParameters param, georefMessage Message=null)
    {
        // TODO
        client.RequestRecords(((List<DataRecord> records) =>onGetAvailableComplete(records,Message)), param);
    }

    private void onGetAvailableComplete(List<DataRecord> Records,georefMessage message)
    {
        List<string> RecievedRefs = new List<string>();
        foreach(DataRecord rec in Records)
        {
            if( storedGeoRefs.ContainsKey(rec.name + rec.id) )
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
                Logger.WriteLine("ADDED");
                // Add the record
                storedGeoRefs.Add(rec.name + rec.id,
                    new GeoReference(rec));
                RecievedRefs.Add(rec.name + rec.id);

                // Should we write a class that keeps a record of everything in the cache at the moment -- so we don't have to do constant file I/O...
                // This class would act like a sudo in-memory cache for both standalone and web player. 
                FileBasedCache.Insert<GeoReference>(rec.name + rec.id, storedGeoRefs[rec.name + rec.id]);
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
        FileBasedCache.Insert<Dictionary<string, GeoReference>>(cacheRestoreEntry, storedGeoRefs);
        // or should we clear the cache!!!!!
    }

    /// Closing on terrain 
    /// load cache on startup ---- check
    /// Closing on Program --- check
    /// Cache Model runs
    /// Cache User parameters
}

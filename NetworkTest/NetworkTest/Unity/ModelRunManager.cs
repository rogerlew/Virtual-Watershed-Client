﻿using System;
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

public class ModelRunManager
{
    // Fields
    VWClient client;

    // Single List<DR> can be stored under a generic model run
    // Dictionary<string, GeoReference> storedGeoRefs = new Dictionary<string, GeoReference>();
    ModelRun generalModelRun = new ModelRun("general", ""); // Update later?

    // TODO When a georef added, check which model run it belongs to and save the model run here
    // Questions that need to be answered: 
    // Should ModelRuns (a reference to) be stored in GeoRefs? (This will create a cycle of references GeoRef_1 -> ModelRun -> GeoRef_1 
    // ~ Half Answered use the GeoRefManager to get the original model run
    Dictionary<string, ModelRun> modelRuns = new Dictionary<string, ModelRun>();
    
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
            
            // Don't know how to fix
            //storedGeoRefs = FileBasedCache.Get<Dictionary<string, GeoReference>>(cacheRestoreEntry);
        }
        // Initialize everything.....
    }

    // Constructors
    public ModelRunManager(VWClient refToClient) { client = refToClient; }

    // Methods
    public ModelRun Get(string key)
    {
        return modelRuns[key];
    }

    // TODO: Needs to be moved
    /* public void buildObject(string recordname,string buildType="")
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
    } */

    // NOTE: Populating the data inside a datarecord. Something like building the texture.
    // Gonna need a parameter object for this ----- for now just defaults
    // TODO Consider Removing DataRecordSetter
    public void Download(List<DataRecord> records, DataRecordSetter SettingTheRecord, string service = "vwc", string operation = "wms", SystemParameters param=null)
    {
        // Create param if one does not exist
        if(param == null) { param = new SystemParameters(); }

        // TODO 
        if(service == "vwc")
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

    // We will return the data records that are specific to the query.
    public List<DataRecord> Query(int number=0, string name="", string TYPE="", string starttime="", string endtime="", string state="", string modelname="")
    {
        List<DataRecord> records = new List<DataRecord>();
        // Check in the list of model runs based on the query parameters everything will be an or operation...
        foreach (var i in modelRuns)
        {
            // Query inside of model run class... ---- AddRange append lists ---- I wonder what happens if you do List.AddRange(List) 
            records.AddRange(i.Value.Query(number,name,TYPE,starttime,endtime,state,modelname));
        }
        return records;
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
            if(modelRuns.ContainsKey(rec.modelRunUUID))
            {
                // Call insert operation
                Logger.WriteLine("ADDED");
                modelRuns[rec.modelRunUUID].Insert(rec);
            }
            // Cache Case
            else if(FileBasedCache.Exists(rec.modelRunUUID))
            {
                // Handle it
            }
            // Normal Case
            else if(!modelRuns.ContainsKey(rec.modelRunUUID))
            {
                // Cache Case -- Check if cache has a georef

                // Normal Case -- Insert it into storedModelRuns
                Logger.WriteLine("ADDED");
                modelRuns.Add(rec.modelRunUUID,new ModelRun(rec.modelname,rec.modelRunUUID,this));

                // Call the insert
                modelRuns[rec.modelRunUUID].Insert(rec);

                //  Testing Variable
                if(!RecievedRefs.Contains(rec.modelRunUUID))
                {
                    RecievedRefs.Add(rec.modelRunUUID);
                }
            }
        }
        foreach(var i in modelRuns)
        {
            Logger.WriteLine(i.Value.Count().ToString());
        }
        if(message != null)
        {
            message(RecievedRefs);
        }
        Logger.WriteLine("CREATED THIS MANY MODEL RUNS: " + modelRuns.Count);
        foreach(var i in modelRuns)
        {
            i.Value.DownloadDatasets();
        }
    }

    public void OnClose()
    {
        // Save things to cache
        //FileBasedCache.Insert<Dictionary<string, GeoReference>>(cacheRestoreEntry, storedGeoRefs);
        // or should we clear the cache!!!!!
    }

    /// Closing on terrain 
    /// load cache on startup ---- check
    /// Closing on Program --- check
    /// Cache Model runs
    /// Cache User parameters
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.IO;



/**
 * Author: Chase Carthen
 * Class: VWClient
 * Description: The following class represents the virtual watershed. 
 */
public class VWClient : Observer
{
    struct DataRecordJob
    {
        public List<DataRecord> records;
        public string jobname;
    }
    public string Name;
    public string App = "//apps/vwp";
    public string Description;
    public string Root;
    DataFactory factory;
    NetworkManager manager;
    ThreadSafeDictionary<string, Thread> Threads = new ThreadSafeDictionary<string, Thread>(); // Alternative to Unity's threadpool

    // Needed still? 
    ThreadSafeDictionary<string, KeyValueTriple<string, string, DataRecord>> Requests = new ThreadSafeDictionary<string, KeyValueTriple<string, string, DataRecord>>();

    // Holds URL -> Observable
    ThreadSafeDictionary<string, Observerable> active = new ThreadSafeDictionary<string, Observerable>();

    // Holds requests that are waiting
    Queue<Observerable> waiting = new Queue<Observerable>();

    int Limit = 10;
    

    Random R = new Random(1);
    public string GenerateToken(string FunctionName)
    {
        string Token = FunctionName + " " + R.Next();

        // Generate Random Token -- Wait until a random value is found that works ... We could do a counter system instead of random.
        Token = FunctionName + " " + R.Next();

        return Token;
    }

    public VWClient(DataFactory datafactory, NetworkManager networkmanager, string root = "http://vwp-dev.unm.edu")
    {
        factory = datafactory;  // Added by constructor instead of building a new one inside here
        manager = networkmanager;   // Added so worker threads can call events
        Root = root;
    }
	
    public override void OnDataComplete(string url)
    {
        if (active.ContainsKey(url))
        {
            active[url].CallBack();
            active.Remove(url);
            if (waiting.Count > 0)
            {
                var job = waiting.Dequeue();
                AddObservable(job);
            }
        }
    }

    public override void OnDataError(string url)
    {
        Logger.WriteLine("There was an error in: " + url);
        if(active.ContainsKey(url))
        {
            active.Remove(url);
            if (waiting.Count > 0)
            {
                var job = waiting.Dequeue();
                AddObservable(job);
            }

            // Send second error message to notify other observers there was an error with obtaining the data record?

        }
        base.OnDataError(url);
    }

    public override void OnDownloadComplete(string url)
    {
        // Loop through the active
        if( active.ContainsKey(url) )
        {
            // Update
            string result = active[url].Update();
            Logger.WriteLine("RESULT: " + result);
            // Check if complete
            if (result == "COMPLETE")
            {
                // Remove from active
                //active.Remove(url);

                // Call OnDataComplete event
                manager.CallDataComplete(url);
            }
            else if (result == "")
            {
                // Error code
                // Print Some Error Here.
            }
            else
            {
                // Add new request
                active[result] = active[url];

                // Remove old <url, observable>
                active.Remove(url);
            }
        }
    }


    /// <summary>
    /// Adds an observable to the list
    /// </summary>
    /// <param name="observable"></param>
    /// <returns></returns>
    void AddObservable(Observerable observable)
    {
        /// Add Lock here
        // If the number active is at threshold, move into waiting
        if (Limit == active.Count())
        {
            waiting.Enqueue(observable);
        }
        // Else Add the observable to "active" and "observables"
        else
        {   
            string URL = observable.Update();
            active[URL] = observable;
        }
    }

    //public void getCoverage(DataRecordSetter Setter, DataRecord Record, string crs = "", string BoundingBox = "", int Width = 0, int Height = 0, string Interpolation = "nearest", DownloadType type = DownloadType.Record, string OutputPath = "", string OutputName = "") // Parameters TODO
    public void getCoverage(DataRecordSetter Setter, DataRecord Record, SystemParameters param)
    {
        // Build a WCS observable
        Logger.WriteLine("GETCOVERAGE");
        var client = new WCSClient(factory, param.type, param.outputPath, param.outputPath);
        client.GetData(Record, param);
        client.Token = GenerateToken("GetCoverage");
        client.callback = Setter;
        AddObservable(client);
    }

    //public void getMap(DataRecordSetter Setter, DataRecord record, int Width = 100, int Height = 100, string Format = "image/png", DownloadType type = DownloadType.Record, string OutputPath = "", string OutputName = "") // Parameters TODO
    public void getMap(DataRecordSetter Setter, DataRecord Record, SystemParameters param)
    {
        Logger.WriteLine("GetMap");
        // Build a WMS observable
        var client = new WMSClient(factory, param.type, param.outputPath, param.outputName);
        client.Token = GenerateToken("GetMap");
        client.App = App;
        client.Root = Root;
        client.GetData(Record, param);
        client.callback = Setter;
        AddObservable(client);
    }

    //public void getFeatures(DataRecordSetter Setter, DataRecord record, string Version = "1.0.0", DownloadType type = DownloadType.Record, string OutputPath = "", string OutputName = "") // Parameters TODO
    public void getFeatures(DataRecordSetter Setter, DataRecord Record, SystemParameters param)
    {
        // Build a WFS observable
        var client = new WFSClient(factory, param.type, param.outputPath, param.outputName);
        client.App = App;
        client.Root = Root;
        client.GetData(Record, param);
        client.Token = GenerateToken("GetFeature");
        client.callback = Setter;
        AddObservable(client);
    }

    bool Activity()
    {
        return Threads.Count() != 0;
    }

    public string RequestRecords(DataRecordSetter Setter, SystemParameters param)
    {
        List<DataRecord> Records = new List<DataRecord>();

        // Make a request to the root html service. -- Need to pass in a call back function...
        string req = Root + App + "/search/datasets.json" + "?offset=" + param.offset + "&model_set_type=" + param.model_set_type + "&limit=" + param.limit + "&version=3";
        if (param.starttime != "" && param.endtime != "")
        {
            req += "&startime=" + param.starttime + "&endtime=" + param.endtime;
        }
        if (param.query != "")
        {
            req += "&query=" + param.query;
        }
        if (param.location != "")
        {
            req += "&location=" + param.location;
        }
        if (param.modelname != "")
        {
            req += "&modelname=" + param.modelname;
        }
        if (param.state != "")
        {
            req += "&state=" + param.state;
        }
        if (param.timestamp_start != "")
        {
            req += "&timestamp_start=" + param.timestamp_start;
        }
        if (param.timestamp_end != "")
        {
            req += "&timestamp_end=" + param.timestamp_end;
        }
        if (param.model_vars != "")
        {
            req += "&model_vars=" + param.model_vars;
        }
            
        // Make the request and enqueue it...
        // Request Download -- 
        //DataRecordJob Job = new DataRecordJob();
        var Obs = new GenericObservable(factory, param.type, param.outputPath, param.outputName);
        Obs.callback = Setter;
        Obs.Token = GenerateToken("RequestRecords");
        Obs.Request(Records, DownloadRecords2, req);
        AddObservable(Obs);
        //DataRecords[req] = Job;

        // Need to put the setter somewhere.

        return req;
    }

    void DownloadRecords2(string url,List<DataRecord> Records)
    {
        factory.Import("VW_JSON", Records, "url://" + url);
    }

    /// Metadata function needs a setter as well
    /// 
    public void GetMetaData(DataRecordSetter Setter, List<DataRecord> records,DownloadType type=DownloadType.Record,string OutputPath="",string OutputName="")
    {
        // One Record only for this function
        if (!records[0].services.ContainsKey("xml_fgdc"))
        {
            return;
        }
        string xml_url = records[0].services["xml_fgdc"];
        // Register Job with Data Tracker
        var Obs = new GenericObservable(factory,type,OutputPath,OutputName);
        Obs.callback = Setter;

        // Generate Token
        Obs.Token = GenerateToken("RequestRecords");

        // 
        Obs.Request(records, GetMetaData, xml_url);
        AddObservable(Obs);
    }

    void GetMetaData(string url,List<DataRecord> Records)
    {
        factory.Import("VW_FGDC", Records, "url://" + url);
    }
}


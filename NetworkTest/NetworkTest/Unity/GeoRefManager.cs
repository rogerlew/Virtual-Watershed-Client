using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GeoRefManager
{
    // Fields
    VWClient client;
    Dictionary<string, GeoReference> stored = new Dictionary<string, GeoReference>();

    // NOTE: Some way of sorting and returning back a sorted list based on metadata
    // NOTE: On download, a GeoRef might already exist. No code has been written to handle that case other than a check.

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

    public void buildObject(string recordname)
    {
        // TODO
    }

    // NOTE: Populating the data inside a datarecord. Something like building the texture.
    public void download(string recordname, string service = "vwc")
    {
        // TODO
    }

    // NOTE: Look inside own dictionary for georef that matches the parameters
    public void query(string parameters)
    {
        // TODO 
    }

    // NOTE: Build a parameter struct (name of struct = ServiceParameters)
    public void getAvailable(int offset, int limit, string model_set_type = "vis", string service = "", string query = "", string starttime = "", string endtime = "", string location = "", string state = "", string modelname = "", string timestamp_start = "", string timestamp_end = "", string model_vars = "", DownloadType type = DownloadType.Record, string OutputPath = "", string OutputName = "")
    {
        // TODO
        client.RequestRecords(onGetAvailableComplete, offset, limit, model_set_type, service, query, starttime, endtime, location, state, modelname, timestamp_start, timestamp_end, model_vars, type, OutputPath, OutputName);
    }

    private void onGetAvailableComplete(List<DataRecord> Records)
    {
        foreach(DataRecord rec in Records)
        {
            if( stored.ContainsKey(rec.name + rec.id) )
            {
                // GeoRef already exists! Do something about it!
            }
            else
            {
                // Add the record
                stored.Add(rec.name + rec.id,
                    new GeoReference(rec));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GeoRefManager
{
    // Fields
    VWClient client;
    Dictionary<String, GeoReference> stored = new Dictionary<string, GeoReference>();

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

    public void download(string recordname, string service = "vwc")
    {
        // TODO
    }

    public void getAvailable(string service = "vwc", int offset, string otherparams)
    {
        // TODO
    }
}

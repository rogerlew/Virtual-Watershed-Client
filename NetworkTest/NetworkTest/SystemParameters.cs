using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

struct SystemParameters
{
    public int offset = 0;
    public int limit = 15;
    public int width = 0;
    public int height = 0;

    public string model_set_type = "vis";
    public string service = "";
    public string query = "";
    public string starttime = "";
    public string endtime = "";
    public string location = "";
    public string state = "";
    public string modelname = "";
    public string timestamp_start = "";
    public string timestamp_end = "";
    public string model_vars = "";
    public string outputPath = "";
    public string outputName = "";
    public string crs = "";
    public string boundingbox = "";
    public string interpolation = "nearest";
    public string format = "image/png";
    public string version = "1.0.0";

    public DownloadType type = DownloadType.Record;
}

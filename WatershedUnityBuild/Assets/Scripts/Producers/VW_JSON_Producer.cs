﻿using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
class VW_JSON_Producer : DataProducer
{
    // Fields
    NetworkManager nm;
    VW_JSON_Parser parser = new VW_JSON_Parser();
    // Possible constructor
    public VW_JSON_Producer( NetworkManager refToNM ) 
    {
        nm = refToNM;
    }
    
    // Make a Parser
    void ParseJSON (List<DataRecord> Record ,string Str)
    {
        parser.Parse(Record, Str);
    }

    ///////////////////////////////////////////////////////////////////////////
    // Overrides Below
    ///////////////////////////////////////////////////////////////////////////
    protected override List<DataRecord> ImportFromURL(List<DataRecord> Records, string path, int priority = 1)
    {
        // Beautiful Lambda here
        // Downloads the bytes and uses the ByteFunction lambda described in the passed parameter which will call the mime parser and populate the record.
        //nc.DownloadBytes(path, ((DownloadBytes) => mp.Parse(Record, DownloadBytes)), priority);
        nm.AddDownload(new DownloadRequest(path, (StringFunction) ((DownloadedString) => ParseJSON(Records, DownloadedString)), priority));

        // Return
        return Records;
    }

    protected override List<DataRecord> ImportFromFile(List<DataRecord> Records, string path)
    {
        // Get the file name
        string filename = Path.GetFileNameWithoutExtension(path);
        string fileDirPath = Path.GetDirectoryName(path);
        string capabilities = fileDirPath + '\\' + filename + ".json";

        if (File.Exists(capabilities))
        {
            // Open the files
            StreamReader capReader = new StreamReader(capabilities);

            // Read the header and data
            string header = capReader.ReadToEnd();

            // Close the files
            capReader.Dispose();
        }
        else
        {
            // Throw an exception that the file does not exist
            throw new System.ArgumentException("File does not exist: " + path);
        }

        // Return
        return Records;
    }

    public override bool ExportToFile(string Path, string outputPath, string outputName)
    {
        // The getType function will determine the type of transfer (file or url) and strip off special tokens to help determine the type.
        Transfer.Type type = Transfer.GetType(ref Path);

        // Put Try Catch HERE
        // If file does not exist 
        if (type == Transfer.Type.URL)
        {
            // Beautiful Lambda here
            // Downloads the bytes and uses the ByteFunction lambda described in the passed parameter which will call the mime parser and populate the record.
            // Network Manager download
            //Logger.WriteLine("URL: " + Path);
            nm.AddDownload(new DownloadRequest(Path, (StringFunction)((DownloadedString) => parser.Parse(outputPath, outputName, DownloadedString))));
        }

        // Return
        return true;
    }
}

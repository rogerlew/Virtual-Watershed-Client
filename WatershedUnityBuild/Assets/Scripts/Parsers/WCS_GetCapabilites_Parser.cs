﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

class WCS_GetCapabilites_Parser : Parser
{
    void ParseWCSCapabilities(DataRecord Record, string Str)
    {
        var reader = System.Xml.XmlTextReader.Create(new System.IO.StringReader(Str));
        Record.WCSCapabilities = Str;


        XmlSerializer serial = new XmlSerializer(typeof(GetCapabilites.Capabilities));
        GetCapabilites.Capabilities capabilities = new GetCapabilites.Capabilities();

        if (serial.CanDeserialize(reader))
        {
            capabilities = ((GetCapabilites.Capabilities)serial.Deserialize(reader));
            Record.WCSOperations = capabilities.OperationsMetadata;
        }
    }

    public override DataRecord Parse(DataRecord record, string Contents)
    {
        // Do stuff here
        ParseWCSCapabilities(record, Contents);
        return record;
    }

    /// <summary>
    /// This version of parse parses the given input and outputs it to the file directory.
    /// </summary>
    /// <param name="Path"></param>
    /// <param name="OutputName"></param>
    /// <param name="str"></param>
    public override void Parse(string Path, string OutputName, string Str)
    {

        // Initialize variables
        var sw = new System.IO.StreamWriter(Path + OutputName + ".xml");
        sw.Write(Str);
        sw.Close();
    }
}


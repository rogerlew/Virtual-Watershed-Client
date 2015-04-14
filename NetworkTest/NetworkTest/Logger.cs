using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public static class Logger
{
    static string path;
    static StreamWriter writer;

    public static void SetPath(string dest)
    {
        if (path != null)
        {
            writer.Close();
        }

        writer = new StreamWriter(dest);
        path = dest;
    }

    public static void Log(string line)
    {
        if (writer != null)
        {
            writer.WriteLine(line);
            writer.Flush();
        }
    }

    public static void WriteLine(string line)
    {
#if UNITY_EDITOR
        Debug.Log(line);
#else
        Console.WriteLine(line);
#endif

    }
    
    public static void ReadKey()
    {
#if !(UNITY_EDITOR)
        Console.ReadKey();
#endif
    }

    public static void Close()
    {
        path = null;
        writer.Close();
        writer = null;
    }
}

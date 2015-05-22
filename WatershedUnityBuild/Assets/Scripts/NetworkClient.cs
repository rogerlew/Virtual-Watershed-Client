﻿using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.Collections.Generic;

//http://stackoverflow.com/questions/1488099/call-an-eventhandler-with-arguments
/**Call Back Functions
 * The following functions are used to populate DataRecords with Downloaded data.
 * The ByteFunction and StringFunction delegates are used to pass downloaded data into the DataRecords.
 * **/
public delegate void StringFunction(string str);
public delegate void ByteFunction(byte[] bytes);

/** 
 * @authors Chase Carthen, Thomas Rushton, Shubham Gogna
 */
public class NetworkClient : WebClient
{
    public delegate void DownloadBytesCallback(Object sender, DownloadDataCompletedEventArgs e);
    NetworkManager netmanager;
    PriorityQueue<DownloadRequest> DownloadRequests = new PriorityQueue<DownloadRequest>();
    Dictionary<string, DownloadDataCompletedEventHandler> DataCompletedEventHandlers = new Dictionary<string, DownloadDataCompletedEventHandler>();
    private readonly object _Padlock = new object();
    int count = 0;
    public int size { get {  return DownloadRequests.Count(); } }

    public NetworkClient(NetworkManager manager) : base()
    {
        netmanager = manager;
        DownloadProgressChanged += DownloadProgressCallback;
    }

    public void Halt()
    {
        DownloadRequests.Clear();
    }

    //WebClient wc = new WebClient();
    //http://stackoverflow.com/questions/2042258/webclient-downloadfileasync-download-files-one-at-a-time
    //http://stackoverflow.com/questions/1488099/call-an-eventhandler-with-arguments

    /// <summary>
    /// DownloadBytes will essentially download bytes based on the given url.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="byteFunc"></param>
    /// <param name="priority"></param>
    public void Download( DownloadRequest req )
    {
        // Enqueue the current request and call the event
        lock (_Padlock)
        {
            DownloadRequests.Enqueue(req);
        }
        netmanager.CallDownloadQueued(req.Url);

        // Check if not busy
        if (!IsBusy)
        {
            // This call will actually run a download in another thread, 
            // but with a catch it will come back to thread of this webclient.
            // Downloads bytes asynchronously.
            StartNextDownload();
        }
    }

    /// <summary>
    /// OnDownloadDataCompleted is the default WebClient function that is called after data has been downloaded.
    /// </summary>
    /// <param name="args"></param>
    protected override void OnDownloadDataCompleted(DownloadDataCompletedEventArgs args)
    {
        count++;
        // Call the base function
        base.OnDownloadDataCompleted(args);

        // Dequeue current download request and use its callback
        DownloadRequest Req;
        lock (_Padlock)
        {
            Req = DownloadRequests.Dequeue();
        }
        try
        {
            Req.Callback(args.Result);
        }
        catch(Exception e)
        {
            Logger.WriteLine(e.Message + " " + e.StackTrace);
            Logger.WriteLine("Insert Custom Error Message / Error code for handling HTTP 404");
            netmanager.CallDataError(Req.Url);
            StartNextDownload();
            return;
        }
        Logger.WriteLine("Completed byte download, passed to callback function.");
        //DataTracker.updateJob(Req.Url, DataTracker.Status.FINISHED);

        // Need some way of notifying that this download is finished --- errors,success
        netmanager.CallDownloadComplete(Req.Url);

        GlobalConfig.loading = false;

        // Start the next one
        StartNextDownload();
    }

    /// <summary>
    /// OnDownloadStringCompleted is the default WebClient function that is called after a string has been downloaded.
    /// </summary>
    /// <param name="args"></param>
    protected override void OnDownloadStringCompleted(DownloadStringCompletedEventArgs args)
    {
        count++;
        // Call the base function
        base.OnDownloadStringCompleted(args);

        // Dequeue current download request and use its callback
        DownloadRequest Req;
        lock (_Padlock)
        {
            Req = DownloadRequests.Dequeue();
        }
        try
        {
            Req.Callback(args.Result);
        }
        catch(Exception e)
        {
            Logger.WriteLine(e.Message + " " + e.StackTrace);
            Logger.WriteLine("Insert Custom Error Message / Error code for handling HTTP 404");
            netmanager.CallDataError(Req.Url);
            StartNextDownload();
            return;
        }
        Logger.WriteLine("Completed string download, passed to callback function. " + Req.Url);

        // Need some way of notifying that this download is finished --- errors,success
        netmanager.CallDownloadComplete(Req.Url);

        GlobalConfig.loading = false;

        // Start the next one
        StartNextDownload();
    }

    // Finaly something that tells us the current progress of a downlaod
    private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
    {
        // Displays the operation identifier, and the transfer progress.
         //Logger.WriteLine(
         //   (string)e.UserState + " " + 
         //   e.BytesReceived + " " + 
         //   e.TotalBytesToReceive + " " +
         //   e.ProgressPercentage);
        GlobalConfig.loading = true;
    }
    
    private void StartNextDownload()
    {
        // If there are any downloads remaining do them!
        lock (_Padlock)
        {
            if (DownloadRequests.Count() > 0 && !IsBusy)
            {
                // Start the next one
                DownloadRequest req = DownloadRequests.Peek();
                Logger.WriteLine("Started: " + req.Url);
                netmanager.CallDownloadStart(req.Url);
                // Check if its a byte
                if (req.isByte)
                {
                    DownloadDataAsync(new System.Uri(req.Url));
                }
                else
                {
                    DownloadStringAsync(new System.Uri(req.Url));
                }
            }
            else
            { 
                Logger.WriteLine("SCARY");
            }
        }
    }

    ~NetworkClient()
    {
        this.CancelAsync();
    }

}
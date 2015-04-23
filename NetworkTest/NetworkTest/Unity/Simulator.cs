using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// The Simulator class will host single or multiple model runs.
/// The simulator class will "run" models based on time steps provided by the user.
/// </summary>
public class Simulator
{
    public float TimeStep;
    public DateTime start;
    public DateTime end;
    // Default one hour
    public TimeSpan TimeDelta;
    List<ModelRun> ModelRuns = new List<ModelRun>();

    // Storing byte data and float data
    List<byte> Texture = new List<byte>();
    List<float[,]> RawData = new List<float[,]>();

    // This is used for determining what in the model run we are focusing on.
    public string ModelVar = "";

    // How many datasets do we have?
    int NumberOfDatasets = 0;

    // Number of datasets that are loaded into
    int Loaded = 0;

    // Threshold for how many we should have in ram
    int MaxLoaded = 1000;

    // List of Data Records to step through.
    List<DataRecord> SetToStepThrough;

    public Simulator()
    {
        start = new DateTime(1997, 1, 1);
        end = new DateTime(1998, 1, 1);
        TimeDelta = new TimeSpan(1, 0, 0);
    }


    // A test simulation function for debugging purposes.
    public void Simulation(float dt)
    {
        
        TimeStep = dt;
        DateTime current = start;
        while(current  < end )
        {
            // Step through current data...

            Console.WriteLine(current);
            current = current.AddTicks((long)(dt * TimeDelta.Ticks));
        }
    }

    // A step function for custom timesteps
    public void Step(float timestep)
    {

    }


    // Use the default timestep contained within this class
    public void Update()
    {

    }

    // A setter function for setting the model runs.
    public void SetModelRuns(List<ModelRun> Models)
    {
        ModelRuns.Clear();
        ModelRuns.AddRange(ModelRuns);
    }
    
    // File Cache Entry
    string CurrentModel = "model";

    /// <summary>
    /// This FetchData function will get data from the model run classes.
    /// </summary>
    private void FetchData(float range=0.0f)
    {
        if(range > 1 )
        {
            range = 1;
        }
        else if(range < 0)
        {
            range = 0;
        }
        // TODO
        // Fetch data

        // check cache for object
    }

    private void FetchData(DateTime date)
    {
        // Fetch the dataset that closely matches this date

    }

    private void FetchAll()
    {
        // Fetch all data related to all model runs -- 
        foreach(var i in ModelRuns)
        {
            i.FetchAll(ModelVar);
        }
    }

}


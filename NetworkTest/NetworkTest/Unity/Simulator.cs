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
    
    /// <summary>
    /// This FetchData function will get data from the model run classes.
    /// </summary>
    private void FetchData()
    {
        // TODO
    }

}


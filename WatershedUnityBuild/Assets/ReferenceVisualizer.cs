﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VTL.ListView;

public class ReferenceVisualizer : MonoBehaviour 
{
    public DownloadManager downloadManager;
    public ListViewManager listView;
    public ListViewManager listViewables;
    public Dictionary<string, GameObject> viewableObjects = new Dictionary<string,GameObject>();
    Queue<DataRecord> queuedRecs = new Queue<DataRecord>();
    Utilities utils = new Utilities();


    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (queuedRecs.Count > 0)
        {
			DataRecord rec = queuedRecs.Dequeue();
			if(rec.Type.ToLower().Contains("shapfile") || rec.Type.ToLower().Contains("shapefile"))
			{
            	BuildShapes(new List<DataRecord> {rec });
			}
			else if(rec.Type.ToLower().Contains("dem") )
			{
				//BuildShapes(new List<DataRecord> { rec });
			}
			else if(rec.Type.ToLower().Contains("doqq"))
			{
				//BuildShapes(new List<DataRecord> { rec });
				BuildDOQQ(rec);
			}
			// add build projector
        }
    }

    void OnEnable()
    {
        //SetFields();
    }

    public void SetFields()
    {
        listView.Clear();

        Debug.LogError("SET FIELDS 2");
        foreach (var i in downloadManager.GetModelRuns())
        {
            var MR = ModelRunManager.GetByUUID(i);
            Debug.LogError("ADDING: " + MR.GetVariables().Count);
            List<string> variables = MR.GetVariables();
            Debug.LogError(MR.ModelName.ToLower());
            Debug.LogError(MR.Total);
            // model_set_type  must be reference
            if(!MR.ModelName.ToLower().Contains("reference"))
            {
                Debug.LogError("Give me a break of that kitkat bar.");
                continue;
            }

            foreach (var variable in variables)
            {
                foreach(var record in MR.Get(variable))
                {
                    //Debug.LogError(record.name);
                    listView.AddRow(new object[] { record.name, record.Type }, record);
                }
            }

            //downloadManager.AddModelRun(i);
        }
    }

    public void SetViewableFields(GameObject obj, DataRecord objectRec)
    {

        Debug.LogError("SET FIELDS 3");
        listViewables.AddRow(new object[]{obj.name},objectRec);
        
    }

    public void DownlaodObjects()
    {
        List<DataRecord> recs = new List<DataRecord>();
        recs = listView.GetSelected();
		 
        foreach (var i in recs)
        {
            if (!viewableObjects.ContainsKey(i.name))
            {
                if (i.Type.ToLower().Contains("shapfile") || i.Type.ToLower().Contains("shapefile"))
                {
					SystemParameters param = new SystemParameters();
					param.Priority = 100;
                    Debug.LogError("DOWNLOADING OBJECTS");
                    ModelRunManager.Download(new List<DataRecord> { i }, buildQueue, operation: "wfs",param: param);

                }
				// DEM, DOQQs
				else if(i.Type.ToLower().Contains("dem") )
				{
					SystemParameters param = new SystemParameters();
					param.Priority = 100;
					param.width = 100;
					param.height = 100;
					Debug.LogError("DOWNLOADING OBJECTS");
					ModelRunManager.Download(new List<DataRecord> { i }, buildQueue, operation: "wcs",param: param);
				}
				else if(i.Type.ToLower().Contains("doqq"))
				{
					SystemParameters param = new SystemParameters();
					param.Priority = 100;

					// Param width, height hard codeness
					param.width = 1024;
					param.height = 1024;
					Debug.LogError("DOWNLOADING OBJECTS");
					ModelRunManager.Download(new List<DataRecord> { i }, buildQueue, operation: "wms",param: param);
				}
            }
        }
    }

    public void BuildShapes(List<DataRecord> records)
    {
        Debug.LogError("BUILDING OBJECTS");
        GameObject go = utils.buildShape(records[0]);
        utils.rebuildShape(go);
        go.name = records[0].name;
        viewableObjects.Add(go.name, go);
        SetViewableFields(go, records[0]);
    }

	public void BuildDOQQ(DataRecord record)
	{
		Debug.LogError("BUILDING OBJECTS");
		GameObject go = utils.buildProjector(record);
		utils.rebuildShape(go);
		go.name = record.name;
		viewableObjects.Add(go.name, go);
		SetViewableFields(go, record);
	}

    void buildQueue(List<DataRecord> recs)
    {
        queuedRecs.Enqueue(recs[0]);
    }

    public void enableObjects()
    {
        List<DataRecord> selected = listViewables.GetSelected();

        foreach (var i in selected)
        {
            Debug.LogError(i.name);
            if (viewableObjects.ContainsKey(i.name))
            {
                viewableObjects[i.name].SetActive(true);
            }
        }
    }

    public void disableObjects()
    {
        List<DataRecord> selected = listViewables.GetSelected();

        foreach (var i in selected)
        {
            if (viewableObjects.ContainsKey(i.name))
            {
                viewableObjects[i.name].SetActive(false);
            }
        }
    }

    //public void BuildImagery(List<DataRecord> records)
    //{
    //    //need to build a utilities function for the projector game object
    //    GameObject go = utils.buildTextures(records[0].Data, Color.cyan, Color.white);
    //    go.name = records[0].name;
    //    viewableObjects.Add(go);
    //}
}

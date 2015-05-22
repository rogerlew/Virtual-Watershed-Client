﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VTL.ListView;

public class ModelRunVisualizer : MonoBehaviour {
    
    public DownloadManager downloadManager;
    public ListViewManager listView;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnEnable()
    {
        //SetFields();
    }

    public void SetFields()
    {
        listView.Clear();

        Debug.LogError("SET FIELDS");
        foreach (var i in downloadManager.GetModelRuns())
        {
            var MR = ModelRunManager.GetByUUID(i);
            Debug.LogError("ADDING: " + MR.GetVariables().Count);
            List<string> variables = MR.GetVariables();
            foreach (var variable in variables)
            {
                listView.AddRow(new object[] { MR.ModelName, variable},MR);
            }
            
            //downloadManager.AddModelRun(i);
        }
    }
}

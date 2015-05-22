﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using VTL;
using VTL.ListView;
using VTL.ProceduralTerrain;
public class TerrainPopulater : MonoBehaviour 
{
	public ListViewManager TerrainList;
	List<object[]> Lists = new List<object[]>();
	List<DataRecord> Records = new List<DataRecord>();
	void GetTerrainList(List<DataRecord> Terrains)
	{
		Debug.LogError ("HELLO THERE" + Terrains.Count);
		Records = Terrains;
		foreach(var rec in Terrains)
		{
			Debug.LogError(rec.name);
			Lists.Add(new object[]{rec.name,
				rec.location});
		}
	}
	public ToggleObjects to;
	VWClient vwc;
	NetworkManager nm;
	List<DataRecord> recordToBuild = new List<DataRecord>();
	// Use this for initialization
	void Start () {
		if (ModelRunManager.client == null) {
			nm = new NetworkManager ();
			vwc = new VWClient (new DataFactory (nm), nm);
			nm.Subscribe (vwc);
			ModelRunManager.client = vwc;
		} 
		else 
		{
			vwc = ModelRunManager.client;
		}
		// Start downloading terrains
		SystemParameters sp = new SystemParameters ();
		sp.query="DEM";
		sp.limit = 100;
		sp.offset = 0;
		vwc.RequestRecords (GetTerrainList, sp);
	}
	
	// Update is called once per frame
	void Update () {
	if (Lists.Count > 0) {
			for(int i =0; i < Lists.Count; i++)
			{
				TerrainList.AddRow(Lists[i],Records[i]);
					}
			Records.Clear();
			Lists.Clear();
		}
		if (recordToBuild.Count > 0) 
		{
			Debug.LogError("REJOICENESS");
			LoadTerrain(recordToBuild[0]);
			recordToBuild.Clear();
		}
	}

	public void LoadTerrain(DataRecord record)
	{
		Debug.LogError ("Load Terrain");


		record.boundingBox = new SerialRect (Utilities.bboxSplit(record.bbox));
		transform tran = new global::transform ();

		// EPSG code
		Debug.LogError (record.projection);
		string EPSG = record.projection.Replace ("epsg:", "");
		Debug.LogError (EPSG);
		GlobalConfig.GlobalProjection = int.Parse (EPSG);

		tran.createCoordSystem(record.projection);
		Debug.LogError ("UTM COORD");

		// Calculate UTM boundingbox
		try
		{
		Vector2 point = tran.transformPoint(new Vector2(record.boundingBox.x, record.boundingBox.y));
		Vector2 upperLeft = tran.transformPoint(new Vector2(record.boundingBox.x,record.boundingBox.y));
		Vector2 upperRight = tran.transformPoint(new Vector2(record.boundingBox.x+record.boundingBox.width,record.boundingBox.y));
		Vector2 lowerRight = tran.transformPoint(new Vector2(record.boundingBox.x+record.boundingBox.width,record.boundingBox.y-record.boundingBox.height));;
		Vector2 lowerLeft = tran.transformPoint(new Vector2(record.boundingBox.x,record.boundingBox.y-record.boundingBox.height));

		// Calculate average xres
		GlobalConfig.BoundingBox = new Rect (lowerLeft.x, upperLeft.y, Mathf.Abs (upperLeft.x - upperRight.x), Mathf.Abs (upperLeft.y - lowerLeft.y));

		float XRes = GlobalConfig.BoundingBox.width / record.Data.GetLength(0);
		float YRes = GlobalConfig.BoundingBox.height / record.Data.GetLength(1);
		StartupConfiguration.LoadConfig ();
		Debug.LogError ("REJOICE");
		var GO = ProceduralTerrain.BuildTerrain (record.Data, XRes, YRes, null);
		GO.transform.position = new Vector3 (-GlobalConfig.BoundingBox.width / 2, 0, -GlobalConfig.BoundingBox.height / 2);
		//GameObject.Instantiate (GO);
		Debug.LogError ("DONE DOWNLOADING");
			to.toggleObjects();
		}
		catch (Exception e)
		{
			Debug.LogError(e.StackTrace);
		}
		//Record[0]
	}

	public void loadTerrain(List<DataRecord> Record)
	{
		Debug.LogError ("SETTING THE TERRAIN");
		recordToBuild.Add(Record [0]);
	}

	public void LoadSelected()
	{
		var Runs = TerrainList.GetSelected ();
		Debug.LogError ("LOAD SELECTED: " + Runs.Count);
		if(Runs.Count > 0)
		{
			SystemParameters sp = new SystemParameters();
			sp.width = 1025;
			sp.height=1025;
			ModelRunManager.Download(new List<DataRecord>{Runs[0]},loadTerrain,param:sp);
		}
		// Load First Terrain
	}
}

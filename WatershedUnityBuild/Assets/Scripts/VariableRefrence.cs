using UnityEngine;
using System.Collections;

public class VariableRefrence : MonoBehaviour {

    // The path to the file with the variables names and description
#if UNITY_EDITOR
    const string MAPFILENAME = "VariableRefrenceFile.txt";
    public static string DirectoryLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"/../../VariableMetaData";
#else
    const string MAPFILENAME = "VariableRefrenceFile.txt";
    public static string DirectoryLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#endif



	// Use this for initialization
	void Start () {
	    


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

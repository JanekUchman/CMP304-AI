using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Boo.Lang.Environments;
using UnityEditor;
using UnityEngine;

public static class WriteToFile  {

	private static List<string[]> rowData = new List<string[]>();
	private static string path = "";
	
	public static void WriteString(string text)
	{
		string path = "Assets/Resources/output.txt";

		//Write some text to the test.txt file
		try
		{
			StreamWriter writer = new StreamWriter(path, true);
			writer.WriteLine(text + "\n");
			writer.Close();
		}
		catch (Exception e)
		{
			Debug.LogError("Warning file open! Please close to record results.");
		}

		//Re-import the file to update the reference in the editor
		//AssetDatabase.ImportAsset(path); 
	}

	public static void CreateCSV()
	{
		// Creating First row of titles manually..
		string[] rowDataTemp = new string[3];
		rowDataTemp[0] = "Time Taken";
		rowDataTemp[1] = "Generations";
		rowDataTemp[2] = "Settings used";
		rowData.Add(rowDataTemp);
#if UNITY_EDITOR
		path = "Assets/Resources/Test - " + DateTime.Now.ToString("yyyy-dd-M   HH-mm-ss") + ".csv";
#else
		path = Application.dataPath + "/Test - "+ DateTime.Now.ToString("yyyy-dd-M   HH-mm-ss") + ".csv";
#endif

		var fs = File.Create(path);
		fs.Close();
	}
	
	public static void WriteToCSV(int timeTaken, int generations, int settingsUsed)
	{
		if (path == "") CreateCSV();
		
		string[] rowDataTemp = new string[3];

		
		rowDataTemp = new string[3];
		rowDataTemp[0] = timeTaken.ToString();
		rowDataTemp[1] = generations.ToString(); // ID
		rowDataTemp[2] = settingsUsed.ToString(); // Income
		rowData.Add(rowDataTemp);
		

		string[][] output = new string[rowData.Count][];

		for(int i = 0; i < output.Length; i++){
			output[i] = rowData[i];
		}

		int length         = output.GetLength(0);
		string delimiter     = ",";

		StringBuilder sb = new StringBuilder();
        
		for (int index = 0; index < length; index++)
			sb.AppendLine(string.Join(delimiter, output[index]));
        
        
		try
		{
			StreamWriter outStream = System.IO.File.CreateText(path);
			outStream.WriteLine(sb);
			outStream.Close();
		}
		catch (Exception e)
		{
			Debug.LogError("Warning file open! Please close to record results." + e.Message);
		}
		
	}

	public static void ClearCSVFile()
	{
		try
		{
			string path = "Assets/Resources/output.csv";
			File.WriteAllText( path, "");
		}
		catch (Exception e)
		{
			Debug.LogError("Warning file open! Please close to record results.");
		}
		
	}
	
	public static void ClearTextFile()
	{
		try
		{
			string path = "Assets/Resources/output.txt";
			File.WriteAllText( path, "");
		}
		catch (Exception e)
		{
			Debug.LogError("Warning file open! Please close to record results.");
		}
	}

	public static void ReadString()
	{
		string path = "Assets/Resources/output.txt";

		//Read the text from directly from the test.txt file
		StreamReader reader = new StreamReader(path); 
		Debug.Log(reader.ReadToEnd());
		reader.Close();

}
}

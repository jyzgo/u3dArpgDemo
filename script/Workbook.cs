using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;

public class Row
{
	public List<string> _cells= new List<string>();
	public List<string> _key = new List<string>();
	
	public string this[string key]
	{
		get{
			string val = "";
			int pos = _key.IndexOf(key);
			if(pos < 0 || pos >= _cells.Count)
			{
				Debug.LogError("can't find the value of the key:["+key+"]");
			}else{
				val = _cells[pos];	
			}
			return val;
		}
	}
} 


public class Sheet
{
	public List<Row>  _rows = new List<Row>();

	public void logoutData()
	{
		string info = "";
		for(int i = 0; i< _rows.Count; i++)
		{
			info += "Row["+i+"]:"+_rows[i]._cells.Count + "    ";
			for(int j= 0; j < _rows[i]._cells.Count; j++)
			{
				info += _rows[i]._cells[j] + "   ";
			}
			info +="\n";
		}
		Debug.Log(info);	
	}
	
	public void SetKeyInfo()
	{
		List<string> keys = _rows[0]._cells;
		for(int i = 1; i< _rows.Count; i++)
		{
			if(_rows[i]._cells.Count != keys.Count)
			{
				
				string	info ="Row[" +(i+1) +"]:  "; 
				for(int j= 0; j < _rows[i]._cells.Count; j++)
				{
					info += "["+_rows[i]._cells[j] + "]   ";
				}
				Debug.LogError(info);
				Debug.LogError("Row[" +(i+1) +"] count: " + _rows[i]._cells.Count + " != key count: " + keys.Count);
				return;
			}
			_rows[i]._key.Clear();
			_rows[i]._key.AddRange(keys);
		}
	}
}


public class Workbook {
		
	public Sheet _sheet = null;
	
	public static Workbook CreatWorkbook(string fileName , string sheetName)
	{
		Workbook workbook = new Workbook();
		workbook.Read(fileName, sheetName);
		return workbook;
	}
		
	public void Read(string fileName , string sheetName)
	{
		XmlReader reader = null;
		try{
			 reader = XmlReader.Create(fileName);
		}
		catch(Exception e)
		{
			Debug.LogError(e.Message  + "  read excel error :["+fileName+"]");
			return;
		}
			
		Row row = null;
		
		bool sheetStart = false;
		bool dataStart = false;
		
		string  cellValue = "";

		while(reader.Read())
		{
			if(reader.Name == "Worksheet")
			{
				log("NoteType:"+ reader.NodeType);
				if(sheetStart && reader.NodeType == XmlNodeType.EndElement)
				{
					log("</Worksheet>");
					sheetStart = false;
					//_sheet.logoutData(); //do not display info now
					_sheet.SetKeyInfo();
					return;
				}else
				{
					string sName = reader.GetAttribute("ss:Name");
					if(sName == sheetName)
					{
						sheetStart = true;
						_sheet = new Sheet();
						log("<Worksheet["+ sheetName+"]>");
					}
				}
			}
			else if(sheetStart && reader.Name == "Row")
			{
				log("NoteType:"+ reader.NodeType);
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					log("</Row>");
				}else{
					row = new Row();
					_sheet._rows.Add(row);
					log("<Row>");
				}
			}else if(sheetStart && (reader.Name == "Data" || reader.Name == "ss:Data"))
			{
				log("NoteType:"+ reader.NodeType);
				if(reader.NodeType == XmlNodeType.EndElement)
				{
					row._cells.Add(cellValue);
					dataStart = false;
					log("</Data>");
				}else{
					cellValue = "";
					dataStart = true;
					log("<Data>");
				}
			}
			else if(sheetStart)
			{
				if(dataStart)
				{ 
					log("NoteType:"+ reader.NodeType + " | " + reader.Name + "  | " + reader.HasValue);
					if(reader.HasValue)
					{
						string val = reader.Value;
						cellValue += val;
						log("["+val+"]");
					}
				}
			}
		}
	}
	
	public void log(string message)
	{
		//Debug.Log(message);
	}
	
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EnumDialogPos
{
	top,
	bottom
}

[Serializable]
public class DialogData
{
	public string id;
	public EnumDialogPos pos;
	public string speakerIDS;
	public string contentIDS;
}

public class DialogDataList : ScriptableObject
{
	public List<DialogData> dataList = new List<DialogData>();

	public DialogData FindDialog(string id)
	{
		return dataList.Find(delegate(DialogData d) { return d.id == id; });
	}
}

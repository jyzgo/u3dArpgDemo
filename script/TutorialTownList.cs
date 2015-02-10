using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[System.Serializable]
public class TutorialTown {
		
	public EnumTutorial id;
	public EnumTutorial preId = EnumTutorial.None;
	public string level;
	
	public string tipIds;	
	public bool isOpen;
	
	public bool only_once;
}


public class TutorialTownList : ScriptableObject
{
	public List<TutorialTown> _dataList = new List<TutorialTown>();
}
using UnityEngine;
using System.Collections;


public class SkillCameraList : ScriptableObject {

	public SkillCamera []_skillCameraList = null;
	
	void OnEnable()
	{
	}
	
	public int AddSkillCamera()
	{
		SkillCamera []_newlist;
		if(_skillCameraList == null)
		{
			_newlist = new SkillCamera[1];
		}
		else
		{
			_newlist = new SkillCamera[_skillCameraList.Length + 1];
		}
		for(int i = 0;_skillCameraList != null && i < _skillCameraList.Length;++i)
		{
			_newlist[i] = _skillCameraList[i];
		}
		
		_newlist[_newlist.Length - 1] = new SkillCamera();
		_newlist[_newlist.Length - 1].cameraName = "NewSkillCamera";
		
		_skillCameraList = _newlist;
		
		return _newlist.Length - 1;
	}
	
	public int GetIndex(string name)
	{
		if(_skillCameraList != null)
		{
			for(int i = 0;i < _skillCameraList.Length;++i)
			{
				if(_skillCameraList[i].cameraName == name)
				{
					return i;
				}
			}
		}
		return -1;
	}
	
	public SkillCamera GetSkillCamera(int index)
	{
		if(_skillCameraList != null && index >= 0 && index < _skillCameraList.Length)
		{
			return _skillCameraList[index];
		}
		return null;
	}
	
	public SkillCamera GetSkillCamera(string name)
	{
		return GetSkillCamera(GetIndex(name));
	}
}

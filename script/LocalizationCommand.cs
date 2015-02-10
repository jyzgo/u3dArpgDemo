using UnityEngine;
using System.Collections;


/// <summary>
/// Localization command.
/// </summary>
public class LocalizationCommand
{
	public LocalizationCommand(string name)
	{
		_name = name;
	}
	
	string _name;	
	string _params;
	
	public void SetRequiredParams(params object[] list)
	{		
		foreach(object o in list)
		{
			_params += " ";
			_params += o.ToString();
		}
		
		_params.TrimStart(' ');
	}
	
	public void AddOptionalParams(string flag, object v)
	{
		_params += " ";
		_params += flag;
		_params += v.ToString();
	}
	
	public bool Execute()
	{
		try
		{
			LocalizationCmdProcessor.GetInstance().ExecuteCmd(_name, _params);
		}
		catch (System.Exception e)
		{
			Debug.LogError(e.ToString());
		}
		
		return true;
	}
}

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

[System.Serializable]
public class DeviceLOD
{
	public string _deviceModel;
	public string []_deviceNames;
	public int _level;
	public int _fps = 30;
	public int _npcCount = 10;
};

public class DeviceLODSetting : ScriptableObject {
	
	public DeviceLOD []LODSettings;
	
	public int _standaloneDefaultLevel;
	public int _standaloneDefualtFPS = 30;
	public int _standaloneDefualtNPCCount = 5;
	public int _iphoneDefaultLevel;
	public int _iphoneDefaultFPS = 30;
	public int _iphoneDefualtNPCCount = 10;
	public int _androidDefaultLevel;
	public int _androidDefaultFPS = 30;
	public int _androidDefualtNPCCount = 5;
	
	private DeviceLOD _info = null;
	
#if UNITY_EDITOR 
	public static string GetMachineName() {return "standalone";}
	public int GetDefaultLevel() {
		return _standaloneDefaultLevel;
	}
	public int GetDefaultFPS() {
		return _standaloneDefualtFPS;
	}
	
	public int GetDefaultNPCCount() {
		return _standaloneDefualtNPCCount;
	}
	
#else
#if UNITY_IPHONE
	
	[DllImport ("__Internal")]
	public static extern string GetMachineName();
	public int GetDefaultLevel() {
		return _iphoneDefaultLevel;
	}
	public int GetDefaultFPS() {
		return _iphoneDefaultFPS;
	}
	public int GetDefaultNPCCount() {
		return _iphoneDefualtNPCCount;
	}	
	
#else
#if UNITY_ANDROID
		
	public static string GetMachineName()
	{
		return "";
	}
	public int GetDefaultLevel() {
		return _androidDefaultLevel;
	}
	public int GetDefaultFPS() {
		return _androidDefaultFPS;
	}
	public int GetDefaultNPCCount() {
		return _androidDefualtNPCCount;
	}	
#else
	public static string GetMachineName(){return "";}
	public int GetDefaultLevel() {
		Debug.LogError("Unknown platform");
		return 0;
	}
	public int GetDefaultFPS() {
		return -1;
	}
	public int GetDefaultNPCCount() {
		return 0;
	}	
#endif // UNITY_ANDROID
#endif // UNITY_IPHONE
#endif // UNITY_EDITOR
	
	
	
	public int GetDeviceLevel() {
		DeviceLOD settings = GetLODSetting();
		return (settings == null) ? GetDefaultLevel() : settings._level;
	}
	
	public int GetTargetFPS() {
		DeviceLOD settings = GetLODSetting();
		return (settings == null) ? GetDefaultFPS() : settings._fps;
	}
	
	public string GetDeviceModel() {
		DeviceLOD settings = GetLODSetting();
		return (settings == null) ? "Unknown Device \"" + GetMachineName() + "\"" : settings._deviceModel;
	}
	
	public int GetNPCCount() {
		DeviceLOD settings = GetLODSetting();
		return (settings == null) ? GetDefaultNPCCount() : settings._npcCount;
	}	
	
	DeviceLOD GetLODSetting() {
		if(_info == null) {
			string machineName = GetMachineName();
			_info = GetLODSettingByName(machineName);
		}
		return _info;
	}
	
	DeviceLOD GetLODSettingByName(string machineName) {
		foreach(DeviceLOD dl in LODSettings)
		{
			foreach(string n in dl._deviceNames) {
				if(machineName.Contains(n)) // find configuration match the device name.
				{
					return dl;
				}
			}
		}
		return null;
	}
	
}

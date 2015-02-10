using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class LocalizationInfo
{
	#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern string APP_GetLanguage();
	
	[DllImport ("__Internal")]
	private static extern string APP_GetCountry();
	#else
	
	private static string APP_GetLanguage()
	{
		return "unkown_Language";
	}
	
	private static string APP_GetCountry()
	{
		return "unkown_Country";
	}
	#endif	
	
	public static string GetLanguage()
	{
		return APP_GetLanguage();
	}
	
	public static string GetCountry()
	{
		return APP_GetCountry();
	}
}

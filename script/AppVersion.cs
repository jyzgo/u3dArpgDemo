using UnityEngine;
using System;
using System.Runtime.InteropServices;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


public class AppVersion : ScriptableObject 
{
	public static string GetCurrentVersionString()
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		return Marshal.PtrToStringAnsi(AppVersionNativeGetCurrentVersionString());
#elif UNITY_ANDROID && !UNITY_EDITOR
		return "";
#else
		return "CurrentVersionString";
#endif
	}
	
	public static string GetCurrentBuildString()
	{
#if UNITY_IPHONE && !UNITY_EDITOR
		return Marshal.PtrToStringAnsi(AppVersionNativeGetCurrentBuildString());
#elif UNITY_ANDROID && !UNITY_EDITOR
		return "";
#else
		return "CurrentBuildString";
#endif
	}
	
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal",CharSet=CharSet.Ansi)]
	private static extern IntPtr AppVersionNativeGetCurrentVersionString();
	
	[DllImport ("__Internal",CharSet=CharSet.Ansi)]
	private static extern IntPtr AppVersionNativeGetCurrentBuildString();
	
#elif UNITY_ANDROID && !UNITY_EDITOR
	
#else
	
#endif
}

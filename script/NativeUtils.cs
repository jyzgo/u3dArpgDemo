using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;

public static class NativeUtils
{
	public enum NetworkType
	{
		NetNone,
		Net3G,
		NetWiFi,
	};
	
	public static bool IsNetworkAvailable()
	{
		int netStatus = NetworkStatus();
		Debug.Log("NetworkStatus = " + netStatus);
		return (netStatus != 0);
	}
	
	public static NetworkType GetNetworkStatus()
	{
		NetworkType netStatus = (NetworkType)NetworkStatus();
		return netStatus;
	}
	
	
	#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern int NetworkStatus();
	#else
	private static int NetworkStatus()
	{
		return 0;
	}
	#endif	
	
    public static void ReportMemory(string eventName)
	{
		#if UNITY_IPHONE 
		if (Debug.isDebugBuild)
		{
			Debug.Log(eventName + " " + MT_GetCurrentMemoryBytes());
		}
		#endif
	}
	
	public static int GetCurrentMemoryBytes()
	{
		return MT_GetCurrentMemoryBytes();
	}
	
	#if UNITY_EDITOR
	private static int MT_GetCurrentMemoryBytes()
	{
		return (int)Profiler.usedHeapSize;
	}
	#elif UNITY_ANDROID
	#if false
	static bool inited = false ;
	static IntPtr debug ;
	static IntPtr getPss ;
	/* api level >= 14 (android 4+), ~50 milliseconds */
	private static int MT_GetCurrentMemoryBytes ()
	{
		if ( !inited )
		{
			debug = AndroidJNI.FindClass ( "android/os/Debug" ) ;
			getPss = AndroidJNIHelper.GetMethodID ( debug, "getPss", "()J", true ) ;
			inited = true ;
		}
		return 1024 * AndroidJNI.CallStaticIntMethod ( debug, getPss, new jvalue[0] ) ;
	}
	#else
	static bool inited = false ;
	static float lastUpdate ;
	static int pss ;
	static AndroidJavaObject activityManager ;
	static int[] pids ;
	/* ~130 milliseconds */
	private static int MT_GetCurrentMemoryBytes ()
	{
		const float updateEvery = 10 ;
		float time = Time.realtimeSinceStartup ;
		if ( !inited )
		{
			inited = true ;
			/* android.content.Context context = com.unity3d.player.UnityPlayer.currentActivity ; */
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer") ;
    		AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity") ;
    		/* android.app.ActivityManager activityManager = (ActivityManager) context.getSystemService(ACTIVITY_SERVICE) ; */
			activityManager = context.Call<AndroidJavaObject>("getSystemService","activity") ;
			/* int pids[] = new int[1] ; pids[0] = android.os.Process.myPid() ; */
			pids = new int[]{ new AndroidJavaClass("android.os.Process").CallStatic<int>("myPid") } ;
		}
		else if ( time < lastUpdate + updateEvery )
		{	return pss ; }
		lastUpdate = time ;
		/* android.os.Debug.MemoryInfo[] memoryInfoArray = activityManager.getProcessMemoryInfo(pids) ; */
		AndroidJavaObject[] memoryInfoArray = activityManager.Call<AndroidJavaObject[]>("getProcessMemoryInfo",pids) ;
		pss = 1024*memoryInfoArray[0].Call<int>("getTotalPss") ;
		return pss ;
	}
	#endif
	#elif UNITY_IPHONE
    [DllImport (m_ExternalFileName,CharSet=CharSet.Ansi)]
    private static extern int MT_GetCurrentMemoryBytes();
    private const string m_ExternalFileName = "__Internal";
	#else
	private static int MT_GetCurrentMemoryBytes()
	{
		return -1;
	}
	#endif
}

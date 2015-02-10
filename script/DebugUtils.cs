using UnityEngine;

using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FaustComm;

public class Assertion
{
    public static void Check(bool condi, string msg1 = "", object msg2 = null)
    {
        if (!condi)
        {
			UnityEngine.Debug.LogError(string.Format(msg1, msg2));
#if ASSERTION_ENABLED && UNITY_EDITOR
			UnityEngine.Debug.Break();
#endif
		}
    }

    public static void Fail()
    {
        Debug.LogError("Fatal error.!");
    }
}

#if !ENABLE_DEBUG_ON_DEVICE && DEVELOPMENT_BUILD && !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
/// This class is enabled on device by default to disable all log info except for errors
public class Debug : InJoyDebugBase
{
}
#endif

/// <summary>
/// Log functions not implemented by default, can be used to hide all debug messages except for errors of some class.
/// Example: private class Debug : InJoyDebugBase {}
/// </summary>
public class InJoyDebugBase
{
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        UnityEngine.Debug.DrawLine(start, end);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        UnityEngine.Debug.DrawLine(start, end, color);
    }

    public static bool isDebugBuild
    {
        get { return UnityEngine.Debug.isDebugBuild; }
    }

    public static void Log(object message) { }

    public static void Log(object message, UnityEngine.Object context) { }

    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }

    public static void LogError(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogError(message, context);
    }

    public static void LogException(Exception exception) { }

    public static void LogException(Exception exception, UnityEngine.Object context) { }

    public static void LogWarning(object message) { }

    public static void LogWarning(object message, UnityEngine.Object context) { }


	public static void DebugCommandResponse(NetResponse response,ref BinaryReader reader){

		return;

		string commandName = response.GetType().Name;
		Regex regex = new Regex("Response");
		commandName = regex.Split(commandName)[0];
		FileInfo info = new FileInfo(Application.streamingAssetsPath+"/command/"+commandName+".txt");
		if(info.Exists){
			info.Delete();

		}
		FileStream fs = info.Create();
		fs.Position = 0;
		fs.SetLength(0);

		MemoryStream s = new MemoryStream();

		int BUFFER_SIZE = 1024;
		byte[] buf = new byte[BUFFER_SIZE];
		int n = reader.Read(buf, 0, BUFFER_SIZE);
		while (n > 0)
		{
			s.Write(buf, 0, n);
			fs.Write(buf,0,n);
			n = reader.Read(buf, 0, BUFFER_SIZE);
		}
		s.Position = 0;
		reader = new BinaryReader(s);


		fs.Close();
		fs.Dispose();
	}
}


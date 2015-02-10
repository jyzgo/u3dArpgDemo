using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Integrity {

	public static System.Version Version {
		get { return new System.Version (1, 0, 2); }
	}
	
	public const string KEY_IS_JAILBROKEN = "KEY_IS_JAILBROKEN";
	public const string KEY_IS_ENCRYPTED = "KEY_IS_ENCRYPTED";
	
	public static bool IsTrusted()
	{
		return IsEncrypted() && !IsJailbroken();
	}
	
	public static Dictionary<string, string> GetDetails()
	{
		return new Dictionary<string, string>()
		{
			{KEY_IS_JAILBROKEN, IsJailbroken().ToString()},
			{KEY_IS_ENCRYPTED, IsEncrypted().ToString()}
		};
	}
	
#if UNITY_IPHONE && !UNITY_EDITOR
	/// <summary>
	/// Determines whether iOS device is jailbroken.
	/// </summary>
	/// <returns>
	/// <c>true</c> if iOS device is jailbroken; otherwise, <c>false</c>.
	/// </returns>
	[DllImport("__Internal")]
	public static extern bool IsJailbroken();
	/// <summary>
	/// Determines whether app binary is encrypted.
	/// </summary>
	/// <returns>
	/// <c>true</c> if app binary is encrypted; otherwise, <c>false</c>. App binary must be encrypted for App Store distribution. 
	/// Missing encryption indicates that binary has been hacked. 
	/// </returns>
	[DllImport("__Internal")]
	public static extern bool IsEncrypted();
#else
	public static bool IsJailbroken ()
	{
		return false;
	}
	
	public static bool IsEncrypted ()
	{
		return false;
	}
#endif
}

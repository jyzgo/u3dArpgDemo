using UnityEngine;
using System.Collections;
#if UNITY_IPHONE
using System.Runtime.InteropServices;
#endif

public class CommonIOSPlugin 
{	
#if UNITY_IPHONE
	/* Interface to native implementation */
	
	[DllImport("__Internal")]
	private static extern string _getIdentifierForVendor ();
	
	/* Public interface for use inside C# / JS code */
	
	// Retrieve Vendor Identifier for device
	public static string GetVendorIdentifier()
	{
		// Call plugin only when running on real device
		if (Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.WindowsEditor)
			return _getIdentifierForVendor();
		return "{\"error\":\"not available on this platform\"}";
	}
#endif
}

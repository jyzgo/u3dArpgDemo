using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkUtils
{
	public static string GetUDID()
	{
		Debug.Log("Getting UDID...");
		
		string udid = PlayerPrefs.GetString("udid");
		if (udid == "")
		{			
#if UNITY_IPHONE		
			udid = PlayerPrefs.GetString("FCLoginID", "");
			if (udid == "")
			{
				Debug.Log("LoginId not found in PlayerPrefs, need to check iOS version to determine which identifier we will use");
				
				// grab the OS string and parse out the major version, e.g. 5, 6, 7 
				int iOSVersion = -1;
				string operatingSystemString = SystemInfo.operatingSystem;
				string versionString = SystemInfo.operatingSystem.Replace("iPhone OS ", "");
				int.TryParse(versionString.Substring(0, 1), out iOSVersion);
				
				Debug.Log("operatingSystemString: " + operatingSystemString + "\n" +
					"versionString: " + versionString + "\n" +
					"iOSVersion: " + iOSVersion); 
				
				if (iOSVersion < 7) // iOS < 7
				{
					Debug.Log("iOSVersion less than 7, using Unity's deviceUniqueIdentifier");
					
					udid = SystemInfo.deviceUniqueIdentifier;
				}
				else
				{
					Debug.Log("iOSVersion 7 or above, need to use the vendor identifier");
					
					udid = CommonIOSPlugin.GetVendorIdentifier();
					//todo:
					
					if (string.IsNullOrEmpty(udid))
					{
						Debug.Log("Could not retrieve vendorId from iOS libs");
						
						return null;	
					}
				}

                udid = "#" + udid;

                PlayerPrefs.SetString("FCLoginID", udid);
				PlayerPrefs.Save();
				
				Debug.Log("Identifier string saved into PlayerPrefs");
			}
#else
			if (Application.platform != RuntimePlatform.OSXEditor)
			{
				udid = SystemInfo.deviceUniqueIdentifier; //OpenUDID.GetUDID();
                udid = "005";
			}
			else
			{
				udid = System.Environment.UserName.Replace(".", "-") + "-0000-0000";//We need at least 8 valid hex characters		
			}
#endif
		}
        return udid;
	}

    public static string GetUDPassword( string udid)
    {
        udid = udid.Remove(0, 1);
        return Utils.Md5Encryt(string.Format("{0}INJOY", udid));
    }

    //todo_network
    public static bool IsSystemMessage(string msg)
    {
        return false;
    }

    public static short GetUDServerID()
    {
        List<ServerInfo> recommendedList = FCDownloadManager.Instance.GameServers.FindAll(
                        delegate(ServerInfo info)
                        {
                            return info.state == ServerState.Recommended;
                        });

        if (null != recommendedList && recommendedList.Count >= 1)
        {
            recommendedList.Sort(ServerInfo.CompareServerFromMaxToMin);
            return recommendedList[0].id;
        }

        List<ServerInfo> newList = FCDownloadManager.Instance.GameServers.FindAll(
                        delegate(ServerInfo info)
                        {
                            return info.state == ServerState.New;
                        });

        if (null != newList && newList.Count >= 1)
        {
            newList.Sort(ServerInfo.CompareServerFromMaxToMin);
            return newList[0].id;
        }

        List<ServerInfo> normalList = FCDownloadManager.Instance.GameServers.FindAll(
                        delegate(ServerInfo info)
                        {
                            return info.state == ServerState.Normal;
                        });

        if (null != normalList && normalList.Count >= 1)
        {
            normalList.Sort(ServerInfo.CompareServerFromMaxToMin);
            return normalList[0].id;
        }


        List<ServerInfo> hotList = FCDownloadManager.Instance.GameServers.FindAll(
                delegate(ServerInfo info)
                {
                    return info.state == ServerState.Hot;
                });

        if (null != hotList && hotList.Count >= 1)
        {
            hotList.Sort(ServerInfo.CompareServerFromMaxToMin);
            return hotList[0].id;
        }

        return 1;
    }


}

public enum LOGIN_STEP
{
    NOT_START = 0,
    LOGINING,
    LOGIN_OK,
    LOGIN_FAILED,
    LOGIN_RECONNECTING

};

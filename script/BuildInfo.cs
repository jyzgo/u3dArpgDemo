using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#else
using System.Text;
using System.Runtime.InteropServices;
#endif
using System.Collections;

namespace InJoy.UnityBuildSystem
{
	public class BuildInfo
	{
		public const string k_bundle_tag = "bundle_tag";

		public const string k_build_tag = "build_tag";

		private static string _buildTag;
		private static string _assetbundleUploadTag;
		public static string buildTag
		{
			get
			{
				if(string.IsNullOrEmpty(_buildTag))
				{
					// try to get from the Resources folder
					TextAsset textAsset = Resources.Load(k_build_tag) as TextAsset;
					if(textAsset != null)
					{
						_buildTag = textAsset.text.Trim();
					}
					else
					{
						_buildTag = "NOT_TAGGED";
					}
				}
				return _buildTag;
			}
			set
			{
				_buildTag = value;
			}
		}
		
		private static string _serverPostTag;
		public static string ServerPostTag
		{
			get
			{
				if (string.IsNullOrEmpty(_serverPostTag))
				{
					TextAsset textAsset = Resources.Load("server_post_tag") as TextAsset;
					if(textAsset != null)
					{
						_serverPostTag = textAsset.text.Trim();
					}
					else
					{
						_serverPostTag = "dev1";
					}
				}
				return _serverPostTag;
			}
			set
			{
				_serverPostTag = value;
			}
		}
		
		public static string assetbundleUploadTag
		{
			get
			{
				if(string.IsNullOrEmpty(_assetbundleUploadTag))
				{
					// try to get from the Resources folder
					TextAsset textAsset = Resources.Load(k_bundle_tag) as TextAsset;
					if(textAsset != null)
					{
						_assetbundleUploadTag = textAsset.text.Trim();
					}
					else
					{
						_assetbundleUploadTag = "NOT_TAGGED";
					}
				}
				return _assetbundleUploadTag;
			}
			set
			{
				_assetbundleUploadTag = value;
			}
		}
	
		public static string bundleId {
			get {
#if UNITY_EDITOR			
				return PlayerSettings.bundleIdentifier;
#elif UNITY_ANDROID
				return GetBundleId();
#elif UNITY_IPHONE
				StringBuilder sb = new StringBuilder(256);
				GetBundleId(sb, sb.Capacity);
				return sb.ToString();
#else
				return "";
#endif
			}
		}
		
		public static string bundleVersion {
			get {
#if UNITY_EDITOR			
				return PlayerSettings.bundleVersion;
#elif UNITY_ANDROID
				return GetBundleVersion();
#elif UNITY_IPHONE
				StringBuilder sb = new StringBuilder(256);
				GetBundleVersion(sb, sb.Capacity);
				return sb.ToString();
#else
				return "";
#endif
			}
		}
#if UNITY_ANDROID && !UNITY_EDITOR
		private static string _bundleId;
		private static string GetBundleId()
		{
			if(string.IsNullOrEmpty(_bundleId))
			{
				// _bundleId = context.getPackageName();
				using(AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using(AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						_bundleId = activity.Call<string>("getPackageName"); // activity is a context
					}
				}
			}
			return _bundleId;
		}
		
		private static string _bundleVersion;
		private static string GetBundleVersion()
		{
			if(string.IsNullOrEmpty(_bundleVersion))
			{
				// _bundleVersion = context.getPackageManager().getPackageInfo(context.getPackageName(), 0 ).versionName;
				using(AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using(AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						// activity is a context. Going further now
						using(AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
						{
							using(AndroidJavaObject packageName = activity.Call<AndroidJavaObject>("getPackageName"))
							{
								using(AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0))
								{
									_bundleVersion = packageInfo.Get<string>("versionName");
								}
							}
						}
					}
				}
			}
			return _bundleVersion;
		}
#endif
#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal", CharSet=CharSet.Ansi)]
		private static extern void GetBundleId(StringBuilder sb, int size);
		
		[DllImport("__Internal", CharSet=CharSet.Ansi)]
		private static extern void GetBundleVersion(StringBuilder sb, int size);
#endif
	}
}

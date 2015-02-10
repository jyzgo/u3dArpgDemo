using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace InJoy.DynamicContentPipeline
{
	using AssetBundles;
	
	// HACK: class Impl should be placed out of the
	// class in order to instantiate MonoBehaviour object
	// appropriately. The trick is to derive class from Impl,
	// place it in global space, instantiate it and work with
	// it just as with instance of the base class. This would
	// work out, because derived class _is_ the base class.
	using DYNAMIC_CONTENT_IMPL = DynamicContentPipelineImpl;
	
	/// <summary>
	/// Dedicated to work with dynamic content.
	/// </summary>
	public static class DynamicContent
	{
		#region Interface
		
		public delegate void OnVoidEvent();
		public delegate bool OnBoolEvent();
		
		/// <summary>
		/// Gets the actual version of the component.
		/// </summary>
		/// <value>
		/// The version.
		/// </value>
		public static Version version
		{
			get { return m_version ?? (m_version = new Version(1, 1, 4)); }
		}
		
		/// <summary>
		/// Sets the thread count to download asset bundles and custom configs with.
		/// </summary>
		/// <value>
		/// The thread count.
		/// </value>
		public static int ThreadCount
		{
			set
			{
				DownloadManager.ThreadCount = value;
				//CustomConfigs.ThreadCount = value;
			}
		}

		/// <summary>
		/// Gets or sets actual time interval between checks for updates.
		/// </summary>
		/// <value>
		/// The time interval, in seconds.
		/// </value>
		public static int TimeInterval
		{
			set { m_timeInterval = value; }
			get { return m_timeInterval; }
		}
		
		/// <summary>
		/// Gets or sets the name of the asset bundles index used if no
		/// ABTesting rules are available.
		/// </summary>
		/// <value>
		/// The name of the asset bundles index.
		/// </value>
		public static string AssetBundlesIndexName
		{
			set { m_assetBundlesIndexName = value; }
			get { return m_assetBundlesIndexName; }
		}
		
		/// <summary>
		/// Gets the loading progress.
		/// </summary>
		/// <value>
		/// The progress (between 0 and 1).
		/// </value>
		public static float Progress
		{
			get
			{
				Debug.Log("Progress - Started");
				// TODO: should ForcedUpdate, ABTesting, configs/overrides affect on progress?
				float progress = 0.0f;
				if(m_isIndexInfo && (m_indexInfo != null) && (m_indexInfo.downloadInfo != null) && (m_indexInfo.downloadInfo.state == DownloadManager.Info.State.Downloaded))
				{
					Debug.Log("Progress - Calculating");
					AssetBundleInfo[] assetBundlesInfo = m_indexInfo.assetBundleInfo;
					long sizeToLoad = 0;
					float loadedSize = 0.0f;
					foreach(AssetBundleInfo assetBundleInfo in assetBundlesInfo)
					{
						sizeToLoad += assetBundleInfo.Size;
						loadedSize += assetBundleInfo.downloadInfo.ReceivingProgress * assetBundleInfo.Size;
					}
					progress = (sizeToLoad > 0) ? (loadedSize / sizeToLoad) : 0.0f;
				}
				Debug.Log("Progress - Finished");
				return progress;
			}
		}
		
		private static string cloudFrontTargetAddress = null;
		private static string cloudFrontBaseAddress = null;
		private static bool isUseCloudFrontAddress = false;
		public static string CloudFrontTargetAddress
		{
			get
			{
				return cloudFrontTargetAddress;
			}
		}
		public static string CloudFrontBaseAddress
		{
			get
			{
				return cloudFrontBaseAddress;
			}
		}
		public static bool IsUseCloudFrontAddress
		{
			get
			{
				return isUseCloudFrontAddress;
			}
		}
		public static void UseCloudFrontAddress(string baseUrl, string cloudFrontAddress)
		{
			cloudFrontBaseAddress = baseUrl;
			cloudFrontTargetAddress = cloudFrontAddress;
			isUseCloudFrontAddress = true;
		}
		
		/// <summary>
		/// Gets IndexInfo instance of loading or loaded asset bundles.
		/// It allows to retrieve additional information, e.g. build tag.
		/// </summary>
		/// <value>
		/// IndexInfo instance.
		/// </value>
		public static IndexInfo AssetBundlesIndexInfo
		{
			get { return m_isIndexInfo ? m_indexInfo : null; }
		}
		
		/// <summary>
		/// Gets Resolution instance of loaded ABTesting rules.
		/// It allows to retrieve additional information, e.g. variant id.
		/// </summary>
		/// <value>
		/// Resolution instance.
		/// </value>
//		public static Resolution ABTestingResolution
//		{
//			get { return m_resolution; }
//		}
		
		/// <summary>
		/// Gets or sets the delegate to handle situation,
		/// if forced update is required.
		/// </summary>
		/// <value>
		/// The delegate.
		/// </value>
		public static OnVoidEvent OnForcedBinariesUpdate { set; get; }
		
		/// <summary>
		/// Gets or sets the delegate to handle situation,
		/// if forced update is available, but not required.
		/// Delegate should return true, if the game must be updated,
		/// and return false, if pipeline can proceed with next step.
		/// </summary>
		/// <value>
		/// The delegate.
		/// </value>
		public static OnBoolEvent OnAvailableBinariesUpdate { set; get; }
		
		/// <summary>
		/// Gets or sets the on no need binaries update.
		/// FC add
		/// </summary>
		/// <value>
		/// The on no need binaries update.
		/// </value>
		public static OnVoidEvent OnNoNeedBinariesUpdate {set;get;}
		
		/// <summary>
		/// Gets or sets the delegate to handle success.
		/// </summary>
		/// <value>
		/// The delegate.
		/// </value>
		public static OnVoidEvent OnFail { set; get; }
		
		/// <summary>
		/// Gets or sets the delegate to handle fail.
		/// </summary>
		/// <value>
		/// The delegate.
		/// </value>
		public static OnVoidEvent OnSuccess { set; get; }
		
		/// <summary>
		/// Gets or sets the delegate to handle updates of game-specific
		/// data like configs and overrides. It is used asynchronously.
		/// If update is successful, result should be set to true.
		/// Do not commit resolution (it is provided by the parameter).
		/// </summary>
		/// <value>
		/// The delegate.
		/// </value>
//		public static List<ICustomDynamicContent> CustomDynamicContent
//		{
//			set { m_customDynamicContent = value; }
//			get { return m_customDynamicContent; }
//		}
		

		public static void Init(params string[] baseUrls)
		{
			Debug.Log("Init - Started");
			Assertion.Check((baseUrls != null) && (baseUrls.Length > 0), "No URL has been specified!");
			if((baseUrls != null) && (baseUrls.Length > 0))
			{
				for(int idx = 0; idx < baseUrls.Length; ++idx)
				{
					while(baseUrls[idx].EndsWith("/"))
					{
						baseUrls[idx] = baseUrls[idx].Remove(baseUrls[idx].Length - 1);
					}
				}
				m_baseUrls = baseUrls;
			}
			else
			{
				m_baseUrls = new string[0];
			}
			Debug.Log("Init - Finished");
		}
		
		/// <summary>
		/// Starts to update dynamic content.
		/// Use delegates to handle different situations.
		/// </summary>
		public static void StartContentUpdate(DynamicContentParam param = null)
		{
			Debug.Log("StartContentUpdate - Started");
			Impl.StartContentUpdate(param);
			Debug.Log("StartContentUpdate - Finished");
		}
		
		/// <summary>
		/// Checks for the dynamic content updates.
		/// </summary>
		/// <returns>
		/// True, if updates have been found, otherwise false.
		/// </returns>
		public static bool CheckForUpdates()
		{
			Debug.Log("CheckForUpdates - Started");
			Impl.CheckForUpdates();
			Debug.Log("CheckForUpdates - Finished");
			return m_shouldBeUpdated;
		}
		
		public static void StartBinaryUpdate()
		{
			Impl.CheckForBinaryUpdate();
		}
		#endregion
		#region Implementation
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		public class Impl : MonoBehaviour
		{
			#region Interface
			
			public static void StartContentUpdate(DynamicContentParam param)
			{
				Debug.Log("StartContentUpdate - Started");
				Assertion.Check(!m_isLoadingInProgress);
				if(m_isLoadingInProgress)
				{
					Debug.Log("StartContentUpdate - Stopping update-in-progress");
					m_isLoadingInProgress = false;
					Instance.StopCoroutine("DoContentUpdate");
				}
				if(m_isCheckingInProgress)
				{
					Debug.Log("StartContentUpdate - Stopping check-in-progress");
					m_isCheckingInProgress = false;
					Instance.StopCoroutine("DoCheckForUpdates");
				}
				Reset(true);
				m_isLoadingInProgress = true;
				Instance.StartCoroutine("DoContentUpdate", param);
				Debug.Log("StartContentUpdate - Finished");
			}
			
			public static void StopContentUpdate()
			{
				Debug.Log("StopContentUpdate - Started");
				if(m_isLoadingInProgress)
				{
					Debug.Log("StopContentUpdate - Stopping update-in-progress");
					m_isLoadingInProgress = false;
					Instance.StopCoroutine("DoContentUpdate");
				}
				Debug.Log("StopContentUpdate - Finished");
				Reset(true);
			}
			
			public static void CheckForBinaryUpdate()
			{
				if(m_isCheckingInProgress)
				{
					Debug.Log("StartContentUpdate - Stopping check-in-progress");
					m_isCheckingInProgress = false;
					Instance.StopCoroutine("DoCheckForUpdates");
				}
				Reset(true);
				m_isCheckingInProgress = true;
				Instance.StartCoroutine("DoForceUpdateCheck");
			}
			
			
			public static void CheckForUpdates()
			{
				Debug.Log("CheckForUpdates - Started");
				if(!m_isLoadingInProgress)
				{
					if(!m_isCheckingInProgress)
					{
						Reset(false);
						m_isCheckingInProgress = true;
						Instance.StartCoroutine("DoCheckForUpdates");
					}
				}
				Debug.Log("CheckForUpdates - Finished");
			}
			
			#endregion
			#region Implementation
			
			private static Impl Instance
			{
				get
				{
					if(m_instance != null)
					{
						return m_instance;
					}
					else
					{
						// create singleton object
						GameObject go = GameObject.Find(kGameObjectName);
						if(go != null)
						{
							m_instance = go.GetComponent<DYNAMIC_CONTENT_IMPL>();
							Assertion.Check(m_instance != null);
							return m_instance;
						}
						else
						{
							Debug.Log(string.Format("CreateInstance \"{0}\"", kGameObjectName));
							go = new GameObject(kGameObjectName);
							go.AddComponent<DYNAMIC_CONTENT_IMPL>();
							UnityEngine.Object.DontDestroyOnLoad(go);
							m_instance = go.GetComponent<DYNAMIC_CONTENT_IMPL>();
							m_instance.useGUILayout = false; // no OnGUI()
							m_instance.enabled = false;      // no FixedUpdate(), no Update()
							return m_instance;
						}
					}
				}
			}
			
			public static string BuildTag
			{
				get
				{
					return InJoy.UnityBuildSystem.BuildInfo.buildTag;
				}
			}
			
			public static string AssetBundleUploadTag
			{
				get
				{
					return InJoy.UnityBuildSystem.BuildInfo.assetbundleUploadTag;
				}
			}
			
			public static string Config
			{
				get
				{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					return "stage";
#else
					return "live";
#endif
				}
			}
			
			public static string Platform
			{
				get
				{
					string ret = "";
					switch(Application.platform)
					{
					case RuntimePlatform.Android:
						ret = "android";
						break;
					case RuntimePlatform.IPhonePlayer:
						ret = "ios";
						break;
					case RuntimePlatform.OSXPlayer:
						ret = "mac";
						break;
					case RuntimePlatform.WindowsPlayer:
						ret = "win";
						break;
#if UNITY_EDITOR
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.WindowsEditor:
#if UNITY_WEBPLAYER
						// nothing to do
#elif UNITY_ANDROID
						ret = "android";
#elif UNITY_IPHONE
						ret = "ios";
#elif UNITY_STANDALONE_OSX
						ret = "mac";
#elif UNITY_STANDALONE_WIN
						ret = "win";
#else
						// nothing to do
#endif
						break;
#endif
					default:
						break;
					}
					return ret;
				}
			}
			
			private static string[] UrlsToForcedUpdate
			{
				get
				{
					string[] ret = new string[m_baseUrls.Length];
					for(int idx = 0; idx < m_baseUrls.Length; ++idx)
					{
						ret[idx] = string.Format("{0}/{1}/ForcedUpdate.xml", m_baseUrls[idx], Platform);
					}
					return ret;
				}
			}
/*			
			private static string[] UrlsToABTestingDecisionTable
			{
				get
				{
					string[] ret = new string[m_baseUrls.Length];
					for(int idx = 0; idx < m_baseUrls.Length; ++idx)
					{
						ret[idx] = string.Format("{0}/{1}/{2}/bundles/{3}/ABTesting.xml", m_baseUrls[idx], Platform, Config, BuildTag);
					}
					return ret;
				}
			}

			public static string UrlToDynamicContentInfo
			{
				get
				{
					string ret = string.Format("{0}/{1}/{2}/bundles/{3}/DynamicContentInfo.json", m_baseUrls[0], Platform, Config, BuildTag);
					return ret;
				}
			}
			
			private static string[] UrlsToDefaultAssetBundles
			{
				get
				{
					string[] ret = new string[m_baseUrls.Length];
					for(int idx = 0; idx < m_baseUrls.Length; ++idx)
					{
						ret[idx] = string.Format("{0}/{1}/{2}/AssetBundles/{3}.version", m_baseUrls[idx], Platform, BuildTag, AssetBundlesIndexName);
					}
					return ret;
				}
			}
*/			
			private static void Reset(bool resetAll)
			{
				Debug.Log("Reset - Started");
				if(resetAll)
				{
					//m_resolution = null;
					m_isIndexInfo = false; //m_indexInfo = null;
				}
				//m_updateInfo = null;
				Debug.Log("Reset - Finished");
			}
/*			
			private IEnumerator DoForceUpdateCheck()
			{
				// check for forced updates
				Debug.Log("DoContentUpdate - ForcedUpdate - Request");
				ForcedUpdate.Init(UrlsToForcedUpdate[0], BuildTag);
				ForcedUpdate.CheckUpdateStatus();
				Debug.Log("DoContentUpdate - ForcedUpdate - Waiting for the result");
				DateTime dateTime = DateTime.Now;
				while(ForcedUpdate.IsCheckInProgress())
				{
					if((DateTime.Now - dateTime).TotalSeconds > kForcedUpdateTimeOut)
					{
						Debug.Log("DoContentUpdate - ForcedUpdate - Timed out");
						break;
					}
					yield return null;
				}
				Debug.Log("DoContentUpdate - ForcedUpdate - Got result");
				if(ForcedUpdate.NeedToQuit())
				{
					Debug.Log("DoContentUpdate - ForcedUpdate - NeedToQuit");
					m_shouldBeUpdated = true;
					if(OnForcedBinariesUpdate != null)
					{
						OnForcedBinariesUpdate();
					}
					m_isLoadingInProgress = false;
					yield break;
				}
				else if(ForcedUpdate.NeedToUpdate())
				{
					Debug.Log("DoContentUpdate - ForcedUpdate - NeedToUpdate");
					if(OnAvailableBinariesUpdate != null)
					{
						if(OnAvailableBinariesUpdate())
						{
							m_shouldBeUpdated = true;
							m_isLoadingInProgress = false;
							yield break;
						}
					}
				}
				else
				{
					if(OnNoNeedBinariesUpdate != null)
					{
						OnNoNeedBinariesUpdate();
					}
					Debug.Log("DoContentUpdate - ForcedUpdate - UpToDate");
				}
				Debug.Log("DoContentUpdate - ForcedUpdate - Finished");
			}
*/			
			private IEnumerator DoContentUpdate(DynamicContentParam param)
			{
				Debug.Log("DoContentUpdate - Started");
				
				// check for forced updates
				#region force update from msk
/*
				Debug.Log("DoContentUpdate - ForcedUpdate - Request");
				ForcedUpdate.Init(UrlsToForcedUpdate[0], BuildTag);
				ForcedUpdate.CheckUpdateStatus();
				Debug.Log("DoContentUpdate - ForcedUpdate - Waiting for the result");
				DateTime dateTime = DateTime.Now;
				while(ForcedUpdate.IsCheckInProgress())
				{
					if((DateTime.Now - dateTime).TotalSeconds > kForcedUpdateTimeOut)
					{
						Debug.Log("DoContentUpdate - ForcedUpdate - Timed out");
						break;
					}
					yield return null;
				}
				Debug.Log("DoContentUpdate - ForcedUpdate - Got result");
				if(ForcedUpdate.NeedToQuit())
				{
					Debug.Log("DoContentUpdate - ForcedUpdate - NeedToQuit");
					m_shouldBeUpdated = true;
					if(OnForcedBinariesUpdate != null)
					{
						OnForcedBinariesUpdate();
					}
					m_isLoadingInProgress = false;
					yield break;
				}
				else if(ForcedUpdate.NeedToUpdate())
				{
					Debug.Log("DoContentUpdate - ForcedUpdate - NeedToUpdate");
					if(OnAvailableBinariesUpdate != null)
					{
						if(OnAvailableBinariesUpdate())
						{
							m_shouldBeUpdated = true;
							m_isLoadingInProgress = false;
							yield break;
						}
					}
				}
				else
				{
					Debug.Log("DoContentUpdate - ForcedUpdate - UpToDate");
				}
				Debug.Log("DoContentUpdate - ForcedUpdate - Finished");
*/
				#endregion
				#region ABTesting from msk
				// prepare initial variant data for the resolution
/*
				Debug.Log("DoContentUpdate - ABTesting - Prepare initial variant");
				Dictionary<string, string> initialVariantData = new Dictionary<string, string>();
				initialVariantData.Add(DecisionTable.RESOLUTION_KEY_VARIANT_ID, "DefaultOffline");
				string str = null;
				foreach(string url in UrlsToDefaultAssetBundles)
				{
					str = (str != null) ? (str + ";" + url) : url;
				}
				initialVariantData.Add("AssetBundlesUrls", str);
				
				// do A/B testing
				Debug.Log("DoContentUpdate - ABTesting - Request resolution");
				m_resolution = Resolution.Retrieve(initialVariantData, UrlsToABTestingDecisionTable);
				Debug.Log("DoContentUpdate - ABTesting - Waiting for the result");
				while(!m_resolution.Ready)
				{
					yield return null;
				}
				Debug.Log("DoContentUpdate - ABTesting - Resolved");
				
				// unload previously loaded asset bundles
				if(m_indexInfo != null && !param.IsAddonContent)
				{
					Debug.Log("DoContentUpdate - AssetBundles - Unload previously loaded ones");
					m_indexInfo.UnloadAll(true);
				}
				
				bool success = false;
#if !ASSET_BUNDLE_FORCE_ONLINE
				Assertion.Check(m_resolution.Data != null, "DoContentUpdate - Retrieved ABTesting variant data are corrupted");
#endif
*/
				#endregion
				
				#region Get Asset bundles list from server
				bool success = false;
//				Debug.Log("Get Asset Bundles list from: "+UrlToDynamicContentInfo);
//				DynamicContentInfo DCInfo = new DynamicContentInfo(UrlToDynamicContentInfo);
//				StartCoroutine(DCInfo.StartDownloadContent());
//				while(!DCInfo.Ready)
//				{
//					yield return null;
//				}
//				Hashtable assetBundleInfo = DCInfo.Result;
				#endregion
				
//				if((m_resolution.Data != null) && m_resolution.Data.ContainsKey("AssetBundlesUrls"))
				if(param.DCInfoCache != null)
				{
					// load asset bundles
					Debug.Log("DoContentUpdate - AssetBundles - Download");
					//make multi-version files
					string[] orgVersionFileUrls = param.DCInfoCache.AssetBundleList.ToArray(typeof(string)) as string[];
//					if(param != null && param.ContentType == DynamicContentParam.EnumContentType.CT_SPECIAL_INDEX)
//					{
					string curVersionFileUrl = param.CheckAllSpecialIndexNameValid(orgVersionFileUrls);
					Assertion.Check(!string.IsNullOrEmpty(curVersionFileUrl), "DoContentUpdate- Wrong Special Index Name"+param.SpecialIndexName);
						
//					}
//					else 
//					{
//						versionFileUrls = orgVersionFileUrls;
//					}

//					if (versionFileUrls == null)
//					{
//						Debug.LogWarning("No Asset Bundle is need update");
//						success = true;
//					}
//					else
//					{
					LogEventStarted(curVersionFileUrl);
					string[] temp = {curVersionFileUrl};
					m_indexInfo = AssetBundles.DownloadAll(temp);
					param.TargetIndexDownloadInfo.IndexDownloadInfo = m_indexInfo;
					m_isIndexInfo = true;
					Debug.Log("DoContentUpdate - AssetBundles - Waiting for the result");
					while(m_indexInfo.state == IndexInfo.State.InProgress)
					{
						yield return null;
					}
					
					LogEventEnded(m_indexInfo);
					Debug.Log(string.Format("DoContentUpdate - AssetBundles - Downloaded with result: {0}, from: {1}", m_indexInfo.state, m_indexInfo.source));
					switch(m_indexInfo.state)
					{
					case IndexInfo.State.Failed:
						success = false;
						break;
					case IndexInfo.State.Succeeded:
						success = true;
						break;
					default:
						// SANITY CHECK
						Assertion.Check(false);
						break;
					}
					//}
				}
				else
				{
					success = false;
				}
				
//				if(success)
//				{
//					Assertion.Check(CustomDynamicContent != null);
//					if((CustomDynamicContent != null) && (CustomDynamicContent.Count > 0))
//					{
//						// load optional configs
//						Debug.Log("DoContentUpdate - CustomUpdate");
//						foreach(ICustomDynamicContent customDynamicContent in CustomDynamicContent)
//						{
//							customDynamicContent.StartContentUpdate(m_resolution);
//							while(customDynamicContent.IsInProgress)
//							{
//								yield return null;
//							}
//							success &= customDynamicContent.Result;
//						}
//						Debug.Log("DoContentUpdate - CustomUpdate is finished with result: {0}", success);
//					}
//					else
//					{
//						success = true;
//					}
//				}
//				
//				if(success)
//				{
//					Debug.Log("DoContentUpdate - ABTesting - Commiting variant");
//					success = m_resolution.Commit();
//				}
				
				Debug.Log("DoContentUpdate - Overall result is {0}");
				if(success)
				{
					if(OnSuccess != null)
					{
						Debug.Log("DoContentUpdate - OnSuccess");
						OnSuccess();
					}
				}
				else
				{
					if(OnFail != null)
					{
						Debug.Log("DoContentUpdate - OnFail");
						OnFail();
					}
				}
				
				// finish
				m_shouldBeUpdated = false;
				m_isLoadingInProgress = false;
				//m_wasOneLoadAtLeast = true;
				Debug.Log("DoContentUpdate - Finished");
				yield break;
			}
/*			
			private IEnumerator DoCheckForUpdates()
			{
				Debug.Log("DoCheckForUpdates - Started");
				if(m_wasOneLoadAtLeast && !m_shouldBeUpdated)
				{
					// check for forced updates
					Debug.Log("DoCheckForUpdates - ForcedUpdate - Request");
					ForcedUpdate.CheckUpdateStatus();
					Debug.Log("DoCheckForUpdates - ForcedUpdate - Waiting for the result");
					DateTime dateTime = DateTime.Now;
					while(ForcedUpdate.IsCheckInProgress())
					{
						if((DateTime.Now - dateTime).TotalSeconds > kForcedUpdateTimeOut)
						{
							Debug.Log("DoCheckForUpdates - ForcedUpdate - Timed out");
							break;
						}
						yield return null;
					}
					Debug.Log("DoCheckForUpdates - ForcedUpdate - Got result");
					if(ForcedUpdate.NeedToQuit())
					{
						Debug.Log("DoCheckForUpdates - NeedToQuit");
						m_shouldBeUpdated = true;
					}
					else
					{
						// check for ABTesting updates
						Debug.Log("DoCheckForUpdates - ABTesting - Request resolution");
						Resolution resolution = Resolution.Retrieve(null, UrlsToABTestingDecisionTable);
						Debug.Log("DoCheckForUpdates - ABTesting - Waiting for the result");
						while(!resolution.Ready)
						{
							yield return null;
						}
						Debug.Log("DoCheckForUpdates - ABTesting - Got result");
						if(resolution.IsApplied)
						{
							// check for asset bundles updates
							if(m_indexInfo != null)
							{
								Debug.Log("DoCheckForUpdates - AssetBundles - Check for updates");
								m_updateInfo = m_indexInfo.CheckForUpdates();
								Assertion.Check(m_updateInfo != null);
								Debug.Log("DoCheckForUpdates - AssetBundles - Waiting for the result");
								while(m_updateInfo.state == UpdateInfo.State.Pending)
								{
									yield return null;
								}
								Debug.Log("DoCheckForUpdates - AssetBundles - Got result: {0}", m_updateInfo.state);
								if(m_updateInfo.state == UpdateInfo.State.Outdated)
								{
									Debug.Log("DoCheckForUpdates - AssetBundles - Outdated");
									m_shouldBeUpdated = true;
								}
							}
							else
							{
								Debug.LogWarning("DoCheckForUpdates - AssetBundles - Nothing to check");
							}
							
							if(!m_shouldBeUpdated)
							{
								Assertion.Check(CustomDynamicContent != null);
								if((CustomDynamicContent != null) && (CustomDynamicContent.Count > 0))
								{
									// check for custom updates
									Debug.Log("DoCheckForUpdates - CustomUpdate - Check for updates");
									foreach(ICustomDynamicContent customDynamicContent in CustomDynamicContent)
									{
										customDynamicContent.CheckForUpdates(resolution);
										while(customDynamicContent.IsInProgress)
										{
											yield return null;
										}
										m_shouldBeUpdated |= customDynamicContent.Result;
									}
									Debug.Log("DoCheckForUpdates - CustomUpdate - Got result: {0}", m_shouldBeUpdated);
								}
								else
								{
									Debug.Log("DoCheckForUpdates - All are up-to-date");
								}
							}
						}
						else
						{
							Debug.Log("DoCheckForUpdates - ABTesting - Outdated");
							m_shouldBeUpdated = true;
						}
					}
				}
				
				// wait for the time before next request
				Debug.Log("DoCheckForUpdates - Request is finished with result: {0}, waiting for the time interval: {1}", m_shouldBeUpdated, TimeInterval);
				yield return new WaitForSeconds(TimeInterval);
				
				m_isCheckingInProgress = false;
				Debug.Log("DoCheckForUpdates - Finished");
				yield break;
			}
*/			
			private void LogEventStarted(string urls)
			{
				Assertion.Check(urls != null);
				++m_nTriesCount;
				m_dateTime0 = DateTime.Now;
//				Dictionary<string, object> dict = new Dictionary<string, object>();
//				dict.Add("Number of attempts", m_nTriesCount);
//				dict.Add("URL", urls ?? "none");
//				CFlurry.LogEvent("BUNDLES_DOWNLOAD_START", dict);
			}
			
			private void LogEventEnded(IndexInfo indexInfo)
			{
				Assertion.Check(indexInfo != null);
				Assertion.Check(m_nTriesCount > 0);
				Assertion.Check(m_dateTime0 != DateTime.MinValue);
//				Dictionary<string, object> dict = new Dictionary<string, object>();
//				dict.Add("Time spent", (DateTime.Now - m_dateTime0).TotalSeconds);
//				dict.Add("Result", indexInfo.state);
//				dict.Add("Source", indexInfo.source);
//				dict.Add("Build tag", indexInfo.BuildTag ?? "none");
//				dict.Add("Hash of cached content", indexInfo.CachedContentHash ?? "none");
//				dict.Add("Hash of loaded content", indexInfo.ContentHash ?? "none");
//				CFlurry.LogEvent("BUNDLES_DOWNLOAD_END", dict);
				m_dateTime0 = DateTime.MinValue;
			}
			
			private const string kGameObjectName = "DynamicContent GameObject";
			private static Impl m_instance = null;
			
			// to drop hung ForcedUpdate
			private const double kForcedUpdateTimeOut = 5.0; // in seconds
			
			// to collect data
			private static long m_nTriesCount = 0;
			private static DateTime m_dateTime0 = DateTime.MinValue;
			
			#endregion
		}
		
		private static Version m_version = null;
		private static int m_timeInterval = 300; // by default
		private static string m_assetBundlesIndexName = "index"; // by default
		//private static List<ICustomDynamicContent> m_customDynamicContent = new List<ICustomDynamicContent>();
		private static string[] m_baseUrls = null;
		//private static bool m_wasOneLoadAtLeast = false;
		private static bool m_isLoadingInProgress = false;
		private static bool m_isCheckingInProgress = false;
		private static bool m_shouldBeUpdated = false;
//		private static Resolution m_resolution = null;
		private static bool m_isIndexInfo = false;
		private static IndexInfo m_indexInfo = null;
		//private static UpdateInfo m_updateInfo = null;
		
		#endregion
	}
}

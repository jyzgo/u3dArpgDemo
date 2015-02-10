using UnityEngine;
using System;
using System.Collections;


public class DynamicContentInfo 
{
	public delegate void OnDCInfoFailedDelegate();
	public OnDCInfoFailedDelegate OnDCInfoFailed = null;
	
	public bool Ready
	{
		set;get;
	}
	
	public ArrayList AssetBundleList
	{
		get
		{
			return assetBundleList;
		}
	}
	
	public string AssetBundleTag
	{
		get
		{
			return assetBundleTag;
		}
	}
	
	private string targetURL = null;
	private const double dynamicContentTimeOut = 5.0;//s
	private ArrayList assetBundleList = null;
	//private Hashtable assetBundleSize = null;
	private string assetBundleTag = null;
	
	public DynamicContentInfo(string infoURL)
	{
		Ready = false;
		targetURL = infoURL;
	}
	
	public IEnumerator StartDownloadContent()
	{
		Debug.Log("Start download Dynamic Content Info");
		WWW getDynamicContentInfo = new WWW(targetURL);
		DateTime utcNow = DateTime.UtcNow;
		while(!getDynamicContentInfo.isDone && string.IsNullOrEmpty(getDynamicContentInfo.error))
		{
			if((DateTime.UtcNow - utcNow).TotalSeconds >= dynamicContentTimeOut)
			{
				Ready = false;
				if(OnDCInfoFailed != null)
				{
					OnDCInfoFailed();
				}
				yield break;
			}
			else
			{
				yield return null;
			}
		}
		
		if(!string.IsNullOrEmpty(getDynamicContentInfo.error))
		{
			Ready = false;
			if(OnDCInfoFailed != null)
			{
				OnDCInfoFailed();
			}
			yield break;
		}
		
		Hashtable result = InJoy.Utils.FCJson.jsonDecode(getDynamicContentInfo.text) as Hashtable;
		ArrayList temp = result["AssetBundlesUrls"] as ArrayList;
		assetBundleList = new ArrayList();
		foreach(string curAddress in temp)
		{
			assetBundleList.Add(curAddress.Replace(FCDownloadManager.URL_BUNDLE_BASE_ADDRESS, FCDownloadManager.URL_SERVER_ROOT));
		}
		//assetBundleSize = result["AssetBundlesSizes"] as Hashtable;
		assetBundleTag = result["AssetBundlesTag"] as string;
		Ready = true;
		Debug.Log("Finished download Dynamic Content Info");
	}
}

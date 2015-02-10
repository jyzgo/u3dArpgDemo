using UnityEngine;
using System.Collections;
using System;

public class WebTexture : MonoBehaviour {
	
	public UITexture targetUITexture = null;
	public string targetWebTextureName = null;
	
	enum WebTextureState
	{
		WTS_WAITING,
		WTS_DOWNLOADING,
		WTS_COMPLETE,
		WTS_RETRY,
	}
	
	DateTime lastRefreshTimeStamp = DateTime.MinValue;
	WebTextureState curState = WebTextureState.WTS_WAITING;
	const double webTextureTimeOut = 10.0;//second
	const double webTextureRefresh = 10.0;//min
	
	// Use this for initialization
	void Start () 
	{
		ActiveWebTexture();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(curState == WebTextureState.WTS_RETRY)
		{
			curState = WebTextureState.WTS_WAITING;
			ActiveWebTexture();
		}
		if(curState == WebTextureState.WTS_COMPLETE && (DateTime.UtcNow - lastRefreshTimeStamp).TotalMinutes >= webTextureRefresh)
		{
			curState = WebTextureState.WTS_WAITING;
			ActiveWebTexture();
		}
		targetUITexture.enabled = curState == WebTextureState.WTS_COMPLETE;
	}
	
	public void ActiveWebTexture()
	{
		if(curState == WebTextureState.WTS_WAITING)
		{
			StartCoroutine("DoActiveWebTexture");
		}
	}
	
	public void InactiveWebTexture()
	{
		if(curState == WebTextureState.WTS_DOWNLOADING || curState == WebTextureState.WTS_RETRY)
		{
			StopCoroutine("DoActiveWebTexture");
		}
		curState = WebTextureState.WTS_WAITING;
	}
	
	IEnumerator DoActiveWebTexture()
	{
		Debug.Log("WebTexture: Start web Texture");
		DateTime utcNow = DateTime.UtcNow;
		float lastProgress = 0.0f;
		curState = WebTextureState.WTS_DOWNLOADING;
		yield return null;
		string address = FCDownloadManager.ServerWebTextureAddress +"/"+ targetWebTextureName.Replace("<language>", LocalizationContainer.CurSystemLang);
		Debug.Log("WebTexture: web Texture address is " + address);
		WWW getWebTexture = new WWW(address);
		while(!getWebTexture.isDone && string.IsNullOrEmpty(getWebTexture.error))
		{
			if((DateTime.UtcNow - utcNow).TotalSeconds >= webTextureTimeOut)
			{
				Debug.Log("WebTexture: Error Time out. Retry");
				curState = WebTextureState.WTS_RETRY;
				yield break;
			}
			else if(getWebTexture.progress > lastProgress)
			{
				utcNow = DateTime.UtcNow;
				lastProgress = getWebTexture.progress;	
			}
			yield return null;
		}
		
		if(!string.IsNullOrEmpty(getWebTexture.error))
		{
			Debug.Log("WebTexture: Error. Error is" + getWebTexture.error);
			curState = WebTextureState.WTS_RETRY;
			yield break;
		}
		
		targetUITexture.mainTexture = getWebTexture.texture;
		curState = WebTextureState.WTS_COMPLETE;
		lastRefreshTimeStamp = DateTime.UtcNow;
		Debug.Log("WebTexture: Finished web Texture");
	}
}

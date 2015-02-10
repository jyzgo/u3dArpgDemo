using UnityEngine;
using System.Collections;


[AddComponentMenu("Trigger/SoundTrigger")]
public class SoundTrigger : MonoBehaviour {
	
	private bool enable;
	public string _clipName = "battle";
	public bool _autoPlay = false;
	
	void Awake()
	{
		enable = true;
	}
	
	void Start()
	{
		if(_autoPlay)
		{
			if(SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayBGM(_clipName,0.5f);
			}	
			enable = false;
		}
	}
	
	public void OnTriggerEnter(Collider other)
	{
		if(!enable)
		{
			return;
		}
		
		//if(other.gameObject.tag.Equals("Player"))
		{
			enable =false;
			if(SoundManager.Instance != null)
			{
				SoundManager.Instance.PlayBGM(_clipName,1.0f);
			}
		}
	}
	
}

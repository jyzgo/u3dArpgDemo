using UnityEngine;
using System.Collections;

public class NotificationController : MonoBehaviour
{
	public GameObject _levelupUI;
	public float _durationFactor = 1.0f;
	
	
	// Use this for initialization
	void Start () 
	{
		_levelupUI.SetActive(false);
		
		PlayerInfo.Instance.OnLevelUp += OnNotifyLevelup;
	}

	void OnDestroy()
	{
		PlayerInfo.Instance.OnLevelUp -= OnNotifyLevelup;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (_levelupUI.activeSelf)
		{
			Animation anim = _levelupUI.GetComponentInChildren<Animation>();
			if (!anim.isPlaying)
			{
				_levelupUI.SetActive(false);
			}
		}
	}
	
	[ContextMenu("BattleRewardsUI")]
	public void BattleRewardsUI()
	{
		UIManager.Instance.OpenUI("BattleRewardsUI");
	}
	
	public void OnNotifyLevelup(int level)
	{
		_levelupUI.SetActive(true);			
		
		TweenAlpha tweener = _levelupUI.GetComponentInChildren<TweenAlpha>();
		tweener.enabled = true;
		tweener.Reset();
		tweener.onFinished = OnPlayLevelup;
		
		Animation anim = _levelupUI.GetComponentInChildren<Animation>();
		anim["board"].speed = 1/_durationFactor;
		anim.wrapMode = WrapMode.Once;
		anim.Play();
		//label.text = Localization.instance.Get("IDS_LEVEL_UP_BIG");
	}
	
	void OnPlayLevelup(UITweener tweener)
	{
	}
}

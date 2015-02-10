using UnityEngine;
using System.Collections.Generic;



public class BubbleEffectManager : MonoBehaviour 
{
#region Singleton
	private static BubbleEffectManager _instance = null;
	public static BubbleEffectManager Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = FindObjectOfType(typeof(BubbleEffectManager)) as BubbleEffectManager;
			}
			
			return _instance;
		}
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
#endregion
	

	// Prefab of bubble
	public GameObject _bubblePrefab;
	
	// Prefab of dynamic font
	public GameObject _dynamicFontPrefab;
	
	public float _baseScale = 0.5f;
	
	public bool _BubbleEffectEnabled = true;
	
	public Color _normalColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color _bloodColor = new Color(1.0f,0.0f,0.0f,1.0f);
	public Color _goldedColor = new Color(1.0f,1.0f,0.0f, 1.0f);
	
	
	void Awake()
	{
		foreach (Transform t in transform)
		{
			t.gameObject.SetActive(false);
		}
	}
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Find the first unused bubble slot.
	public Transform FindFirstUnusedSlot(AnimationPlayer.BubbleType type)
	{
		foreach (Transform t in transform)
		{
			if (!t.gameObject.activeSelf)
			{
				AnimationPlayer player = t.gameObject.GetComponent<AnimationPlayer>();
				
				if (player.BubblePrefabType == type)
				{
					return t;
				}
			}
		}
		
		// Initialise a new slot.
		return AddNewBubbleSlot(type);
	}

	Transform AddNewBubbleSlot(AnimationPlayer.BubbleType type)
	{
		GameObject go;
		
		if (type == AnimationPlayer.BubbleType.BitmapFont)
		{
			go = NGUITools.AddChild(gameObject, _bubblePrefab);
		}
		else
		{
			go = NGUITools.AddChild(gameObject, _dynamicFontPrefab);
		}
	
		return go.transform;
	}
	
	
 
	// Bubble text from head of our character
	public void BubbleUpEffect(string text, Color color, Vector3 source, float size_factor)
	{
		if(!_BubbleEffectEnabled) {
			return;
		}
		Transform t = FindFirstUnusedSlot(AnimationPlayer.BubbleType.BitmapFont);
		
		t.position = source;
		t.gameObject.SetActive(true);
		
		AnimationPlayer ap = t.gameObject.GetComponent<AnimationPlayer>();
		ap.eventReceiver = gameObject;
		
		BubbleEffect be = t.gameObject.GetComponent<BubbleEffect>();
		UILabel label = be._text;
		label.text = text;
		label.color = color;
		label.transform.localScale = new Vector3(_baseScale,_baseScale,1.0f) * size_factor;
		
		AnimationPlayer player = t.GetComponent<AnimationPlayer>();
		player.Play();

		
		TweenAlpha alpha = t.gameObject.GetComponentInChildren<TweenAlpha>();
		alpha.enabled = true;
		alpha.Reset();
	}
	
	// Bubble Hurt Effect
	public void BubbleUpHurtEffect(string text, bool critical, FC_ELITE_TYPE type, bool isFromSkill, Vector3 pos)
	{
		float size_factor = 1.0f;
		Color color = _normalColor;
		
		// Scale
		if (type == FC_ELITE_TYPE.Boss
			|| type == FC_ELITE_TYPE.MiniBoss)
		{
			size_factor = 2.0f;
		}
		else if (type == FC_ELITE_TYPE.Elite)
		{
			size_factor = 1.5f;
		}
		else
		{
			size_factor = 1.0f;
		}
		
		if (critical)
		{
			size_factor = 2.0f;
		}
		
		// Critical
		if (critical && type==FC_ELITE_TYPE.hero)
		{
			text = "!";		
			size_factor = 2.0f;
		}
		
		
		
		// Color
		if ((type!=FC_ELITE_TYPE.hero) && (!isFromSkill))
		{
			color = _normalColor;
		}
		else if ((type!=FC_ELITE_TYPE.hero) && (isFromSkill))
		{
			color = _goldedColor;
		}
		else if (type==FC_ELITE_TYPE.hero)
		{
			color = _bloodColor;
		}
		
		BubbleUpEffect(text, color, pos, size_factor);
	}
	
	void OnFinishBubble(ActiveAnimation anim)
	{
		anim.transform.parent.parent.gameObject.SetActive(false);
	}
	
	public void BubbleUpDynamicFont(string text, Vector3 source, bool symbol)
	{
		BubbleUpDynamicFont(text, "", source, symbol);
	}
	
	// Create text use dynamic
	public void BubbleUpDynamicFont(string text, string text2, Vector3 source, bool symbol)
	{
		if(!_BubbleEffectEnabled) {
			return;
		}
		Transform t = FindFirstUnusedSlot(AnimationPlayer.BubbleType.DynamicFont);
		t.position = source;
		t.gameObject.SetActive(true);
		
		BubbleEffect be = t.gameObject.GetComponent<BubbleEffect>();
		UILabel label = be._text;
		label.text = text;
		be._text2.text = text2;
		
		be._sprite.SetActive(symbol);
		
		TweenAlpha alpha = t.gameObject.GetComponentInChildren<TweenAlpha>();
		alpha.enabled = true;
		alpha.Reset();
		
		AnimationPlayer player = t.gameObject.GetComponent<AnimationPlayer>();
		player.eventReceiver = gameObject;
		player.callWhenFinished = "OnFinishBubble";
		player.Play();
	}
}

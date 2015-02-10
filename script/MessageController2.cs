using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class MessageController2 : MonoBehaviour {
	
	public GameObject _all;
	public UILabel _label;
	
	private List<Message> _list = new List<Message>();
	private bool _isOpen = false;
	private bool _isShowing = false;
	private float _timer = 0;	
	public Message _curMessage = null;
	
	private static MessageController2 _instance = null;
	public static MessageController2 Instance
	{
		get{
			if(_instance == null)
			{
				_instance = FindObjectOfType(typeof(MessageController2)) as MessageController2;
			}
			return _instance;
		}	
	}
	
	void OnDestroy()
	{
		_instance = null;	
	}
	
	void Awake()
	{
		_instance = this;
		Reset();
	}
	
	public void Reset()
	{
		_all.SetActive(false);
		_list.Clear();
		_isShowing = false;
		_isOpen = false;
		_timer = 0;
		_curMessage = null;
	}
	
	
	public void CloseMessage(string removeText)
	{
		if(_curMessage != null && _curMessage._text == removeText)
		{
			_curMessage._time = 0;
		}
			
		foreach(Message message in _list)
		{
			if(message._text == removeText)
			{
				message._time = 0;
			}
		}
	}
	
	public void CloseCurMessage()
	{
		if(_curMessage != null)
		{
			_curMessage._time = 0;
		}
	}
	
	public void AddMessage(float time,string text)
	{
		Message message = new Message();
		message._text = text;
		message._time = time;
		
		_list.Add(message);
		
		if(!_isShowing)
		{
			ShowNext();
		}
	}
	
	public void AddMessageIds(float time,string textIds, params object[] args)
	{
		AddMessage(time,ResolveString(textIds,args));	
	}
	
	
	public static string ResolveString(string textIds, params object[] args)
	{
		if(args != null && args.Length > 0)
		{
			string[] argsVal = new string[args.Length];
		
			for(int i = 0; i< args.Length; i++)
			{
				string strArg = args[i].ToString();
				
				if(strArg.Contains("IDS_"))
				{
					argsVal[i] = Localization.instance.Get(strArg);
				}else{
					argsVal[i] = strArg;	
				}
			}
			return string.Format(Localization.instance.Get(textIds),argsVal);
		}else{
			return Localization.instance.Get(textIds);
		}
	}
	
	
	private void ShowNext()
	{
		if(_list.Count>0)
		{
			Message message = _list[0];
			_list.RemoveAt(0);
			ShowMessage(message);	
		}else{
			_isShowing = false;
			_isOpen = false;
			CloseMessageBox();	
		}
	}
	
	private void ShowMessage(Message message)
	{
		if(_curMessage == null)
		{
			_curMessage = message;
			_label.text = message._text;
			_label.alpha = 1;
		}else{
			_curMessage = message;
			FadeOut();
		}
		
		_timer = 0;
		_isShowing = true;
		
		if(!_isOpen)
		{
			_isOpen = true;
			OpenMessageBox();		
		}
	}
	
	
	private void FadeOut()
	{
		TweenAlpha tween = _label.GetComponent<TweenAlpha>();
		tween.eventReceiver = gameObject;
		tween.callWhenFinished = "FadeOutOver";
		tween.from = 1;
		tween.to = 0;
		tween.Reset();
		tween.Play(true);
	}
		
	private void FadeIn()
	{
		TweenAlpha tween = _label.GetComponent<TweenAlpha>();
		tween.eventReceiver = gameObject;
		tween.callWhenFinished = "FadeInOver";
		tween.from = 0;
		tween.to = 1;
		tween.Reset();
		tween.Play(true);
	}
	
	public void FadeOutOver()
	{
		_label.text = _curMessage._text;	
		FadeIn();
	}
	
	public void FadeInOver()
	{

	}
	
	
	private void OpenMessageBox()
	{
		_all.SetActive(true);
		TweenScale tween = _all.GetComponent<TweenScale>();
		tween.eventReceiver = gameObject;
		tween.callWhenFinished = "AniamtionOver1";
		tween.from = new Vector3(1f,0.1f,1);
		tween.to = new Vector3(1,1,1);
		tween.Reset();
		tween.Play(true);
	}
	
	private void CloseMessageBox()
	{
		TweenScale tween = _all.GetComponent<TweenScale>();
		tween.eventReceiver = gameObject;
		tween.callWhenFinished = "AniamtionOver2";
		tween.from = new Vector3(1,1,1);
		tween.to = new Vector3(1,0.1f,1);
		tween.Reset();
		tween.Play(true);	
	}
	
	 void AniamtionOver1()
	{
			
	}
	
	 void AniamtionOver2()
	{
		_all.SetActive(false);
	}
	
	 void Update()
	{
		if(_isShowing)
		{
			_timer += Time.deltaTime;
			if(_timer >= _curMessage._time)
			{
				ShowNext();
			}
		}
	}
	
}

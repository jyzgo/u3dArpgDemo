using UnityEngine;
using System.Collections;

public class UITownCredits : MonoBehaviour {
	
	public UILabel _textLabel;
	public UIDraggablePanel _draggablePanel;
	public BoxCollider _textListCollider;
	public float _moveStep = 5.0f;
	public float _waitSeconds = 3.0f;
	
	bool _isPressing = false;
	Transform _textListPanelTrans;
	SpringPanel _springPanel = null;
	float _textHeight = 0.0f;
	float _boardHeight2D = .0f;
	bool _isStarting = true;
	
	void Start ()
	{
		OnInitialize(); 
	}
	
	void OnInitialize()
	{
		UITextList uiTextList = _textListCollider.GetComponent<UITextList>();
		_textLabel.lineWidth = Mathf.FloorToInt( uiTextList.maxWidth );
		
		string versionNumString = string.Format(Localization.instance.Get("IDS_SETTING_ABOUT"), AppVersion.GetCurrentVersionString());
		string versionAppString = string.Format(Localization.instance.Get("IDS_SETTING_ABOUT2"), AppVersion.GetCurrentBuildString());
		_textLabel.text = versionNumString + "\n" + Localization.instance.Get("IDS_SETTING_CREDITS") + "\n" + versionAppString + "("+ FCDownloadManager.Instance.CurrentAssetBundleTag +")";
		
		_textHeight = _textLabel.relativeSize.y * _textLabel.transform.localScale.y;
		_textListCollider.size = new Vector3(_textListCollider.size.x, _textHeight, _textListCollider.size.z);
		_textListCollider.center = new Vector3(_textListCollider.center.x, - _textHeight / 2.0f, _textListCollider.center.z);
		_springPanel = _draggablePanel.gameObject.GetComponent<SpringPanel>();
		_textListPanelTrans = _draggablePanel.gameObject.transform;
		
		_boardHeight2D = _draggablePanel.gameObject.GetComponent<UIPanel>().clipRange.w / 2.0f;
		
		_draggablePanel.ResetPosition();
		_draggablePanel.MoveRelative(new Vector3(0, -_boardHeight2D, 0));
		_isPressing = false;
		_isStarting = true;
		StartCoroutine("WaitAWhile");
	}
	
	void Update ()
	{
		if(!_isStarting && !_isPressing && !_springPanel.enabled)
		{
			_draggablePanel.MoveRelative(new Vector3(0, _moveStep * Time.deltaTime, 0));
			
			if(_textListPanelTrans.localPosition.y > _textHeight + _boardHeight2D)
			{
				_draggablePanel.MoveRelative(new Vector3(0, -(_textHeight + _boardHeight2D * 2.0f), 0));
			}
		}
	}
	
	void Close()
	{
		UIManager.Instance.CloseUI("Panel(Credits)");
	}
	
	void OnPress()
	{
		_isPressing = true;
		if(_isStarting)
		{
			_isStarting = false;
			StopCoroutine("WaitAWhile");
		}
	}
	
	void OnRelease()
	{
		_isPressing = false;
	}
	
	IEnumerator WaitAWhile()
	{
		yield return new WaitForSeconds(_waitSeconds);
		_isStarting = false;
	}
}

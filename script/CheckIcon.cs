using UnityEngine;
using System.Collections;

public class CheckIcon : MonoBehaviour {

	public GameObject  _icon;
	public GameObject _text;
	
	private UILabel _label;
	
	void Start()
	{
		_label = _text.GetComponent<UILabel>();
	}
	
	// Update is called once per frame
	void Update () {
		
		bool isFull = PlayerInfo.Instance.IsInventoryFull();
		
		if (isFull)
		{
			_label.text = Localization.instance.Get("IDS_FULL");
			_icon.SetActive(isFull);
			_text.SetActive(isFull);
		}
		else
		{
			_icon.SetActive(isFull);
			_text.SetActive(isFull);
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MB_TYPE
{
	MB_NULL = 0,
	MB_OK,
	MB_OKCANCEL,
	MB_WAITING,
	MB_INPUT,
	MB_FLOATING,
	MB_WAITING_DELAY,
	MB_OK_WITH_ITEMS,
}

public enum ID_BUTTON
{
	ID_OK = 1,
	ID_CANCEL,
}

public enum MBButtonColorScheme
{
	red_green,
	red_red,
	green_red,
	green_green
}

public class UIMessageBox : MonoBehaviour
{
	public delegate void OnClickCallback(ID_BUTTON buttonID);
	private OnClickCallback _buttonClicked;

	public delegate void OnInputCallback(ID_BUTTON buttonID, string inputText);

	public UIStretch bgStretch;

	public UILabel labelCaption;

	public UILabel labelText;

	// Use this for initialization
	void Start()
	{
		bgStretch.uiCamera = MessageBoxCamera.Instance.uiCamera;
	}

	public virtual void SetParams(string text, string caption, OnClickCallback callback, params System.Object[] args)
	{
        if (string.IsNullOrEmpty(caption))
            caption = Localization.Localize("IDS_TITLE_GLOBAL_NOTICE");
		labelCaption.text = caption;
		labelText.text = text;

		_buttonClicked = callback;
	}

	protected void Close()
	{
		UIMessageBoxManager.Instance.CloseMessageBox(gameObject);
	}

	void OnClickOK()
	{
		if (_buttonClicked != null)
		{
			_buttonClicked(ID_BUTTON.ID_OK);
		}

		Close();
	}

	void OnClickCancel()
	{
		if (_buttonClicked != null)
		{
			_buttonClicked(ID_BUTTON.ID_CANCEL);
		}

		Close();
	}

	public virtual void LocalizeButtonText(string leftStrIDS, string rightStrIDS)
	{
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMessageBoxInput : UIMessageBox
{
	public UIInput inputBox;

	private const int k_max_input_length = 12;
	
	private OnInputCallback _inputClicked;
	
	public override void SetParams(string text, string caption, OnClickCallback callback, params System.Object[] args)
	{
		base.SetParams(text, caption, callback, args);

		UIMessageBox.OnInputCallback inputCallback = args[0] as UIMessageBox.OnInputCallback;

		labelCaption.text = caption;
		labelText.text = text;
		inputBox.text = Localization.instance.Get("IDS_INPUT_HERE");
		inputBox.maxChars = k_max_input_length;
		_inputClicked = inputCallback;
	}
	
	void OnInputOK()
	{
		if (_inputClicked != null)
		{
			_inputClicked(ID_BUTTON.ID_OK, inputBox.text);
		}

		Close();
	}

	void OnInputCancel()
	{
		if (_inputClicked != null)
		{
			_inputClicked(ID_BUTTON.ID_CANCEL, inputBox.text);
		}

		Close();
	}
}

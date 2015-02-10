using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIMessageBoxOKCancel : UIMessageBox
{
	public UIImageButton leftButton;

	public UISprite leftSprite1;
	
	public UISprite leftSprite2;

	public UILabel leftButtonLabel;

	public UIImageButton rightButton;

	public UISprite rightSprite1;

	public UISprite rightSprite2;

	public UILabel rightButtonLabel;

	private string[] k_sprite_names = new string[]
	{
		"38_b",  //red-green
		"40_b",
		"38_b",  //red-red
		"38_b",
		"40_b",  //green-red
		"38_b",
		"40_b",  //green-green
		"40_b",
	};

	public override void SetParams(string text, string caption, UIMessageBox.OnClickCallback callback, params object[] args)
	{
		base.SetParams(text, caption, callback, args);

		if (args.Length > 0)
		{
			SetButtonColorScheme((MBButtonColorScheme)args[0]);
		}
		else
		{
			SetButtonColorScheme(MBButtonColorScheme.red_green);
		}
	}

	private void SetButtonColorScheme(MBButtonColorScheme colorScheme)
	{
		string leftSpriteName = k_sprite_names[(int)colorScheme * 2];

		string rightSpriteName = k_sprite_names[(int)colorScheme * 2 + 1];

		leftButton.normalSprite = leftSpriteName;
		leftSprite1.spriteName = leftSpriteName;
		leftSprite2.spriteName = leftSpriteName;

		rightButton.normalSprite = rightSpriteName;
		rightSprite1.spriteName = rightSpriteName;
		rightSprite2.spriteName = rightSpriteName;
	}

	public override void LocalizeButtonText(string leftStrIDS, string rightStrIDS)
	{
		if (!string.IsNullOrEmpty(leftStrIDS))
		{
			UILocalize localizeLeft = leftButtonLabel.gameObject.GetComponent<UILocalize>();

			localizeLeft.key = leftStrIDS;

			localizeLeft.Localize();
		}

		if (!string.IsNullOrEmpty(rightStrIDS))
		{
			UILocalize localizeRight = rightButtonLabel.gameObject.GetComponent<UILocalize>();

			localizeRight.key = rightStrIDS;

			localizeRight.Localize();
		}
	}
}

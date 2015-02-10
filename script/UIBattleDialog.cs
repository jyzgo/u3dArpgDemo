using UnityEngine;

using System;
using System.Collections.Generic;

public class UIBattleDialog : MonoBehaviour
{
	public UILabel labelSpeaker;

	public UILabel labelContent;

	public void ShowDialog(string speaker, string content)
	{
		labelSpeaker.text = Localization.instance.Get(speaker);

		labelContent.text = Localization.instance.Get(content);
	}

	public System.Collections.IEnumerator Close()
	{
		yield return new WaitForEndOfFrame();

		gameObject.SetActive(false);
	}
}

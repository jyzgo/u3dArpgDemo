using UnityEngine;

using System;
using System.Collections.Generic;

public class UIBattleDialogManager : MonoBehaviour
{
	private static UIBattleDialogManager _instance;

	public static UIBattleDialogManager Instance
	{
		get { return _instance; }
	}

	public UIBattleDialog[] dialogs;

	private Dictionary<string, EnumDialogPos> _mapping = new Dictionary<string, EnumDialogPos>();

	void Awake()
	{
		_instance = this;
	}


	void OnDestroy()
	{
		_instance = null;
	}

	public void ShowDialog(string id)
	{
		DialogData dd = DataManager.Instance.GetDialogData(id);

		if (dd != null)
		{
			_mapping[id] = dd.pos;

			int index = (int)dd.pos;

			UIBattleDialog dialog = dialogs[index];
			if (dialog != null)
			{
				if (!dialog.gameObject.activeSelf)
				{
					dialog.gameObject.SetActive(true);
				}
			}
			dialogs[index].ShowDialog(dd.speakerIDS, dd.contentIDS);
		}
	}

	public void CloseDialog(string id)
	{
		if (_mapping.ContainsKey(id))
		{
			EnumDialogPos pos = _mapping[id];
			StartCoroutine(dialogs[(int)pos].Close());
		}
	}
}

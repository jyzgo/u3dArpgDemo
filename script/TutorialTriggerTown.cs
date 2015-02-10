using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TutorialTownTriggerInfo
{
	private const float k_posZ = -20;

	public EnumTutorial tutorialID;

	public GameObject target;

	public string tipTextIDS;		//the ids to fill

	public void Activate()
	{
		TutorialMaskEffect.Instance.GotoTarget(target);
	}

	public void Close()
	{
		TutorialMaskEffect.Instance.Close();
	}
}


public class TutorialTriggerTown : MonoBehaviour
{
	public List<TutorialTownTriggerInfo> infoList;

	private bool _hasActiveTutorial;	//at least 1 tutorial is active

	private TutorialTownTriggerInfo _currentTutorialInfo;

	public void TryToStartTutorial()
	{
		foreach (TutorialTownTriggerInfo info in infoList)
		{
			EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(info.tutorialID);
			if (state != EnumTutorialState.Finished)
			{
				info.Activate();
				_currentTutorialInfo = info;
			}
			break;
		}
	}

	//since there is only one button to click, the call must come from the desired object
	public void TryToCloseTutorial()
	{
		if (_currentTutorialInfo != null)
		{
			PlayerInfo.Instance.ChangeTutorialState(_currentTutorialInfo.tutorialID, EnumTutorialState.Finished);
			
			_currentTutorialInfo.Close();

			_currentTutorialInfo = null;
		}
	}
}
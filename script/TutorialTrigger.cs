using UnityEngine;
using System.Collections;

public enum TutorialAction
{
	Start,
	Finish
}

public class TutorialTrigger : MonoBehaviour
{
	public TutorialAction _action = TutorialAction.Start;
	public EnumTutorial tutorialId;

	public void OnTriggerEnter(Collider other)
	{
		ActionController ac = other.gameObject.GetComponent<ActionController>();
		if (ac != null && ac.IsPlayerSelf)
		{
			if (TutorialManager.Instance != null)
			{
				if (_action == TutorialAction.Start)
				{
					TutorialManager.Instance.ReceiveStartTutorialEvent(tutorialId); //attack kill skill1 skill2 skill3 skill4
				}
				else
				{
					TutorialManager.Instance.ReceiveFinishTutorialEvent(tutorialId);//move
				}
			}
		}
	}
}

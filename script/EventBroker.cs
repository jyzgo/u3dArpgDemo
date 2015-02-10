using UnityEngine;
using System;
using System.Collections.Generic;

//a broker class that responses to cinematic events. Can be spawned for different triggers.
public class EventBroker : MonoBehaviour
{
	//called from CinematicTrigger
	public void OnCinematicStart()
	{
		
	}

	public void OnCinematicEnd()
	{
		TutorialManager.Instance.ReceiveStartTutorialEvent(EnumTutorial.Battle_Move);
	}
}


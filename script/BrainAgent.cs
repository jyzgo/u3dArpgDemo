using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/BrainAgent")]
public class BrainAgent : MonoBehaviour,FCAgent
{
	public virtual void Init(FCObject owner)
	{
	
	}
	public virtual AIAgent.STATE GetNextStateByRun(bool findedTarget,bool inFinding)
	{
		return AIAgent.STATE.MAX;
	}
	public virtual AIAgent.STATE GetNextStateByBorn()
	{
		return AIAgent.STATE.MAX;
	}
}

using UnityEngine;
using System.Collections;

public class PassiveSkillBase : MonoBehaviour {
	
	protected FCPassiveSkill _owner;
	public string _skillName = "";
	
	public void Init(FCPassiveSkill eps)
	{
		_owner = eps;
		OnInit();
	}
	
	protected virtual void OnInit()
	{
		
	}
}

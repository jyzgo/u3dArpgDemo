using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AttackAgent))]
public class AttackAgentEditor : Editor
{
	static public int SortByName (AttackBase a, AttackBase b) { return string.Compare(a._name , b._name); }
	static public int SortByName (FCSkill a, FCSkill b) { return string.Compare(a._skillName , b._skillName); }
	
	AttackAgent _target;
	void OnEnable(){
		_target = (AttackAgent)target;
	}
	
	public override void OnInspectorGUI()
	{
		AttackBase[] abs = _target._attacks.GetComponents<AttackBase>();
		if(_target._forceRefresh
			|| _target._attackList == null 
			|| _target._attackList.Count == 0
			|| abs.Length != _target._attackList.Count)
		{
			_target._attackList = new System.Collections.Generic.List<AttackBase>();
			foreach(AttackBase ab in abs)
			{
				_target._attackList.Add(ab);
			}
			_target._attackList.Sort(SortByName);
			foreach(AttackBase ab in _target._attackList)
			{
				Debug.Log(ab._name);
			}
			foreach(FCSkill skill in _target._skills)
			{
				Debug.Log("skill name");
				Debug.Log(skill._skillName);
				foreach(FCAttackConfig eac in skill._attackConfigs )
				{
					Debug.Log("attack name");
					Debug.Log(eac._attackModuleName);
					Debug.Log("attack conditions");
					foreach(AttackConditions ac in eac._attackConditions )
					{
						Debug.Log(ac._attCon.ToString());
						Debug.Log(ac._conVal.ToString());
						Debug.Log(ac._jumpIdx.ToString());
					}
				}
				
			}
			
		}
		if(_target._forceRefresh )
		{
			_target._forceRefresh = false;
			EditorUtility.SetDirty(_target);
		}
		DrawDefaultInspector();
		if(GUI.changed){
						
		}
		
	}
	
}

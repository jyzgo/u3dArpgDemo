using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class SkillTableChecker : ScriptableWizard{

	protected string _acDatalistPath = "Assets/GlobalManagers/Data/Characters/AcDataList.asset";
	protected string _characterTable = "Assets/GlobalManagers/Data/Characters/CharacterTable.asset";
	
	protected string _skillDataListPath = "Assets/GlobalManagers/Data/Skill/SkillDataList.asset";
	protected string _skillMapListPath = "Assets/GlobalManagers/Data/Skill/SkillMapList.asset";
	protected string _skillModuleConfigListPath = "Assets/GlobalManagers/Data/Skill/SkillModuleConfigList.asset";
	protected string _skillModuleDataListPath = "Assets/GlobalManagers/Data/Skill/SkillModuleDataList.asset";
	protected string _aiAgentsPath = "Assets/Entity/Characters";
	public void OnWizardUpdate()
    {
		
    }
	
	public static void ClearLog()
	{
	    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
	 
	    Type type = assembly.GetType("UnityEditorInternal.LogEntries");
	    MethodInfo method = type.GetMethod("Clear");
	    method.Invoke(new object(), null);
	}
	[MenuItem("Tools/Skill/Check skill table", false, 4)]
    static void ShowSkillTableInfo()
    {
		ScriptableWizard.DisplayWizard<SkillTableChecker>("Show check result", "Close!","Check!");
    }
	
	public void OnWizardCreate()
	{
		
	}
	public void OnWizardOtherButton()
	{
		/*
		AcDataList adl = AssetDatabase.LoadAssetAtPath(_acDatalistPath,typeof(AcDataList)) as AcDataList;
		CharacterTable ct = AssetDatabase.LoadAssetAtPath(_characterTable,typeof(CharacterTable)) as CharacterTable;
		AttackModuleDataList amdl = AssetDatabase.LoadAssetAtPath(_skillModuleDataListPath,typeof(AttackModuleDataList)) as AttackModuleDataList;
		SkillDataList sdl = AssetDatabase.LoadAssetAtPath(_skillDataListPath,typeof(SkillDataList)) as SkillDataList;
		AttackModuleConfigList amcl = AssetDatabase.LoadAssetAtPath(_skillModuleConfigListPath,typeof(AttackModuleConfigList)) as AttackModuleConfigList;
		int errorCount = 0;
		int enemyCountInGame = 0;
		List<string> aiAgentNames = new List<string>();
		ClearLog();
		foreach(AcData ad in adl._dataList)
		{
			string adName = ad._id;
			if(adName.Contains("town"))
			{
				continue;
			}
			
			enemyCountInGame++;
			CharacterInformation ret = null;
			foreach(CharacterInformation ci in ct._characterInfos)
			{
				if(ci.label == adName)
				{
					ret = ci;
					break;
				}
			}
			if(ret == null)
			{
				Debug.Log("\n-------------------" +adName+ "---------------------start--------------------------");
				Debug.LogError("we have no " +adName+ " in CharacterTable");
				Debug.Log("\n-------------------" +adName+ "---------------------end----------------------------");
				errorCount++;
				continue;
			}
			else
			{
				GameObject gb = AssetDatabase.LoadAssetAtPath(ret._AiAgentPath,typeof(GameObject)) as GameObject;
				if(gb == null)
				{
					Debug.Log("\n-------------------" +adName+ "---------------------start--------------------------");
					Debug.LogError("we have no AiAgent "+ret._AiAgentPath);
					Debug.Log("\n-------------------" +adName+ "---------------------end----------------------------");
					errorCount++;
					continue;
				}
				else
				{
					Debug.Log("\n-------------------"+"adName"+"---------------------"+gb.name+"------------start--------------");
					aiAgentNames.Add(gb.name);
				}
				List<string> amds = new List<string>();
				foreach(AttackModuleData amd in amdl._dataList)
				{
					if(amd._enemyId == adName)
					{
						amds.Add(amd._attackModuleId);
					}
				}
				if(amds.Count == 0)
				{
					Debug.LogError("we have no Skill info for "+adName);
					errorCount++;
				}
				if(gb != null && amds.Count != 0)
				{
					AttackBase[] abs = null;
					List<string> attackNameList = null;
					AttackAgent aa = gb.GetComponent<AttackAgent>();
					if(aa == null)
					{
						Debug.LogError(gb.name + " has no AttackAgent to restore skill info");
						errorCount++;
					}
					else
					{
						abs = aa._attacks.GetComponents<AttackBase>();
						attackNameList = new List<string>();
						for(int i =0;i<abs.Length;i++)
						{
							attackNameList.Add(abs[i]._name);
							if(abs[i]._name == "")
							{
								Debug.LogError(abs[i].name +" should not be null!");
								errorCount++;
							}
						}
					}
					if(amds.Count != attackNameList.Count || attackNameList.Count == 0)
					{
						Debug.LogError("Enemy "+ adName + " and Ai "+  gb.name + " have different attack module data !");
						errorCount++;
						foreach(string amd in amds)
						{
							if(!attackNameList.Contains(amd))
							{
								attackNameList.Add(amd);
								Debug.LogError("Ai "+gb.name+" has no attackModule named "+amd+"!");
								errorCount++;
							}
						}
						foreach(string an in attackNameList)
						{
							if(!amds.Contains(an))
							{
								Debug.LogError("Enemy "+adName+" has no attackModule named "+an+"!");
								errorCount++;
							}
						}
					}
					else
					{
						for(int i =0; i< amds.Count ;i++)
						{
							if(!attackNameList.Contains(amds[i]))
							{
								Debug.LogError("Ai "+gb.name+" has no "+ amds[i] + " , please check Excel!");
								errorCount++;
							}
						}
					}
					if(abs != null)
					{
						attackNameList.Clear();
						for(int i =0;i<abs.Length;i++)
						{
							attackNameList.Add(abs[i]._name);
						}
					}
					if(amcl != null)
					{
						foreach(AttackModuleConfig amc in amcl._dataList)
						{
							if(amc._enemyId == adName && !attackNameList.Contains(amc._attackModuleId))
							{
								Debug.LogError("Ai "+gb.name+" has no "+ amc._attackModuleId + " for skill "+ amc._skillId
									+ " of Enemy "+ adName+"! Please check Excel! SkillModuleConfigList");
								errorCount++;
							}
						}
					}
					if(sdl != null)
					{
						foreach(SkillData sd in sdl._dataList)
						{
							if(adName == sd._enemyId)
							{
								if(sd._comboHitMax != sd._attackModuleConfigIdsList.Count)
								{
									Debug.LogError("Enemy "+ adName+"'s skill: "+ sd._skillId + "'s combo count is wrong!" );
									errorCount++;
								}
								foreach(string an in sd._attackModuleConfigIdsList)
								{
									if(!attackNameList.Contains(an))
									{
										Debug.LogError("Ai "+gb.name+" has no "+ an + " for skill "+ sd._skillId
											+ " of Enemy "+ adName+"! Please check Excel! SkillDataList");
										errorCount++;
									}
								}
							}

						}
					}
					
				}
				Debug.Log("-------------------"+"adName"+"---------------------"+gb.name+"------------end--------------\n");
			}
		}
		if(aiAgentNames.Count != 0)
		{
			string[] gbs = null;
			List<string> aiagentObjNames = new List<string>();
			if(Directory.Exists(_aiAgentsPath))
			{
				gbs = Directory.GetFiles(_aiAgentsPath);
			}
			foreach(string gbn in gbs)
			{
				if(gbn.Contains(".prefab") 
					&& !gbn.Contains("town") 
					&& !gbn.Contains("Town") 
					&& !gbn.Contains(".meta")
					&& !gbn.Contains("selection"))
				{
					GameObject gb = AssetDatabase.LoadAssetAtPath(gbn,typeof(GameObject)) as GameObject;
					aiagentObjNames.Add(gb.name);	
				}
			}
			foreach(string gb in aiagentObjNames)
			{
				if(!aiAgentNames.Contains(gb))
				{
					Debug.LogWarning("No enemy use " + gb + " as its aiAgent!");
				}
			}
			gbs = null;
			aiagentObjNames = null;
		}
		if(errorCount == 0)
		{
			Debug.LogWarning("Check " + enemyCountInGame+ " enemy for no error!");
		}
		else
		{
			Debug.LogWarning("Check " + enemyCountInGame+ " enemy for " + errorCount + " error!");
		}
		adl = null;
		ct = null;
		amdl = null;
		amcl = null;
		sdl = null;
		*/
	}
	
}

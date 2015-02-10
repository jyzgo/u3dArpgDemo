using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AnimationControllerImport : AssetPostprocessor {
	
	static List<string> aniInfoPath = new List<string>();
	
	public class JumpInfo
	{
		public string _targetID = "";
		public int _jumpCondition = -1;
	}
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                                               string[] movedAssets, string[] movedFromPath)
    {
		aniInfoPath.Clear();
        if (CheckEventModified(importedAssets)|| CheckEventModified(movedAssets))
        {
			string[] ap = aniInfoPath.ToArray();
			
            foreach (string child in ap)
            {
				//FCAnimationInfo sd = AssetDatabase.LoadAssetAtPath("assets/data/camera/newSkillCameraList.asset", typeof(SkillCameraList)) as SkillCameraList;
				string elem = child;
				elem = elem.Replace(".controller",".asset");
				string path = elem;
				FCAnimationInfo ei= AssetDatabase.LoadAssetAtPath(elem, typeof(FCAnimationInfo)) as FCAnimationInfo;
				FCAnimationInfo eim = null;
				//AssetDatabase.DeleteAsset(elem);
				//ei = null;
				if(ei == null)
				{
					ei = ScriptableObject.CreateInstance<FCAnimationInfo>();
					//need not create new folder now
					/*string[] folders = elem.Split('/');	
					string[] parentfolders = elem.Split('/');
					for(int i = 1;i< folders.Length-2;i++)
					{
						parentfolders[i] = parentfolders[i-1]+"/"+folders[i];
					}
					for(int i = 0;i< parentfolders.Length-2;i++)
					{
						if(!Directory.Exists(parentfolders[i]+"/"+folders[i+1]))
						{
							AssetDatabase.CreateFolder(parentfolders[i],folders[i+1]);
						}
					}*/
					path = AssetDatabase.GenerateUniqueAssetPath(elem);
					
					//ei= AssetDatabase.LoadAssetAtPath(path, typeof(FCAnimationInfo)) as FCAnimationInfo;
				}
				else
				{
					eim = ei;
					ei = ScriptableObject.CreateInstance<FCAnimationInfo>();
				}
                Debug.Log("Model event changed = "+elem);
				bool flag = false;
				ei._animationInfo = new List<FCAnimationInfoDetails>();
				List<JumpInfo> jumpInfos = new List<JumpInfo>();
				
				using( System.IO.StreamReader reader = new System.IO.StreamReader(child)) 
				{
					string line = reader.ReadLine();
					while(line != null)
					{
						if(!flag && line == "State:")
						{
							flag = true;
						}
						if(flag)
						{
							if(line == "StateMachine:")
							{
								break;
							}
							string uid = "";
							if(line.Contains("--- !u!"))
							{
								string[]ass = line.Split('&');
								uid = ass[1];
							}
							if(line.Contains("m_SrcState: {fileID: 0}"))
							{
								line = reader.ReadLine();
								if(line.Contains("m_DstState"))
								{
									line = line.Replace("  m_DstState: {fileID: ","");
									line = line.Replace("}","");
									if(line != "0")
									{
										JumpInfo ji = new JumpInfo();
										ji._targetID = line;
										line = reader.ReadLine();
										while(!line.Contains("m_ConditionEvent"))
										{
											line = reader.ReadLine();
										}
										if(line.Contains("state"))
										{
											line = reader.ReadLine();
											if(line.Contains("m_EventTreshold"))
											{
												line = line.Replace("    m_EventTreshold: ","");
											}
											if(int.TryParse(line, out ji._jumpCondition))
											{
												jumpInfos.Add(ji);
											}
										}
										else
										{
											ji = null;
										}
									}
								}
							}
							if(line.Contains("  m_Name: ") && line.Length > 11)
							{
								FCAnimationInfoDetails eds = new FCAnimationInfoDetails();
								string nameS = line.Remove(0,10);
								nameS = "Base." + nameS;
								eds._animationName = nameS;
								eds._nameHashCode = Animator.StringToHash(nameS);
								if(uid != "")
								{
									eds._uidString = uid;
								}
								line = reader.ReadLine();
								if(line.Contains("m_Speed"))
								{
									string speed = line.Remove(0,11);
									eds._speed = float.Parse(speed);
								}
								ei._animationInfo.Add(eds);
							}
						}
						line = reader.ReadLine();
					}
					
				}
				ei.RefreshList();
				//true means need not refresh asset
				bool ret = true;
				if(eim == null)
				{
					ret = false;
				}
				else if(eim != null && ei._animationInfo.Count == eim._animationInfo.Count)
				{
					for(int i =0; i< ei._animationInfo.Count; i++)
					{
						if(ei._animationInfo[i]._speed != eim._animationInfo[i]._speed
							|| ei._animationInfo[i]._nameHashCode != eim._animationInfo[i]._nameHashCode)
						{
							ret = false;
							break;
						}
					}
				}
				else if(eim != null && ei._animationInfo.Count != eim._animationInfo.Count)
				{
					ret = false;
				}
				if(!ret)
				{
					//AssetDatabase.DeleteAsset(elem);
					AssetDatabase.CreateAsset(ei,path);
					ei = AssetDatabase.LoadAssetAtPath(path, typeof(FCAnimationInfo)) as FCAnimationInfo;
					elem = elem.Replace(".asset",".prefab");
					GameObject goOld = AssetDatabase.LoadAssetAtPath(elem,typeof(GameObject)) as GameObject;
					GameObject goNew = PrefabUtility.InstantiatePrefab(goOld) as GameObject;
					if(goNew != null)
					{
						AvatarController avc = goNew.GetComponent<AvatarController>();
						if(avc != null)
						{
							avc._animationInfos = ei;
							PrefabUtility.ReplacePrefab(goNew,goOld);
							PrefabUtility.DisconnectPrefabInstance(goNew);
							GameObject.DestroyImmediate(goNew);
							//AssetDatabase.CreateAsset(elem,goOld);
						}
					}
				}
				else
				{
					eim = null;
					ei = null;
				}

            }
			ap = null;
        }
		
    }
	
	private static bool CheckEventModified(string[] files)
    {

        bool resModified = false;
        
        foreach (string file in files)
        {
            if (file.Contains(".controller"))
            {
                aniInfoPath.Add(file);
                resModified = true;
            }
        }

        return resModified;
    }
}

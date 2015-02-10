using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class SfxChecker : ScriptableWizard
{
	private string SoundClipPath = "Assets/Resources/Audio/sfx/";
	private List<string> _soundNames;
	private string SoundClipAsset = "Assets/GlobalManagers/Data/Sound/SoundClipList.asset";
	private List<SoundClip> _deadClip = new List<SoundClip>();
	[MenuItem("Tools/SFX/Check sfx table", false, 4)]
    static void ShowSfxTableInfo()
    {
		ScriptableWizard.DisplayWizard<SfxChecker>("Show check result", "Close!","Check!");
    }
	
	public void OnWizardCreate()
	{
		
	}
	
	public void OnWizardOtherButton()
	{
		SoundClipList scl= AssetDatabase.LoadAssetAtPath(SoundClipAsset, typeof(SoundClipList)) as SoundClipList;
		_soundNames = new List<string>();
		HandleWavDirectory(SoundClipPath);
		bool hasError = false;
		
		foreach(SoundClip sc in scl._soundList)
		{
			bool ret = false;
			string scn = "";
			if(sc.Clip == null)
			{
				scn = sc._name;
				hasError = true;
			}
			else
			{
				scn = sc.Clip.ToString().Replace(" (UnityEngine.AudioClip)","");
			}
			foreach(string sn in _soundNames)
			{
				if(sn.Contains(scn+".wav"))
				{
					ret = true;
				}
			}
			if(!ret)
			{
				Debug.LogError("sc have no source clip : " + scn);
				_deadClip.Add(sc);
				hasError = true;
			}
			string AssetName = sc._clipPath;
			AssetName = "Assets/Resources/" + sc._clipPath + ".wav";
			string[] nameAll = sc._clipPath.Split('/');
			if(nameAll[nameAll.Length-1] != sc._name)
			{
				hasError = true;
				Debug.LogError("id name != clip name");
				Debug.Log(sc._name);
				if(sc.Clip != null)
				{
					Debug.Log(sc.Clip.ToString());
				}
				string path = AssetDatabase.RenameAsset(AssetName, sc._name);
				Debug.LogError(path);
			}
			
			AssetName = AssetName.Replace(nameAll[nameAll.Length-1],sc._name);
			AssetName = AssetName.Replace("Assets/Resources/","");
			AssetName = AssetName.Replace(".wav","");
			sc._clipPath = AssetName;
		}
		//remove no use file
		if(!hasError)
		{
			List<SoundClip> sclNew = new List<SoundClip>();
			foreach(string sn in _soundNames)
			{
				bool ret = false;
				foreach(SoundClip sc in scl._soundList)
				{
					if(sn.Contains(sc._name+".wav"))
					{
						ret = true;
					}
				}
				if(!ret)
				{
					Debug.LogError(sn + " is not used for sound clip list!");
					Debug.LogError("we will add it to list");
					SoundClip scnew = new SoundClip();
					string AssetName = sn;
					AssetName = AssetName.Replace("Assets/Resources/","");
					AssetName = AssetName.Replace(".wav","");
					
					scnew._clipPath = AssetName;
					string[] nameAll = AssetName.Split('/');
					scnew._name = nameAll[nameAll.Length-1];
					sclNew.Add(scnew);
					/*System.IO.FileInfo fileSound = new System.IO.FileInfo(sn);
					if(fileSound.Exists)
					{
						fileSound.Delete();
					}
					string snMeta = sn + ".meta";
					fileSound = new System.IO.FileInfo(snMeta);
					if(fileSound.Exists)
					{
						fileSound.Delete();
					}*/
				}
			}
			
			foreach(SoundClip sc in scl._soundList)
			{
				sclNew.Add(sc);
				sclNew.Sort(SortByName);
			}
			SoundClipList newScl = ScriptableObject.CreateInstance<SoundClipList>();
			newScl._soundList = sclNew.ToArray();
			scl = null;
			AssetDatabase.CreateAsset(newScl,SoundClipAsset);
			
		}
		else
		{
			List<SoundClip> sclNew = new List<SoundClip>();
			foreach(SoundClip sc in scl._soundList)
			{
				if(_deadClip.Count >0 && _deadClip.Contains(sc))
				{
					continue;
				}
				sclNew.Add(sc);
				sclNew.Sort(SortByName);
			}
			SoundClipList newScl = ScriptableObject.CreateInstance<SoundClipList>();
			newScl._soundList = sclNew.ToArray();
			scl = null;
			_deadClip.Clear();
			_deadClip = null;
			AssetDatabase.CreateAsset(newScl,SoundClipAsset);
		}
		Debug.Log(_soundNames.Count);
		_soundNames.Clear();
		_soundNames = null;
	}
	
	static public int SortByName (SoundClip a, SoundClip b) { return string.Compare(a._name , b._name); }
	private void HandleWavFile(string[] filesArray)
	{
		foreach(string fi in filesArray)
		{
			_soundNames.Add(fi);
		}
	}
	
	private void HandleWavDirectory(string[] dirsArray)
	{
		foreach(string di in dirsArray)
		{
			string dif = di + "/";
			HandleWavDirectory(dif);
		}
	}
	
	private void HandleWavDirectory(string di)
	{
		string[] files = System.IO.Directory.GetFiles(di,"*.wav");
		HandleWavFile(files);
		files = null;
		string[] dirs = System.IO.Directory.GetDirectories(di);
		HandleWavDirectory(dirs);
	}
}
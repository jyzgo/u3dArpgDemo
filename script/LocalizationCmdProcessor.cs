using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Linq;


public class LocalizationCmdProcessor
{
	private static LocalizationCmdProcessor _instance;
	int[] _filter = new int[]{10, 160};
	
	private LocalizationCmdProcessor()
	{
	}
	
	public static LocalizationCmdProcessor GetInstance()
	{
		if (_instance == null)
		{
			_instance = new LocalizationCmdProcessor();
		}
		
		return _instance;
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	
	private Process _process;
	
	public void Initialize()
	{
		_process = new Process();
		_process.StartInfo.RedirectStandardOutput = true;
		_process.StartInfo.RedirectStandardError = true;
		_process.StartInfo.UseShellExecute = false;
	}
	
	public void Clean()
	{
		_process.Close();
	}
	
	public void ExecuteCmd(string cmdName, string args)
	{
		_process.StartInfo.FileName = cmdName;
		_process.StartInfo.Arguments = args;
		
		_process.Start();
		
		string error = _process.StandardOutput.ReadToEnd();
		
		_process.WaitForExit();
		
		DisplayErrorMessage(error);
		UnityEngine.Debug.Log(error);
	}
	
	public void DisplayErrorMessage(string msg)
	{
		string str = "";
		string pattern = "Load Glyph Fail:";
		
		int begin = msg.IndexOf("Processing font:");
		int end = msg.IndexOf('\n');
		
		if (begin == -1)
		{
			return;
		}
		
		string fileinfo = msg.Substring(begin, end-begin+1);
		
		while ((begin = msg.IndexOf(pattern, begin)) != -1)
		{
			begin += pattern.Length;
			end = msg.IndexOf('\n', begin);
			int charcode = System.Convert.ToInt32(msg.Substring(begin, end-begin));
			
			if (!_filter.Contains(charcode))
			{
				string sub = new string((char)charcode, 1);
				str += sub;
				str += "	";
			}			
			
			begin = end;
		}
		
		if (str != string.Empty)
		{
			UnityEditor.EditorUtility.DisplayDialog("Character not Exist", fileinfo+str, "OK");
		}
	}
}

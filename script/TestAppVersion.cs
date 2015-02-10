using UnityEngine;
using System.Collections;

public class TestAppVersion : MonoBehaviour 
{
	
	private string m_labelMessage;					// debug log
	private int m_logStringsCount;					// debug log count
	// Use this for initialization
	void Start () {
		if (Debug.isDebugBuild)
		{
			transform.FindChild("Live").gameObject.SetActive(false);
		}
		else
		{
			transform.FindChild("Development").gameObject.SetActive(false);
		}
	}
	
	void OnGUI()
	{
		// show log
		GUI.enabled = true;
		GUI.Label(new Rect(10, 300, 450, 100), m_labelMessage);
		
		if(GUI.Button(new Rect(10, 500, 450, 60), "CurrentVersionString"))
		{
			string strCurrentVersionString = AppVersion.GetCurrentVersionString();
			
			PostLogMessage("CurrentVersionString... " + strCurrentVersionString);
		}
		
		if(GUI.Button(new Rect(10, 590, 450, 60), "CurrentBuildString"))
		{
			string strCurrentBuildString = AppVersion.GetCurrentBuildString();
			
			PostLogMessage("CurrentBuildString... " + strCurrentBuildString);
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
	
	// log message for debug
	private void PostLogMessage(string msg)
	{
		// trim old messages
		m_logStringsCount++;
		if(m_logStringsCount > 4)
		{
			m_labelMessage = m_labelMessage.Substring(m_labelMessage.IndexOf("\n") + 1);
		}
		
		// show msg
		m_labelMessage += msg + "\n";
	}
}

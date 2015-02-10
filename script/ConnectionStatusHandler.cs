using UnityEngine;
using System.Collections;

public enum SERVER_STATUS_FLAGS
{
	LOADING = 0,
	SAVING,
	LOGIN,
	AUTO_RECONNECT,
	NONE,
};




//this handler is to settle the problem of disconnect problem in download/upload
public class ConnectionStatusHandler : MonoBehaviour {
	
	public GameObject _loadingRoot = null;
	public Transform _loadingJuhuaTrans = null;
	public UILabel _loadingLabel = null;
	
	private int _connectionStatus = 0;
	private float _loadingJuhuaTick = 0.0f;
	
	
	private string[] _statusString = {
		"IDS_SERVER_STATUS_LOADING",
		"IDS_SERVER_STATUS_SAVING",
		"IDS_SERVER_STATUS_LOGIN"
	};
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (_connectionStatus == 0)
			_loadingRoot.SetActive(false);
		else
		{
			_loadingRoot.SetActive(true);
		
			
			//rotate juhua?
			_loadingJuhuaTick += Time.deltaTime;
			if (_loadingJuhuaTick > FCConst.k_juhua_speed)
			{
				_loadingJuhuaTick = 0;
				_loadingJuhuaTrans.Rotate(Vector3.forward, -45);
			}
		}
		
	
	}
	
	//set true/false for a flag
	public void SetStatusActive(SERVER_STATUS_FLAGS status, bool active)
	{
        if (active)
        {
            _connectionStatus |= (1 << (int)status);
            _loadingLabel.text = Localization.instance.Get(_statusString[(int)status]);
        }
        else
        {
            _connectionStatus &= ((1 << (int)status) ^ 0xffff);
        }
	}
}

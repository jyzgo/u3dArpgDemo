using UnityEngine;
using System.Collections;

public class TimeScaleListener : MonoBehaviour {

	private ActionController _owner = null;
	
	public ActionController Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

    private Animator _animator;
	private bool _scaleIsActive;
    // Use this for initialization
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
   	void Update()
    {
		if(GameManager.Instance.IsPVPMode)
		{
			return;
		}
        if (_animator != null && _owner != null)
        {
			if(_owner.IsPlayerSelf && !GameManager.Instance.GamePaused)
			{
				 float timeScaleValue = _animator.GetFloat("timescale");
				//Debug.Log("timescale = "+timeScaleValue);
				if(!_scaleIsActive)
				{
					_scaleIsActive = (timeScaleValue >=0.5f);
					if (_scaleIsActive)
		            {
						//_owner.ACSlowDownAnimation
						//Time.timeScale = 0.1f;
						Time.timeScale = Mathf.Clamp((timeScaleValue-0.5f),0.1f,2f);
						Time.fixedDeltaTime = 0.03333334f * Time.timeScale;
		            }
				}
				else
				{
					
					if(timeScaleValue <0.5f)
					{
                        Time.timeScale = GameSettings.Instance.TimeScale;
						Time.fixedDeltaTime = 0.03333334f * Time.timeScale;
						_scaleIsActive = false;
					}
					else
					{
						Time.timeScale = Mathf.Clamp((timeScaleValue-0.5f),0.1f,2f);
						Time.fixedDeltaTime = 0.03333334f * Time.timeScale;
					}
				}

			}

        }
    }
	
	public void SlowDownTimeScale(float timeLast,float timeScale)
	{
		if(GameManager.Instance.IsPVPMode)
		{
			return;
		}
		StartCoroutine(STATE(timeLast, timeScale));
	}
	
	private void OnDestroy()
	{
		if(GameManager.Instance != null
				&& !GameManager.Instance.GamePaused)
		{
            Time.timeScale = GameSettings.Instance.TimeScale;
		}
	}
	IEnumerator STATE(float timeLast,float timeScale)
	{
		Time.timeScale = timeScale;
		float timeCount = Time.realtimeSinceStartup;
		bool inState = true;
		while(inState)
		{
			if(GameManager.Instance != null
				&& !GameManager.Instance.GamePaused)
			{
				if(timeLast >= Time.realtimeSinceStartup-timeCount)
				{
					Time.timeScale = timeScale;
					Time.fixedDeltaTime = 0.03333334f * Time.timeScale;
				}
				else
				{
					inState = false;
				}
			}
			yield return null;
		}
        Time.timeScale = GameSettings.Instance.TimeScale;
		Time.fixedDeltaTime = 0.03333334f * Time.timeScale;
	}
}

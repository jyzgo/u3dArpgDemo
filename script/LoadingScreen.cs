using UnityEngine;
using System.Collections;


public delegate void OnProgressCallback(float step);

public class LoadingScreen : MonoBehaviour 
{
	public UISlider _loadingProgress;
	
	AsyncOperation _asynOperation;
	bool _loading = false;
	
	float _fadeTime = 0.0f;
	
	void Awake() 
	{
	}
		
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!_loading)
		{
			return;
		}
		
		if (_asynOperation != null)
		{
			_loadingProgress.sliderValue = _asynOperation.progress*0.9f;
			
			if (_asynOperation.isDone)
			{
				if (UIManager.Instance != null)
				{
					Camera camera = UIManager.Instance._UICamera.GetComponent<Camera>();
					camera.enabled = false;
				}
				
				_fadeTime += Time.deltaTime;
				if (_fadeTime > 3.0f)
				{
					EndProgress();
					
					Camera camera = UIManager.Instance._UICamera.GetComponent<Camera>();
					camera.enabled = true;
				}
			}
		}
		else
		{
			StepProgress(0.001f);
		}
	}
	
	public void StartProgress(AsyncOperation asynOp)
	{
		StartProgress();
		
		UIManager.Instance._UICamera.SetActive(false);
		
		_asynOperation = asynOp;
	}
	
	public void StartProgress()
	{
		_loading = true;
		_loadingProgress.sliderValue = 0.0f;
	}
	
	public void SetProgress(float progress)
	{
		if (progress > _loadingProgress.sliderValue)
		{
			_loadingProgress.sliderValue = progress;
		}
	}
	
	public void StepProgress(float step)
	{
		if (_loadingProgress.sliderValue <= 0.95f)
		{
			_loadingProgress.sliderValue += step;
		}
	}
	
	public void EndProgress()
	{
		_loading = false;
		_loadingProgress.sliderValue = 1.0f;
		
		LoadingScreenBootstrap.Instance.Clean();
	}
}

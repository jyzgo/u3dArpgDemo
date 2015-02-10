using System.Collections;
using UnityEngine;

public class VideoPlayerManager : MonoBehaviour
{
    public string _filename = "";
    public Color _backgroundColor = Color.black;

    public float _durationToDetectSkip;

    private float _skippedPercentage = 0.0f;

    public float SkippedPercentage
    {
        get { return _skippedPercentage; }
    }

    void OnDestroy()
    {
        if (AppManager.instance != null)
        {
            AppManager.instance.Started -= AppManagerStarted;
        }
    }

    void Start()
    {
        AppManager.instance.Started += AppManagerStarted;
    }

#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
	private void AppManagerStarted()
	{
		//StartCoroutine(DoPlayVideo());
	}

    private IEnumerator DoPlayVideo()
    {
        float startTime = Time.realtimeSinceStartup;

        Handheld.PlayFullScreenMovie(_filename, _backgroundColor, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);

        yield return null;

        float duration = Time.realtimeSinceStartup - startTime;

        if (_durationToDetectSkip > 0.0f)
            _skippedPercentage = Mathf.Clamp01(duration / _durationToDetectSkip);
    }
#else
    private void AppManagerStarted()
    {
    }
#endif
}

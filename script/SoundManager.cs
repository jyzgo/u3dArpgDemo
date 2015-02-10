//#define test_music
using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public MusicGroup[] backgroundMusicGroups;

    public SoundClipList _soundClipList;

    private Dictionary<string, SoundClip> _dictionarySoundClip;
    public static bool _enableBGM = true;
    public static bool _enableSFX = true;
    public float _sfxVolume = 1.0f;
    public float _bgmVolume = 1.0f;
    private AudioSource _bgmSource;
    private AudioSource[] _sfxChannel;


    private Transform _cameraTransform = null;

    private int _sfxChannelsNumber = 20;
    private static SoundManager _instance = null;

    private string _lastMusicName = null;

    public static SoundManager Instance { get { return _instance; } }

    [Serializable]
    public class MusicGroup
    {
        public string groupName;

        public string defaultMusic;  //this music is always accessible and resides in folder "StreamingAssets"

        public List<string> musicNameList;

        public string OnGoingMusicName { get; set; }
    }

    public bool EnableSFX
    {
        get
        {
            _enableSFX = (NGUITools.soundVolume >= 1f) ? true : false;
            return _enableSFX;
        }
        set
        {
            _enableSFX = value;

            NGUITools.soundVolume = _enableSFX ? 1f : 0f;
        }
    }
	
	bool mLoaded = false;
    public bool EnableBGM
    {
        get 
		{
			if (!mLoaded)
			{
				mLoaded = true;
				int bgm = PlayerPrefs.GetInt("BGM", 1);
				_enableBGM = (bgm==1) ? true: false;
			}
			return _enableBGM; 
		}
        set
        {
			// don't save when close by Tapjoy
			if (_enableBGM != value)
			{
				mLoaded = true;
				
				int bgm = value ? 1 : 0;
				PlayerPrefs.SetInt("BGM", bgm);
			}
			
            if (_enableBGM && !value)
            {
                if (_bgmSource != null)
                {
                    if (_bgmSource.isPlaying)
                    {
                        _bgmSource.Stop();
                    }
                }
            }
            else if (!_enableBGM && value)
            {
                if (_bgmSource != null)
                {
                    PlayBGMWithFade(_lastMusicName, 0.1f);
                }
            }

            _enableBGM = value;
        }
    }

    public void ClearSoundClipResource()
    {
        foreach (string name in _dictionarySoundClip.Keys)
        {
            SoundClip soundClip = _dictionarySoundClip[name];

            if (soundClip.HasLoaded)
            {
                Resources.UnloadAsset(soundClip.Clip);
                soundClip.ClearClip();
            }
        }

        for (int i = 0; i < _sfxChannelsNumber; i++)
        {
            _sfxChannel[i].clip = null;
        }
    }

    void Awake()
    {
        if (_instance == null || !_instance)
        {
            _instance = this;
        }
        DontDestroyOnLoad(transform.gameObject);

        _bgmSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
    }

    void Start()
    {

        _sfxChannel = new AudioSource[_sfxChannelsNumber];
        for (int i = 0; i < _sfxChannelsNumber; i++)
        {
            _sfxChannel[i] = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            _sfxChannel[i].volume = _sfxVolume;
            _sfxChannel[i].priority = 128;
        }


        _bgmSource.loop = false;
        _bgmSource.volume = _bgmVolume;
        _bgmSource.playOnAwake = false;
        _bgmSource.priority = 100;

        BuildDictionary();
    }


    void OnDestroy()
    {
        ClearSoundClipResource();
        _instance = null;
    }

    private void BuildDictionary()
    {
        _dictionarySoundClip = new Dictionary<string, SoundClip>();
        foreach (SoundClip soundClip in _soundClipList._soundList)
        {
            if (soundClip._name == null)
            {
                Debug.LogError("sound clip list");
            }
            else
            {
                try
                {
                    _dictionarySoundClip.Add(soundClip._name.Trim(), soundClip);
                }
                catch
                {
                    Debug.LogError("Duplicated sound name: " + soundClip._name);
                }
            }
        }
    }

    private string GetRandomMusic(string groupName)
    {
        //this is a group name
        MusicGroup group = null;
        foreach (MusicGroup mg in backgroundMusicGroups)
        {
            if (mg.groupName == groupName)
            {
                group = mg;
                break;
            }
        }

        if (group == null)
        {
            Debug.LogError("Wrong group name: " + groupName);
            return null;
        }

        List<string> availableList = new List<string>();

        List<string> list2 = new List<string>();

        foreach (string music in group.musicNameList)
        {
            if (HasLocalCopy(music))
            {
                availableList.Add(music);
            }
            else
            {
                list2.Add(music);
            }
        }

        if (list2.Count > 0)
        {
            StartCoroutine(DownloadMusic(list2[0]));
        }

        if (availableList.Count > 0)
        {
            return availableList[UnityEngine.Random.Range(0, availableList.Count)];
        }
        else
        {
            return group.defaultMusic;
        }
    }

    //play specified music name
    public void PlayBGM(string clipName, float fadeTime)
    {
        Debug.Log("PlayBGM: " + clipName);

        if (!clipName.EndsWith(".mp3"))
        {
            clipName = GetRandomMusic(clipName);
        }

        _lastMusicName = clipName;

        if (clipName == null)
        {
            Debug.LogError("PlayBGM name is null");
            return;
        }

        if (EnableBGM)
        {
            PlayBGMWithFade(clipName, fadeTime);
        }
        else
        {
            _bgmSource.clip = null;
            _nextMusicName = null;
            _isLoadingBGMusic = false;
        }
    }

    public void StopBGM(float fadeTime)
    {
        if (EnableBGM)
        {
            StartCoroutine(StopBGMWithFade(fadeTime));
        }
    }

    private IEnumerator StopBGMWithFade(float fadeTime)
    {
        if (_bgmSource.isPlaying)
        {
            float time = fadeTime;
            while (time > 0)
            {
                _bgmSource.volume = (time / fadeTime) * _bgmVolume;
                time -= Time.deltaTime;

                yield return null;
            }

            _bgmSource.Stop();
        }
		_playingMusicName = null;
    }


    private string _nextMusicName;
    private string _playingMusicName = null;
    private bool _isLoadingBGMusic;

    private MusicGroup _currentDownloadingGroup;

    private string _downloadingMusicName;  //the music that is being downloaded
	
	private float _downloadProgress;

    private void PlayBGMWithFade(string musicName, float fadeTime)
    {
        if (_playingMusicName == musicName) //already playing
        {
            return;
        }

        //find the group
        MusicGroup group = null;

        foreach (MusicGroup mg in backgroundMusicGroups)
        {
            if (mg.musicNameList.IndexOf(musicName) >= 0 || mg.defaultMusic == musicName)
            {
                group = mg;
                break;
            }
        }

        if (group == null)
        {
            Debug.LogWarning("Music name not found in any group: " + musicName);
            return;
        }

        if (!HasLocalCopy(musicName))
        {
			Debug.LogWarning(musicName + " not downloaded yet. Will play a substitute: " + group.defaultMusic);

            StartCoroutine(DownloadMusic(musicName));

			//find a substitute
            musicName = GetRandomMusic(group.groupName);

            if (musicName == _playingMusicName)
            {
                return;
            }
        }
		else
		{
			Debug.Log("Already downloaded. Just play it.");
		}

        //now the music is definitely on local, but could be under different folders

        _nextMusicName = musicName;

        if (_isLoadingBGMusic)
        {
            return;
        }

		_fadeTime = fadeTime;
        if (_bgmSource.isPlaying)
        {
            //fade out current
            _fadeMode = FadeMode.FadeOut;
            _fadeTimer = 0;
        }
        else
        {
            StartCoroutine(StreamPlay(musicName));
        }
    }

    private IEnumerator StreamPlay(string musicName)
    {
#if UNITY_IPHONE

        string url;

        if (File.Exists(Path.Combine(Application.streamingAssetsPath, musicName)))
        {
            url = "file://" + Application.streamingAssetsPath + '/' + musicName;
			
			Debug.Log(musicName + " found in streaming folder. url = " + url);
        }
        else
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                url = "file://" + Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/Temp/" + musicName;
            }
            else
            {
                url = "file://" + Application.persistentDataPath + '/' + musicName;
            }

			Debug.Log(musicName + " found in persistent data path folder. url = " + url);
        }

        _playingMusicName = null;

        _isLoadingBGMusic = true;

        Debug.Log("Start loading " + musicName);

        using (WWW www = new WWW(url))
        {
            while (!www.isDone && string.IsNullOrEmpty(www.error))
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                _isLoadingBGMusic = false;

                Debug.LogError("Error loading music " + url);

                yield break;
            }

            _bgmSource.clip = www.GetAudioClip(false, true, AudioType.MPEG);

            _isLoadingBGMusic = false;

            if (_nextMusicName == musicName && EnableBGM) //make sure the loaded one is still the required one
            {
                _bgmSource.volume = 0;
                _bgmSource.Play();
                _playingMusicName = musicName;

                //fade in
                _fadeMode = FadeMode.FadeIn;
                _fadeTimer = 0;

                Debug.Log("Playing music " + _playingMusicName);
            }
        }
#else
        yield break;
#endif
    }
	
	
	
	
	
	
	
    public void StopSoundEffect(string name)
    {
        SoundClip soundClip = _dictionarySoundClip[name];
        if (soundClip == null)
        {
            Debug.LogError("StopSoundEffect clip is null");
            return;
        }

        StopSoundEffect(soundClip);
    }

    public void StopAllSoundEffect()
    {
        foreach (AudioSource audio in _sfxChannel)
        {
            audio.Stop();
        }
    }

    
    public void StopSoundEffect(AudioSource sound)
    {
		if(sound == null)
		{
			return;
		}
		
        foreach (AudioSource audio in _sfxChannel)
        {
            if (audio == sound)
            {
                audio.Stop();
            }
        }
    }

    public void StopSoundEffect(AudioSource sound, float timeDelay)
    {
		if(sound == null)
		{
			return;
		}
			
        if (timeDelay <= 0)
        {
            sound.Stop();
        }
        else
        {
            StartCoroutine(DelayStopSound(sound, timeDelay));
        }
    }

    private IEnumerator DelayStopSound(AudioSource sound, float timeDelay)
    {
        float timeCount = timeDelay;
        while (timeCount > 0)
        {
            timeCount -= Time.deltaTime;
            yield return null;
        }
        sound.Stop();
    }

    private void StopSoundEffect(SoundClip audioToStop)
    {
        for (int channelIndex = 0; channelIndex < _sfxChannelsNumber; channelIndex++)
        {
            if (_sfxChannel[channelIndex].clip == audioToStop.Clip)
            {
                _sfxChannel[channelIndex].Stop();
            }
        }
    }
	
	
	
	private List<AudioSource>  pauseLoopSoundList = new List<AudioSource>();
	
	
	public void Pause()
	{
		pauseLoopSoundList.Clear();
	 	for (int channelIndex = 0; channelIndex < _sfxChannelsNumber; channelIndex++)
        {
            if (_sfxChannel[channelIndex].clip != null)
            {
				if(_sfxChannel[channelIndex].loop 
					&& _sfxChannel[channelIndex].isPlaying)
				{
					pauseLoopSoundList.Add(_sfxChannel[channelIndex]);
				}
                _sfxChannel[channelIndex].Stop();
            }
        }
	}
	
	
	public void Resume()
	{
		for (int channelIndex = 0; channelIndex < pauseLoopSoundList.Count; channelIndex++)
        {
           pauseLoopSoundList[channelIndex].Play();
        }
	}
	
	
	public void PlayOpenSoundEffect()
	{
		 AudioSource  audioSource = SoundManager.Instance.PlaySoundEffect("item_drop");
		 if(audioSource != null)
		 {
			audioSource.ignoreListenerPause = true;
		 }	
	}

    public AudioSource PlaySoundEffect(string name, Vector3 pos)
    {
        float volume = 1.0f;

        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }


        if (_cameraTransform != null)
        {
            Vector3 cameraPos = _cameraTransform.position;
            cameraPos.y = 0;
            pos.y = 0;

            float distance = Vector3.Distance(cameraPos, pos);

            volume = 1 - (distance - 15) * 0.1f;
            if (volume < 0.3f)
            {
                volume = 0.3f;
            }
            else if (volume > 1.0f)
            {
                volume = 1.0f;
            }
        }

        return PlaySoundEffect(name, false, volume);
    }

    public AudioSource PlaySoundEffect(string name)
    {
        return PlaySoundEffect(name, false, 1.0f);
    }

    public AudioSource PlaySoundEffect(string name, bool loop)
    {
        return PlaySoundEffect(name, loop, 1.0f);
    }

    public AudioSource PlaySoundEffect(string name, bool loop, float volume)
    {
        if (!EnableSFX)
        {
            return null;
        }


        if (name == null)
        {
            Debug.LogError("PlaySoundEffect name is null");
            return null;
        }

        name = name.Trim();


        if (!_dictionarySoundClip.ContainsKey(name))
        {
            Debug.LogError("PlaySoundEffect can not found name: " + name);
            return null;
        }

        SoundClip soundClip = _dictionarySoundClip[name];

        if (soundClip._rate != 100)
        {
            int random = UnityEngine.Random.Range(0, 100);
            if (random > soundClip._rate)
            {
                return null;
            }
        }

        return PlaySoundEffect(soundClip, loop, volume);
    }

    private AudioSource PlaySoundEffect(SoundClip soundClip, bool loop, float volume)
    {
        if (EnableSFX && soundClip.Clip)
        {
            int channelIndex = 0;
            int leastImportantIndex = 0;


            for (; channelIndex < _sfxChannelsNumber; channelIndex++)
            {

                if (_sfxChannel[channelIndex].clip == null)
                {
                    continue;
                }

                if (_sfxChannel[channelIndex].clip.name == soundClip.Clip.name)
                {
                    if (soundClip._onlyOne)
                    {
                        if (!_sfxChannel[channelIndex].isPlaying)
                        {
                            _sfxChannel[channelIndex].volume = volume;
                            _sfxChannel[channelIndex].Play();
                        }
                        //fixed by  should have better way to replay sound 
                        //when it is already playing
                        else if (_sfxChannel[channelIndex].time > 0.3f)
                        {
                            _sfxChannel[channelIndex].Play();
                        }
                        return _sfxChannel[channelIndex]; // exit
                    }
                    else
                    {
                        if (!_sfxChannel[channelIndex].isPlaying)
                        {
                            _sfxChannel[channelIndex].volume = volume;
                            _sfxChannel[channelIndex].Play();
                            return _sfxChannel[channelIndex]; // exit
                        }
                    }
                }
            }


            channelIndex = 0;
            for (; channelIndex < _sfxChannelsNumber; channelIndex++)
            {

                if (!_sfxChannel[channelIndex].isPlaying)
                {
                    _sfxChannel[channelIndex].clip = soundClip.Clip;
                    _sfxChannel[channelIndex].loop = loop;
                    _sfxChannel[channelIndex].priority = soundClip._priority;
                    _sfxChannel[channelIndex].volume = volume;
                    _sfxChannel[channelIndex].Play();

                    return _sfxChannel[channelIndex]; // exit
                }


                // verify the least important playing sound based on its priority and timestamp
                if (_sfxChannel[leastImportantIndex].priority < _sfxChannel[channelIndex].priority)
                {
                    leastImportantIndex = channelIndex;
                }
            }


            if (channelIndex == _sfxChannelsNumber)
            {
                _sfxChannel[leastImportantIndex].Stop();
                _sfxChannel[leastImportantIndex].clip = soundClip.Clip;
                _sfxChannel[leastImportantIndex].loop = loop;
                _sfxChannel[leastImportantIndex].priority = soundClip._priority;
                _sfxChannel[leastImportantIndex].volume = volume;
                _sfxChannel[leastImportantIndex].Play();

                return _sfxChannel[leastImportantIndex];
            }
        }
        return null;
    }

    private enum FadeMode
    {
        None,
        FadeIn,
        FadeOut
    }

    private FadeMode _fadeMode;

    private float _fadeTime;
    private float _fadeTimer;

    private float _bgMusicFadeTime;


    void Update()
    {
        UpdateBGMusic();
    }

    void UpdateBGMusic()
    {
        //volume control of fading in/out background music
        if (_fadeMode == FadeMode.FadeOut)
        {
            _fadeTimer += Time.deltaTime;
            _bgmSource.volume = (1 - _fadeTimer / _fadeTime) * _bgmVolume;

            if (_fadeTimer >= _fadeTime)
            {
                _fadeMode = FadeMode.None;
                _fadeTimer = 0;

                _bgmSource.Stop();

                Destroy(_bgmSource.clip);

                _bgmSource.clip = null;

                if (_nextMusicName != null)
                {
                    StartCoroutine(StreamPlay(_nextMusicName));
                }
            }
        }
        else if (_fadeMode == FadeMode.FadeIn)
        {
            _fadeTimer += Time.deltaTime;

            _bgmSource.volume = _fadeTimer > _fadeTime ? _bgmVolume : _fadeTimer / _fadeTime * _bgmVolume;

            if (_fadeTimer >= _fadeTime)
            {
                _fadeMode = FadeMode.None;
                _fadeTimer = 0;
                Debug.Log("[BG Music] Music fully faded in. Volume = " + _bgmSource.volume);
            }
        }
        else
        {
            if (EnableBGM &&
                _nextMusicName != null &&
                !_isLoadingBGMusic &&
                !_bgmSource.isPlaying)
            {
                _bgmSource.Stop();
                _bgmSource.volume = 0;
                StartCoroutine(StreamPlay(_nextMusicName));
                Debug.Log("[BG Music] Replaying ............" + _nextMusicName);
            }
        }
    }

    #region Download music file
    private bool _isDownloading;

    //just download, do not play
    private IEnumerator DownloadMusic(string musicName)
    {
        if (_isDownloading)
        {
            yield break;    //download one file at a time
        }
		
		_isDownloading = true;
		
		_downloadingMusicName = musicName;

        string url = FCDownloadManager.ServerMusicAddress + "/" + musicName;

        using (WWW www = new WWW(url))
        {
			Debug.Log("Downloading " + url);
			
            while (!www.isDone && string.IsNullOrEmpty(www.error))
            {
#if allow_play_remote
				//play remote
                if (!audio.isPlaying && www.url.StartsWith("http") && www.progress > 0.05f)
                {
                    this.audio.clip = www.GetAudioClip(false, true, AudioType.MPEG);

                    audio.Play();

                    Debug.Log("Start playing before fully downloaded. Progress = " + www.progress);
                }
#endif //play remote
				_downloadProgress = www.progress;
				
                yield return null;
            }
			
			_downloadProgress = 1;
				
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError("Failed to download the music: " + www.url + "\t\tError: " + www.error);

                yield break;
            }

            Debug.Log("Fully downloaded: " + url);

            if (url.StartsWith("http"))
            {
                string localFileName;

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    localFileName = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/Temp/" + musicName;
                }
                else
                {
                    localFileName = Path.Combine(Application.persistentDataPath, musicName);
                }

                try
                {
                    File.WriteAllBytes(localFileName, www.bytes);

                    Debug.Log("Downloaded and saved to local: " + localFileName);

                }
                catch (Exception)
                {
                    Debug.LogError("Save failed. -----reason unknown.");
                }

                yield return new WaitForSeconds(0.5f);

                if (!File.Exists(localFileName))
                {
                    Debug.LogWarning("Failed to save file!");
                }
            }
			
			_downloadingMusicName = null;
            _isDownloading = false;
        }
    }

    private bool HasLocalCopy(string musicName)
    {
        string dataPath;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/Temp";
        }
        else
        {
            dataPath = Application.persistentDataPath;
        }
        return File.Exists(Path.Combine(dataPath, musicName)) || File.Exists(Path.Combine(Application.streamingAssetsPath, musicName));
    }

#if test_music
	private string _testMusic = "battle_1.mp3";
	void OnGUI()
	{
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 200, 400, 40), "Platform:\t" + Application.platform.ToString());

        _testMusic = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 40), _testMusic);
		
		if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 40, 100, 40), "Play"))
		{
			PlayBGM(_testMusic, 0.2f);
		}
		
		if (_playingMusicName != null)
		{
			GUI.Label(new Rect(Screen.width /2 + 60, Screen.height / 2 - 20, 200, 40), "Playing:\t" + _playingMusicName);
		}

		if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 20, 100, 40), "Stop"))
		{
			StopBGM(0);
		}
		
		if (_isDownloading)
		{
			GUI.Label(new Rect(Screen.width /2 - 100, Screen.height / 2 + 100, 400, 40), "Downloadin:\t" + _downloadingMusicName + "\t\t" + (_downloadProgress * 100).ToString("F2") + '%');
		}
	}
#endif

#if UNITY_EDITOR && false
	void OnGUI()
	{
		//show on bottom the music playing and downloading
		string msg = string.Format("Music. Playing: {0}\tDownloading: {1} Progress {2:F2}%", _playingMusicName == null ? "None" : _playingMusicName, _downloadingMusicName == null ? "None" : _downloadingMusicName, _downloadProgress * 100);

		GUI.Label(new Rect(10, Screen.height - 20, 500, 20), msg);
	}
#endif
    #endregion //download and play
}

//#define test_effect

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIBattleRewardsHandler : GestureProcessor
{
    public GameObject _rewardRoot;
    public GameObject _particleEffect;
    public UILabel _itemName;

    public GameObject congratsGO;

    public GameObject shieldRoot;

    public GameObject[] shields;

    public GameObject rewardGO;

    public UILabel labelCongrats;

    public GameObject buttonContinue;

    public GameObject buttonQuit;

    public UILabel[] _itemAttributes;
	
	public string[] _levelToShowRate;

    private NGUICoverFlow _coverFlow;
    private List<ItemInventory> _itemList;
    private GestureController _gestureController;

    private TweenPosition _posTweener;

    // Use this for initialization
    void Start()
    {
        _gestureController = GetComponent<GestureController>();

        SoundManager.Instance.PlaySoundEffect("summary_loot_item");

        congratsGO.SetActive(false);

        _posTweener = shieldRoot.GetComponent<TweenPosition>();

        _posTweener.onFinished = OnEffectOver;
    }

    //called by OpenUI
    void OnInitialize()
    {
        buttonContinue.SetActive(true);


        {
            buttonQuit.SetActive(false);
        }
    }
	
	
    void OnEnable()
    {
        _itemList = PlayerInfo.Instance.CombatInventory.itemList;

        _rewardRoot.SetActive(true);

        _particleEffect.SetActive(true);

        _coverFlow = _rewardRoot.GetComponent<NGUICoverFlow>();
        _coverFlow._OnCoverChanged = OnCoverChanged;
        _coverFlow.Initialize();

        if (_itemList.Count == 0)
        {
            _itemName.text = Localization.instance.Get("IDS_REVIVE_NO_ITEMS");
        }
    }

    void OnDisable()
    {
        _coverFlow.Clear();

        _particleEffect.SetActive(false);
    }

    public override void ProcessGesture(Gesture.GestureData data, float delta)
    {
        int page = Mathf.RoundToInt(delta / 50f);

        if (data.direction == Gesture.Directions.Left || data.direction == Gesture.Directions.DownLeft
            || data.direction == Gesture.Directions.UpLeft)
        {
            _coverFlow.FlowCover(page);
        }
        else if (data.direction == Gesture.Directions.Right || data.direction == Gesture.Directions.DownRight
            || data.direction == Gesture.Directions.UpRight)
        {
            _coverFlow.FlowCover(-page);
        }
    }

    public override bool ProcessGesture(Gesture.Directions direction, float delta)
    {
        int page = Mathf.RoundToInt(delta / _gestureController._minimumAmplitude);

        if (page <= 0)
        {
            return false;
        }

        if (direction == Gesture.Directions.Left || direction == Gesture.Directions.DownLeft
            || direction == Gesture.Directions.UpLeft)
        {
            _coverFlow.FlowCover(page);

            return true;
        }
        else if (direction == Gesture.Directions.Right || direction == Gesture.Directions.DownRight
            || direction == Gesture.Directions.UpRight)
        {
            _coverFlow.FlowCover(-page);

            return true;
        }

        return false;
    }

 
    void OnClickBackButton()
    {
        buttonContinue.SetActive(false);

        {
#if test_effect
            if (true || BattleSummary.Instance.NewDifficultyOpened)
#else
            if (BattleSummary.Instance.NewDifficultyOpened)
#endif
            {
                StartCoroutine(ShowNewDifficultyEffect());
            }
            else
            {
                LevelManager.Singleton.ExitLevel();
            }
        }
    }

    void OnQuitCallback(ID_BUTTON buttonID)
    {
        if (buttonID == ID_BUTTON.ID_OK)
        {
            LevelManager.Singleton.ExitLevel();
        }
    }

    private IEnumerator ShowNewDifficultyEffect()
    {
        buttonContinue.SetActive(false);

        int difficultyLevel = PlayerInfo.Instance.difficultyLevel;
#if test_effect
        difficultyLevel = 1;
#endif
        rewardGO.SetActive(false);
        _particleEffect.SetActive(false);
        _rewardRoot.SetActive(false);

        labelCongrats.transform.parent.gameObject.SetActive(false);

        labelCongrats.text = string.Format(Localization.instance.Get("IDS_NEW_DIFFICULTY_LEVEL_OPENED"), Localization.instance.Get(FCConst.k_difficulty_level_names[difficultyLevel]));

        shields[difficultyLevel - 1].SetActive(true);

        shields[difficultyLevel].SetActive(true);

        Vector3 startPos = Vector3.left * 800 * (difficultyLevel - 1);

        shieldRoot.transform.localPosition = startPos;

        _posTweener.enabled = false;

        congratsGO.SetActive(true);

        SoundManager.Instance.PlaySoundEffect("menu_success");

        yield return new WaitForSeconds(1.5f);  //show the shield as still

        _posTweener.from = startPos;

        _posTweener.to = Vector3.left * 800 * difficultyLevel;

        _posTweener.Reset();

        _posTweener.enabled = true;

        shields[difficultyLevel].transform.FindChild("shield").animation.Play();

        SoundManager.Instance.PlaySoundEffect("gate appear");
    }

    private void NewDifficultyLevelOpenCallback()
    {
        congratsGO.SetActive(false);

        LevelManager.Singleton.ExitLevel();
    }

    void OnClickReplay()
    {
        LevelManager.Singleton.LoadLevelWithRandomConfig(LevelManager.Singleton.CurrentLevel, LevelManager.Singleton.CurrentDifficultyLevel);
    }

    public void OnCoverChanged(int index)
    {
        if ((index >= 0) && (index < _itemList.Count))
        {
            ItemData item = _itemList[index].ItemData;
            _itemName.text = Localization.instance.Get(item.nameIds);
            _itemName.color = UIGlobalSettings.Instance.GetColorByEnum(_itemList[index].DisplayRareLevel);


            List<string> attributesText = new List<string>();
            UIUtils.DrawTextChangeLine(_itemAttributes, attributesText);
        }
    }

    private void OnEffectOver(UITweener tweener)
    {
        StartCoroutine(AfterEffect());
    }

    private IEnumerator AfterEffect()
    {
        labelCongrats.transform.parent.gameObject.SetActive(true);

        labelCongrats.transform.parent.GetComponent<TweenScale>().Reset();

        SoundManager.Instance.PlaySoundEffect("Quest_finish");

        yield return new WaitForSeconds(0.9f);

        SoundManager.Instance.PlaySoundEffect("equip_success"); //estimate

        yield return new WaitForSeconds(1f);

        buttonContinue.SetActive(true);
    }
}

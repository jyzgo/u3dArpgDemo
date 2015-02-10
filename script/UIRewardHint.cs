using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIRewardHint : MonoBehaviour
{
    //interface
    public float maxLastingTime = 2.0f;

    public float minLastingTime = 0.3f;

    public float maxFadeTime = 0.5f;

    public float minFadeTime = 0.2f;

    public GameObject displayBoard;
    public UILabel nameLabel;
    public UITexture icon;
    public UISprite iconBG;
    //end interface

    private float _lastingTime = 2.0f;
    private float _fadeTime;

    private static UIRewardHint _instance;
    public static UIRewardHint Instance
    {
        get { return _instance; }
    }

    private class HintItem
    {
        public ItemData itemData;
        public int amount;
        public int extraRareLevel;

        public HintItem(ItemData itemData, int amount, int extraRareLevel)
        {
            this.itemData = itemData;
            this.amount = amount;
            this.extraRareLevel = extraRareLevel;
        }
    }
    private Queue<HintItem> _itemQueue = new Queue<HintItem>();

    private TweenAlpha _tweener;

    private float _time;

    private enum EnumStatus
    {
        idle,
        fadeIn,
        fadeOut,
        lasting,
    }

    private EnumStatus _status;

    void Awake()
    {
        displayBoard.SetActive(false);

        _instance = this;

        _tweener = GetComponent<TweenAlpha>();

        _tweener.enabled = false;

        _status = EnumStatus.idle;
    }

    public void EnqueueDisplayItem(string itemId, int amount, int extraRareLevel = 0)
    {
        ItemData itemData = DataManager.Instance.GetItemData(itemId);
        gameObject.SetActive(true);
        if (itemData != null)
        {
            HintItem hintItem = new HintItem(itemData, amount, extraRareLevel);
            _itemQueue.Enqueue(hintItem);
        }
    }

    public void StopAndClearDisplay()
    {
        _tweener.enabled = false;

        _itemQueue.Clear();

        displayBoard.SetActive(false);
    }

    void Update()
    {
        switch (_status)
        {
            case EnumStatus.idle:
                if (_itemQueue.Count == 0) break;

                displayBoard.SetActive(true);
                UpdateItemDisplay();
                FadeIn();
                break;

            case EnumStatus.fadeIn:
                break;

            case EnumStatus.fadeOut:
                break;

            case EnumStatus.lasting:
                _time += Time.deltaTime;
                if (_time > _lastingTime)
                {
                    FadeOut();
                }
                break;
        }
    }

    private void UpdateItemDisplay()
    {
        HintItem hint = _itemQueue.Dequeue();

        //show
        int role = (int)PlayerInfo.Instance.Role;

        icon.mainTexture = null;    //force to load

        UIUtils.LoadTexture(icon, hint.itemData, role);

        iconBG.spriteName = UIGlobalSettings.QualityNamesMap[(ItemQuality)hint.itemData.rareLevel];

        nameLabel.text = string.Format(Localization.Localize("IDS_MESSAGE_COMBAT_LOOT_ITEM"),
            hint.itemData.DisplayNameWithRareColor);

        //if (hint.amount > 1 || hint.itemData._id == "valor_point")
        {
            nameLabel.text = nameLabel.text + " x " + hint.amount.ToString(); 
        }
    }

    private void FadeOut()
    {
        _status = EnumStatus.fadeOut;

        _tweener.from = 1;
        _tweener.to = 0;
        _tweener.duration = _fadeTime;
        _tweener.onFinished = OnFadeOutFinished;
        _tweener.Reset();

        _tweener.enabled = true;
    }

    private void FadeIn()
    {
        CalculateDisplayTime();

        _status = EnumStatus.fadeIn;

        _tweener.from = 0;
        _tweener.to = 1;
        _tweener.duration = _fadeTime;
        _tweener.onFinished = OnFadeInFinished;
        _tweener.Reset();
        _tweener.enabled = true;
    }

    private void OnFadeInFinished(UITweener tweener)
    {
        _time = 0;
        _status = EnumStatus.lasting;
    }

    private void OnFadeOutFinished(UITweener tweener)
    {
        if (_itemQueue.Count == 0) displayBoard.SetActive(false);

        _status = EnumStatus.idle;
    }

    private int k_start_size = 3;		//minimum size for the formula to take effect

    private void CalculateDisplayTime()
    {

        int queueSize = _itemQueue.Count;

        float percent = queueSize < k_start_size ? 1f : 1f * k_start_size / queueSize;

        _lastingTime = Mathf.Clamp(maxLastingTime * percent, minLastingTime, maxLastingTime);

        _fadeTime = Mathf.Clamp(maxFadeTime * percent, minFadeTime, maxFadeTime);

        //Debug.LogError(string.Format("Queue size = {0}  Percent = {1:F3}   Lasting = {2:F2}", queueSize, percent, _lastingTime));
    }

    void OnDestroy()
    {
        _instance = null;
    }

#if debug_hint
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 180, 80, 30), "Show item"))
        {
            this.EnqueueDisplayItem("all_helm_2_blue_hpmfi", 100);
        }
    }
#endif
}
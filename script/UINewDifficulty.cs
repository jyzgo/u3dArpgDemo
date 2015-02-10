using UnityEngine;
using System.Collections;

public class UINewDifficulty : MonoBehaviour
{
    public UILabel labelDesc;

    private TweenAlpha _tweener;

    void Awake()
    {
        _tweener = GetComponent<TweenAlpha>();
        _tweener.onFinished = OnFadeOutFinished;
    }

    private void OnInitialize()
    {
        collider.enabled = true;
        _tweener.enabled = false;

        int difficulty = PlayerInfo.Instance.difficultyLevel;

        labelDesc.text = string.Format(Localization.instance.Get("IDS_NEW_DIFFICULTY_LEVEL_QUEST_AVAILABLE"), Localization.instance.Get(FCConst.k_difficulty_level_names[difficulty]));
    }

    private void OnButtonOKClick()
    {
        this.collider.enabled = false;
        
        _tweener.enabled = true;
        _tweener.Reset();

        UIManager.Instance.OpenUI("TownHome");
    }

    private void OnFadeOutFinished(UITweener tweener)
    {
        UIManager.Instance.CloseUI("NewDifficulty");
    }
}

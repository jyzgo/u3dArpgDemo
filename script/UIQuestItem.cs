using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIQuestItem : MonoBehaviour
{
    public GameObject iconNew;

    public UITexture iconNPC;

    public GameObject mainQuestBG;

	public GameObject sideQuestBG;

	public GameObject dailyQuestBG;

    public GameObject glowEffect;  //shown when selected

    public GameObject tick;

    private UIQuest _uiQuest;       //caller

    private QuestProgress _questProgress;   //the quest progress of this quest item

    public QuestProgress questProgress { get { return _questProgress; } }

    private QuestData _questData;

    public QuestData questData { get { return _questData; } }

    private const string k_npc_icon_path = "Assets/UI/bundle/NpcIcons/{0}.png";

    public void Init(QuestProgress qp, UIQuest uiQuest)
    {
        QuestData qd = QuestManager.instance.CurrentQuestList.FindQuestDataByID(qp.quest_id);

        iconNPC.mainTexture = InJoy.AssetBundles.AssetBundles.Load(string.Format(k_npc_icon_path, qd.giver)) as Texture2D;

        //main quest or side
        mainQuestBG.SetActive(qd.quest_type == QuestType.main);
        sideQuestBG.SetActive(qd.quest_type != QuestType.main);

        iconNew.SetActive(!qp.viewed);
        glowEffect.SetActive(false);
        tick.SetActive(qp.isCompleted);

        //remember
        _uiQuest = uiQuest;
        _questProgress = qp;
        _questData = qd;
    }

    private void OnClick()
    {
        _uiQuest.SetFocusItem(this);
    }

    public void OnSetFocus()
    {
		if (!_questProgress.viewed)
		{
			_questProgress.viewed = true;

			Utils.RememberViewedQuest(_questData.quest_id);
		}
		
		glowEffect.SetActive(true);
        
        iconNew.SetActive(false);
    }

    public void OnLoseFocus()
    {
        glowEffect.SetActive(false);
    }
}
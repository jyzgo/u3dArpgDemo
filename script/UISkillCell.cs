using UnityEngine;
using System.Collections;

public class UISkillCell : MonoBehaviour
{
	public GameObject iconNew;

	// Shaders
	public Shader _grayShader;
	public Shader _normalShader;

	private UISkillsHandler _skillsHandler;

	public UITexture skillIcon;

	private SkillData _skillData;
	public SkillData skillData
	{
		get
		{
			return _skillData; 
		}
	}

	private bool unlocked;
	public bool Unlocked
	{
		set
		{
			if (!value)
			{
				skillIcon.shader = _normalShader;
			}
			else
			{
				skillIcon.shader = _grayShader;
			}
			gameObject.SetActive(false);
			_skillsHandler.gridPanel.UpdateDrawcalls();
			gameObject.SetActive(true);
			_skillsHandler.gridPanel.UpdateDrawcalls();
		}
	}

	void Start()
	{
		UpdateFlagState();
	}

	public void SetData(SkillData skillData, UISkillsHandler handler)
	{
		_skillsHandler = handler;

		_skillData = skillData;

		Texture2D tex = InJoy.AssetBundles.AssetBundles.Load(skillData.iconPath) as Texture2D;
		skillIcon.mainTexture = tex;

		int skillRank = PlayerInfo.Instance.GetSkillLevel(skillData.skillID);
		this.Unlocked = skillRank == 0;
		
		TweenScale tweener = GetComponent<TweenScale>();
		tweener.enabled = false;
	}

	public void UpdateFlagState()
	{
		if (UISkillsHandler.IsNewSkill(_skillData))
		{
			iconNew.SetActive(true);
		}
		else
		{
			iconNew.SetActive(false);
		}
	}

	private void OnClick()
	{
		_skillsHandler.SelectSkill(gameObject);

		SoundManager.Instance.PlaySoundEffect("button_normal");
	}
}
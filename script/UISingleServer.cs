using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UISingleServer : MonoBehaviour
{
    public UIImageButton cubeButton;
    public UIImageButton imageButton;
    public UISprite backGround1;
    public UISprite backGround2;
    public UILabel serverNameLabel;
    public UILabel serverIdLabel;

    private UISelectServer uiSelectServer;
    private ServerInfo serverInfo;
    private bool isCaheServer = false;

    private static bool _isSelected;

    void OnEnable()
    {
        _isSelected = false;
    }

    public void Init(UISelectServer uss, ServerInfo info, ServerButtonImage buttonImage, bool isCache = false)
    {
        uiSelectServer = uss;
        serverInfo = info;
        isCaheServer = isCache;

        serverIdLabel.text = serverInfo.id.ToString();
        serverNameLabel.text = string.Format("{0}({1})", serverInfo.name, buttonImage.buttonLabelTxt);
        bool isEnable = buttonImage.State != ServerState.Unavailable;
        if (isEnable)
        {
            imageButton.label.color = buttonImage.labelColor;
            cubeButton.label.color = buttonImage.labelColor;
        }
        imageButton.isEnabled = isEnable;
        cubeButton.isEnabled = isEnable;
    }

    void OnClick()
    {
        if (!imageButton.isEnabled)
        {
            return;
        }

        if (_isSelected)
        {
            return;
        }
        _isSelected = true;

        NetworkManager.Instance.ServerID = (short)serverInfo.id;
        PlayerPrefs.SetInt(PrefsKey.ServerID, serverInfo.id);

        int cacheOne = PlayerPrefs.GetInt(PrefsKey.ServerCacheOne);

        if (cacheOne != serverInfo.id)
        {
            PlayerPrefs.SetInt(PrefsKey.ServerCacheOne, serverInfo.id);
            PlayerPrefs.SetInt(PrefsKey.ServerCacheTwo, cacheOne);
        }

        NetworkManager.Instance.SelectedRole = EnumRole.NotSelected;
        CharacterSelector.Instance.IsManualSelectServerComplete = true;
        CharacterSelector.Instance.StepAt(EnumLoadingStep.step1);
    }

    private void SaveServerCache()
    {
        if (!PlayerPrefs.HasKey(PrefsKey.ServerCacheOne))
        {
            PlayerPrefs.SetInt(PrefsKey.ServerCacheOne, serverInfo.id);
        }
        else
        {
            int cacheOne = PlayerPrefs.GetInt(PrefsKey.ServerCacheOne);

            if (cacheOne == serverInfo.id)
            {
                return;
            }

            PlayerPrefs.SetInt(PrefsKey.ServerCacheOne, serverInfo.id);

            if (!PlayerPrefs.HasKey(PrefsKey.ServerCacheTwo))
            {
                PlayerPrefs.SetInt(PrefsKey.ServerCacheTwo, cacheOne);
            }
            else
            {
                int cacheTwo = PlayerPrefs.GetInt(PrefsKey.ServerCacheTwo);

                if (cacheOne != cacheTwo)
                {
                    PlayerPrefs.SetInt(PrefsKey.ServerCacheTwo, cacheOne);
                }
            }
        }

    }
}
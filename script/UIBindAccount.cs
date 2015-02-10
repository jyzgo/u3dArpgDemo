using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FaustComm;


public class UIBindAccount : MonoBehaviour
{
    private static UIBindAccount _instance;
    public UIPanel blackBackground;

    public UILabel title;

    public UIImageButton closeButton;
    public UIImageButton returnButton;
    public UIImageButton bindButton;
    public UILabel bindText;

    public UILabel accountRule;
    public UILabel accountContent;
    public UILabel accountError;
    public UISprite acccountGreen;
    public UISprite accounttRed;

    public UILabel passwordRule;
    public UILabel passwordContent;
    public UILabel passwordError;
    public UISprite passwordGreen;
    public UISprite passwordRed;

    public UILabel passwordAgainContent;
    public UILabel passwordAgainError;
    public UISprite passwordAgainGreen;
    public UISprite passwordAgainRed;

    public int accountMin = 4;
    public int accountMax = 30;
    public int passwordMinCount = 6;
    public int passwordMaxCount = 20;

    void Awake()
    {
        _instance = this;
        acccountGreen.gameObject.SetActive(false);
        passwordGreen.gameObject.SetActive(false);
        passwordAgainGreen.gameObject.SetActive(false);

        accounttRed.gameObject.SetActive(false);
        passwordRed.gameObject.SetActive(false);
        passwordAgainRed.gameObject.SetActive(false);

        UIEventListener.Get(closeButton.gameObject).onClick = OnCloseWindow;
        UIEventListener.Get(bindButton.gameObject).onClick = OnBind;
    }

    void OnEnable()
    {
        blackBackground.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        blackBackground.gameObject.SetActive(false);
    }

    public void OnInitialize()
    {
        accountContent.text = Localization.instance.Get("IDS_MESSAGE_LOGIN_INPUTACCOUNT");
        passwordContent.text = Localization.instance.Get("IDS_MESSAGE_LOGIN_INPUTPASSWORD");
        passwordAgainContent.text = Localization.instance.Get("IDS_MESSAGE_REGISTER_INPUTPASSWORDAGAIN");

        UIEventListener.Get(returnButton.gameObject).onClick = OnCloseWindow;

        title.text = Localization.instance.Get("IDS_TITLE_BIND");
        bindText.text = Localization.instance.Get("IDS_BUTTON_LOGIN_BIND");
    }

    bool CheckAccount()
    {
        bool legal = false;
        string accountText = accountContent.text;
        if (accountText.IndexOf("|") >= 0)
        {
            acccountGreen.gameObject.SetActive(false);
            accountError.gameObject.SetActive(false);
            accountRule.gameObject.SetActive(true);
            accounttRed.gameObject.SetActive(false);
            return legal;
        }

        if (accountText == Localization.instance.Get("IDS_MESSAGE_LOGIN_INPUTACCOUNT") ||
            string.IsNullOrEmpty(accountText))//empty
        {
            accountRule.gameObject.SetActive(true);
            accountError.gameObject.SetActive(false);
            accounttRed.gameObject.SetActive(false);
            acccountGreen.gameObject.SetActive(false);
        }
        else
        {
            if (!ValidateAccount())                 //不是邮箱格式
            {
                accounttRed.gameObject.SetActive(true);
                accountRule.gameObject.SetActive(true);
                acccountGreen.gameObject.SetActive(false);
                accountError.gameObject.SetActive(true);
                accountError.text = Localization.instance.Get("IDS_MESSAGE_REGISTER_ACCOUNTINFOWRONG");
            }
            else
            {
                legal = true;
                acccountGreen.gameObject.SetActive(true);
                accountError.gameObject.SetActive(false);
                accountRule.gameObject.SetActive(false);
                accounttRed.gameObject.SetActive(false);
            }
        }
        return legal;
    }

    bool CheckPassword()
    {
        passwordRule.text = Localization.instance.Get("IDS_MESSAGE_LOGIN_PASSWORDRULE");
        passwordRule.gameObject.SetActive(true);
        bool legal = false;
        string passwordText = passwordContent.text;
        if (passwordText.IndexOf("|") >= 0)
        {
            passwordRed.gameObject.SetActive(false);
            passwordGreen.gameObject.SetActive(false);
            passwordError.gameObject.SetActive(false);
            return legal;
        }
        if (passwordText == Localization.instance.Get("IDS_MESSAGE_LOGIN_INPUTPASSWORD") ||
            string.IsNullOrEmpty(passwordText))
        {
            //clean
            passwordRed.gameObject.SetActive(false);
            passwordGreen.gameObject.SetActive(false);
            passwordError.gameObject.SetActive(false);
        }
        else
        {//dirty
            if (!ValidatePassword())
            {
                passwordRed.gameObject.SetActive(true);
                passwordGreen.gameObject.SetActive(false);
                passwordError.gameObject.SetActive(true);
                passwordError.text = Localization.Localize("IDS_MESSAGE_LOGIN_PASSWORDWRONG");
            }
            else
            {
                passwordRule.gameObject.SetActive(false);
                passwordRed.gameObject.SetActive(false);
                passwordGreen.gameObject.SetActive(true);
                passwordError.gameObject.SetActive(false);
                legal = true;
            }
        }
        return legal;
    }

    bool CheckRepassword()
    {
        bool legal = false;
        string passwordText = passwordContent.text;

        if (passwordText.IndexOf("|") >= 0)
        {
            passwordText = passwordText.Remove(passwordText.IndexOf("|"));
        }
        string repasswordText = passwordAgainContent.text;
        if (repasswordText.IndexOf("|") >= 0)
        {
            passwordAgainRed.gameObject.SetActive(false);
            passwordAgainGreen.gameObject.SetActive(false);
            passwordAgainError.gameObject.SetActive(false);
            return legal;
        }

        if (string.IsNullOrEmpty(repasswordText) ||
             Localization.Localize("IDS_MESSAGE_REGISTER_INPUTPASSWORDAGAIN") == repasswordText)
        {
            passwordAgainRed.gameObject.SetActive(false);
            passwordAgainGreen.gameObject.SetActive(false);
            passwordAgainError.gameObject.SetActive(false);
        }
        else
        {
            if (!ValidateRepassword())
            {
                passwordAgainError.text = Localization.Localize("IDS_MESSAGE_REGISTER_TWOPASSWORDWRONG");
                passwordAgainRed.gameObject.SetActive(true);
                passwordAgainGreen.gameObject.SetActive(false);
                passwordAgainError.gameObject.SetActive(true);
            }
            else if (passwordRed.gameObject.activeInHierarchy)
            {
                passwordAgainError.text = "";
                passwordAgainRed.gameObject.SetActive(true);
                passwordAgainGreen.gameObject.SetActive(false);
                passwordAgainError.gameObject.SetActive(true);
            }
            else
            {
                passwordAgainRed.gameObject.SetActive(false);
                passwordAgainGreen.gameObject.SetActive(true);
                passwordAgainError.gameObject.SetActive(false);
                legal = true;
            }
        }
        return legal;
    }

    private GameObject successMessageBox;

    void OnBind(GameObject go)
    {
        if (CheckAccount() && CheckPassword() && CheckRepassword())
        {
            ConnectionManager.Instance.RegisterHandler(OnRequestBind, true);
        }
        else
        {
            string error = string.Empty;
            if (Localization.Localize("IDS_MESSAGE_LOGIN_INPUTACCOUNT") == accountContent.text)
                error = accountContent.text;
            else if (Localization.Localize("IDS_MESSAGE_LOGIN_INPUTPASSWORD") == passwordContent.text)
                error = passwordContent.text;
            else if (Localization.Localize("IDS_MESSAGE_REGISTER_INPUTPASSWORDAGAIN") == passwordAgainContent.text)
                error = passwordAgainContent.text;
            else if (!ValidateAccount())
                error = Localization.Localize("IDS_MESSAGE_REGISTER_ACCOUNTINFOWRONG");
            else if (!ValidatePassword())
                error = Localization.Localize("IDS_MESSAGE_LOGIN_PASSWORDWRONG");
            else if (!ValidateRepassword())
                error = Localization.Localize("IDS_MESSAGE_REGISTER_TWOPASSWORDWRONG");
            UIMessageBoxManager.Instance.ShowMessageBox(error, "", MB_TYPE.MB_OK, null);
        }
    }

    void OnRequestBind()
    {
        NetworkManager.Instance.OnBindAccount(accountContent.text, passwordContent.text, OnBindCallBack);
    }

    void OnBindCallBack(NetResponse msg)
    {
        ConnectionManager.Instance.SendACK(OnRequestBind, true);
        if (msg.errorCode == 0)
        {
            PlayerPrefs.SetString(PrefsKey.Account, accountContent.text);
            PlayerPrefs.SetString(PrefsKey.Password, passwordContent.text);

            NetworkManager.Instance.Account = accountContent.text;
            NetworkManager.Instance.Password = passwordContent.text;
            NetworkManager.Instance.IsAccountBind = true;
            //CharacterSelector.Instance.IsLoadingCharacter = true;
            CharacterSelector.Instance.RefreshAccount();

            successMessageBox = UIMessageBoxManager.Instance.ShowMessageBox(Localization.Localize("IDS_MESSAGE_LOGIN_BOUNDSUCCESSFUL"),
                "", MB_TYPE.MB_OK, OnCloseSuccessTips);
        }
        else
        {
            string errorMessage = Utils.GetErrorIDS(msg.errorCode);
            UIMessageBoxManager.Instance.ShowMessageBox(errorMessage, "", MB_TYPE.MB_OK, null);
        }
    }

    void OnCloseSuccessTips(ID_BUTTON buttonID)
    {
        UIMessageBoxManager.Instance.CloseMessageBox(successMessageBox);
        OnCloseWindow();
    }

    void OnRegisterCallBack(NetResponse msg)
    {
        Debug.Log("OnRegisterCallBack");

        if (msg.errorCode == 0)
        {
            RegisterResponse myMsg = (RegisterResponse)msg;

			NetworkManager.Instance.ResetSession(myMsg.accountId, myMsg.session);

            NetworkManager.Instance.Account = accountContent.text;
            NetworkManager.Instance.Password = passwordContent.text;
            NetworkManager.Instance.IsAccountBind = true;
            PlayerPrefs.SetString(PrefsKey.Account, accountContent.text);
            PlayerPrefs.SetString(PrefsKey.Password, passwordContent.text);

            Application.LoadLevel("Null");
            LoadCharacterSelectionScene();
        }
        else
        {
            string errorMessage = Utils.GetErrorIDS(msg.errorCode);
            UIMessageBoxManager.Instance.ShowMessageBox(errorMessage, "", MB_TYPE.MB_OK, null);
        }
    }

    void LoadCharacterSelectionScene()
    {
        StopAllCoroutines();
        NetworkManager.Instance.SelectedRole = EnumRole.NotSelected;
        Application.LoadLevelAsync("character_selection");
    }

    void OnCloseWindow(GameObject go = null)
    {
        UIManager.Instance.CloseUI("UIBindAccount");
        UIManager.Instance.OpenUI("CharacterSelection");
    }

    void Update()
    {
        CheckAccount();
        CheckPassword();
        CheckRepassword();
    }

    bool ValidateAccount()
    {
        string accountText = accountContent.text;
        if (!Utils.IsNumberOrEnglish(accountText[0]) || //首字母不是数字或者字母
            accountText.Length < accountMin ||          //长度小于最小值
            accountText.Length > accountMax ||          //长度大于最大值
            !Utils.FilterWords(accountText) ||          //过滤字
            !Utils.IsMail(accountText))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ValidatePassword()
    { 
        string passwordText = passwordContent.text;
        if (passwordText.Length < passwordMinCount
            || passwordText.Length > passwordMaxCount)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ValidateRepassword()
    {
        if (passwordContent.text == passwordAgainContent.text)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static UIBindAccount Instance { get { return _instance; } }

}

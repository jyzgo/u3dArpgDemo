using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FaustComm;


public class UILogin : MonoBehaviour
{
    public UIPanel blackBackground;
    private static UILogin _instance;

    public GameObject account;
    public UILabel accountContent;
    public UILabel accountError;

    public GameObject password;
    public UILabel passwordContent;

    public UIImageButton loginButton;
    public UIImageButton backButton;
    public UIImageButton closeButton;

    private const int accountMin = 4;
    private const int accountMax = 30;
    private bool _isCheckAccountError = false;
    private string _oldErrorAccount = string.Empty;

    private GameObject _errorMessageBox;

    void OnEnable()
    {
        blackBackground.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        blackBackground.gameObject.SetActive(false);
    }

    void Awake()
    {
        _instance = this;

        accountContent.text = Localization.instance.Get("IDS_MESSAGE_LOGIN_INPUTACCOUNT");
        passwordContent.text = Localization.instance.Get("IDS_MESSAGE_LOGIN_PASSWORD");

        UIEventListener.Get(loginButton.gameObject).onClick = Login;
        UIEventListener.Get(backButton.gameObject).onClick = OnClosePanel;
        UIEventListener.Get(closeButton.gameObject).onClick = OnClosePanel;
    }

    void OnClosePanel(GameObject go)
    {
        UIManager.Instance.CloseUI("UILogin");
        UIManager.Instance.OpenUI("CharacterSelection");
    }

    private void OnCloseErrorTips(ID_BUTTON buttonID)
    {
        UIMessageBoxManager.Instance.CloseMessageBox(_errorMessageBox);
    }

    void Login(GameObject go)
    {
        NetworkManager.Instance.OnLogin(accountContent.text, passwordContent.text, OnLoginCallback);
    }

    private GameObject successMessageBox;

    private void OnLoginCallback(NetResponse msg)
    {
        if (msg.errorCode == 0)
        {
            LoginResponse myMsg = (LoginResponse)msg;

			NetworkManager.Instance.ResetSession(myMsg.accountId, myMsg.session);

            Debug.Log(string.Format("Login succeeded. AccountId = {0}  Session id = {1}", myMsg.accountId, myMsg.session));

            NetworkManager.Instance.Account = accountContent.text;
            NetworkManager.Instance.Password = passwordContent.text;
            NetworkManager.Instance.IsAccountBind = true;
            PlayerPrefs.SetString(PrefsKey.Account, accountContent.text);
            PlayerPrefs.SetString(PrefsKey.Password, passwordContent.text);

            //Application.LoadLevel("Null");
            LoadCharacterSelectionScene();
            //StartCoroutine(Wait());
            OnClosePanel(null);
        }
        else
        {
            successMessageBox = UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_LOGIN_ERRPR"), Localization.instance.Get("IDS_TITLE_GLOBAL_WARNING"), MB_TYPE.MB_OK, OnCloseSuccessTips);

            Debug.Log("Login failed. Error code = " + msg.errorCode);
        }
    }

    private void OnCloseSuccessTips(ID_BUTTON buttonID)
    {
        UIMessageBoxManager.Instance.CloseMessageBox(successMessageBox);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
    }

    private void LoadCharacterSelectionScene()
    {
        StopAllCoroutines();
        NetworkManager.Instance.SelectedRole = EnumRole.NotSelected;
        Application.LoadLevelAsync("character_selection");
    }


    private void ShowAccountErrorText(string str, bool isLegal)
    {
        if (accountError.text != str)
        {
            accountError.text = str;
        }
    }


    private void CheckAccount()
    {
        string checkAccountText = accountContent.text;
        if (checkAccountText.IndexOf("|") >= 0)
        {
            checkAccountText = checkAccountText.Remove(checkAccountText.IndexOf("|"));
        }

        if (checkAccountText == Localization.instance.Get("IDS_MESSAGE_LOGIN_INPUTACCOUNT") || null == checkAccountText || checkAccountText.Length <= 0)
        {
            ShowAccountErrorText(string.Empty, false);
            return;
        }

        if (!Utils.IsNumberOrEnglish(checkAccountText[0]) || //首字母不是数字或者字母
            accountContent.text.Length < accountMin ||          //长度小于最小值
            accountContent.text.Length > accountMax ||          //长度大于最大值
            !Utils.FilterWords(checkAccountText) ||          //过滤字
            !Utils.IsMail(checkAccountText))                 //不是邮箱格式
        {
            ShowAccountErrorText(Localization.instance.Get("IDS_MESSAGE_REGISTER_ACCOUNTINFOWRONG"), false);
        }
        else if (_isCheckAccountError)
        {
            ShowAccountErrorText(Localization.instance.Get("IDS_MESSAGE_REGISTER_ACCOUNTINFOWRONG"), false);

            if (_oldErrorAccount != string.Empty && _oldErrorAccount != checkAccountText)
            {
                _isCheckAccountError = false;
                ShowAccountErrorText(string.Empty, false);
            }
        }
        else
        {
            ShowAccountErrorText(string.Empty, true);
        }
    }

    void Update()
    {
        CheckAccount();
    }


    public static UILogin Instance
    {
        get { return _instance; }
    }

}


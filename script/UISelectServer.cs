using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FaustComm;


public enum ServerState
{ 
    Unavailable = -1,
    New,
    Recommended,
    Normal,
    Hot
}


public class ServerButtonImage
{
    public Color labelColor;

    public string buttonLabelTxt;

    public ServerState State;

    public ServerButtonImage(ServerState state, Color color, string key)
    {
        labelColor = color;
        State = state;
        buttonLabelTxt = Localization.instance.Get(key);
    }
}

public class UISelectServer : MonoBehaviour
{
    public UIPanel blackBackground;

    private static UISelectServer _instance;

    public UIImageButton closeButton;

    public UIGrid cacheGrid;
    public UIGrid allServerGrid;
    public UIDraggablePanel allDraggablePanel;

    public UISingleServer cloneSingle;

    void Awake()
    {
        cloneSingle.gameObject.SetActive(false);
        UIEventListener.Get(closeButton.gameObject).onClick = OnCloseButtonClicked;
        _instance = this;
    }

    void Start()
    {
        Initialize();
    }

    void OnInitializeWithCaller(bool isFirstEnterGame)
    {
        closeButton.gameObject.SetActive(!isFirstEnterGame);
    }

    void OnEnable()
    {
        blackBackground.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        blackBackground.gameObject.SetActive(false);
    }

    void Initialize()
    {
        InitThreeServer();

        ///////

        InitAllServer();
    }

    void InitThreeServer()
    {
        List<ServerInfo> serverList = FCDownloadManager.Instance.GameServers;
        serverList.Sort(ServerInfo.CompareServerFromMaxToMin); //paixu
        const int Count = 3;
        List<ServerInfo> threeServerList = new List<ServerInfo>();

        ServerInfo serverInfo = GetServerInfoFromCache(PrefsKey.ServerCacheOne);

        if (null != serverInfo)
        {
            threeServerList.Add(serverInfo);
        }

        serverInfo = GetServerInfoFromCache(PrefsKey.ServerCacheTwo);

        if (null != serverInfo)
        {
            threeServerList.Add(serverInfo);
        }

        if (threeServerList.Count >= 1)
        {
            foreach (ServerInfo info in serverList)
            {
                if (threeServerList.Count >= Count)
                {
                    break;
                }

                if (!threeServerList.Contains(info))
                {
                    threeServerList.Add(info);
                }
            }
        }
        else
        {
            List<ServerInfo> recommendedList = serverList.FindAll(
                        delegate(ServerInfo info)
                        {
                            return info.state == ServerState.Recommended;
                        });

            foreach (ServerInfo info in recommendedList)
            {
                if (threeServerList.Count >= Count)
                {
                    break;
                }

                if (!threeServerList.Contains(info))
                {
                    threeServerList.Add(info);
                }
            }

            if (threeServerList.Count < Count)
            {
                foreach (ServerInfo info in serverList)
                {
                    if (threeServerList.Count >= Count)
                    {
                        break;
                    }

                    if (!threeServerList.Contains(info))
                    {
                        threeServerList.Add(info);
                    }
                }
            }
        }

        int index = 0;
        foreach (ServerInfo info in threeServerList)
        {
            ServerButtonImage buttonImage = GetServerButtonImageByState(info.state);
            GameObject obj = NGUITools.AddChild(cacheGrid.gameObject, cloneSingle.gameObject);
            UISingleServer singServer = obj.GetComponent<UISingleServer>();
            singServer.Init(this, info, buttonImage, true);
            singServer.gameObject.SetActive(true);
            singServer.name = string.Format("sort{0}", index.ToString("000"));
            ++index;
        }

        UIGrid uigrid = cacheGrid.GetComponent<UIGrid>();
        uigrid.repositionNow = true;
    }

    void InitAllServer()
    {
        int index = 0;
        foreach (ServerInfo info in FCDownloadManager.Instance.GameServers)
        {
            ServerButtonImage buttonImage = GetServerButtonImageByState(info.state);
            GameObject obj = NGUITools.AddChild(allServerGrid.gameObject, cloneSingle.gameObject);
            obj.transform.localPosition = cloneSingle.transform.localPosition;
            UISingleServer singServer = obj.GetComponent<UISingleServer>();
            singServer.Init(this, info, buttonImage);
            singServer.gameObject.SetActive(true);
            obj.name = string.Format("sort{0}", index.ToString("000"));
            ++index;
        }

        UIGrid uigrid = allServerGrid.GetComponent<UIGrid>();
        uigrid.repositionNow = true;
    }


    ServerInfo GetServerInfoFromCache( string key)
    {
        List<ServerInfo> serverList = FCDownloadManager.Instance.GameServers;
        ServerInfo serverInfo = null;

        if (PlayerPrefs.HasKey(key))
        {
            short cacheValue = (short)PlayerPrefs.GetInt(key);
            serverInfo = serverList.Find(delegate(ServerInfo info) { return info.id == cacheValue; });
        }

        return serverInfo;
    }


    ServerButtonImage GetServerButtonImageByState(ServerState state)
    {
        ServerButtonImage buttonImage;
        Color color;
        switch (state)
        { 
            case ServerState.Unavailable:
                color = new Color(246f / 255f, 246f / 255f, 246f / 255f);
                buttonImage = new ServerButtonImage(state, color, "IDS_NAME_SERVERSTATUS_UNAVAILABLE");
                break;
            case ServerState.New:
                color = new Color(0f / 255f, 255f / 255f, 255f / 255f);
                buttonImage = new ServerButtonImage(state, color, "IDS_NAME_SERVERSTATUS_NEW");
                break;
            case ServerState.Recommended:
                color = new Color(0f / 255f, 255f / 255f, 255f / 255f);
                buttonImage = new ServerButtonImage(state, color, "IDS_NAME_SERVERSTATUS_RECOMMAND");
                break;
            case ServerState.Normal:
                color = new Color(40f / 255f, 220f / 255f, 0f / 255f);
                buttonImage = new ServerButtonImage(state, color, "IDS_NAME_SERVERSTATUS_NORMAL");
                break;
            case ServerState.Hot:
                color = new Color(255f / 255f, 190f / 255f, 0f / 255f);
                buttonImage = new ServerButtonImage(state, color, "IDS_NAME_SERVERSTATUS_HOT");
                break;
            default:
                buttonImage = null;
                break;
        }
        return buttonImage;
    }

    void OnCloseButtonClicked(GameObject go)
    {
        UIManager.Instance.CloseUI("UISelectServer");
        UIManager.Instance.OpenUI("CharacterSelection");
        CharacterSelector.Instance.IsManualSelectServerComplete = true;
    }

    public static UISelectServer Instance { get { return _instance; } set { _instance = value; } }
}

using UnityEngine;
using System.Collections;

public class PreBoot : MonoBehaviour
{
	void Start()
	{
		ClearSingletons();

		//for release, go to prod
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
		NetworkManager.CurEndPoint = InJoy.FCComm.Endpoint.STAGE3;
		FCDownloadManager.URL_SERVER_ROOT = "http://official_cdn_site";
		Application.LoadLevel("Boot");
		return;
#endif //!DEVELOPMENT_BUILD

    }

#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
	void OnGUI()
	{
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        
        GUILayout.BeginHorizontal();
        float buttonWidth = Screen.width / 4;
        float buttonHeight = Screen.height / 5 - 10;
        GUILayout.Space((Screen.width - buttonWidth) / 2);
		GUILayout.BeginVertical();

		//enter boot

        if (GUILayout.Button("Stand-Alone", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
		{
			FCDownloadManager.CurServerPostType = FCDownloadManager.ServerPostType.SPT_TEST;
			FCDownloadManager.URL_SERVER_ROOT = FCDownloadManager.URL_SERVER_ROOT_TEST;;
			InJoy.UnityBuildSystem.BuildInfo.ServerPostTag = "test1";
			NetworkManager.isOfflinePlay = true;
			Application.LoadLevel("Boot");
			return;
		}

        if (GUILayout.Button("外网", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
		{
			FCDownloadManager.CurServerPostType = FCDownloadManager.ServerPostType.SPT_TEST;
			FCDownloadManager.URL_SERVER_ROOT = FCDownloadManager.URL_SERVER_ROOT_TEST;
			InJoy.UnityBuildSystem.BuildInfo.ServerPostTag = "test1";

			Application.LoadLevel("Boot");
			return;
		}

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

	}
#endif //DEVELOPMENT_BUILD

	void ClearSingletons()
	{
		Utils.DestroySingleton<NetworkManager>();
		Utils.DestroySingleton<FCDownloadManager>();
		Utils.DestroySingleton<UIMessageBoxManager>();
		Utils.DestroySingleton<AppLoading>();
		Utils.DestroySingleton<Localization>();
		Utils.DestroySingleton<CheatManager>();
		Utils.DestroySingleton<PhotonView>();
		Utils.DestroySingleton<LevelManager>();
		Utils.DestroySingleton<ParticleManager>();
		Utils.DestroySingleton<EquipmentAssembler>();
		Utils.DestroySingleton<BulletAssembler>();
		Utils.DestroySingleton<GameManager>();
		Utils.DestroySingleton<LoadingManager>();
		Utils.DestroySingleton<QuestManager>();
		Utils.DestroySingleton<DailyBonusManager>();
		
		//this should be the last
		Utils.DestroySingleton<GameLauncher>();
	}
}

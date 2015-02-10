using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This script automatically connects to Photon (using the settings file), 
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandom : Photon.MonoBehaviour
{
    public bool AutoConnect = false;
    public int GuiSpace = 0;
    private bool ConnectInUpdate = true;

	// Use this for initialization
    public virtual void Start ()
    {
        // TYPED LOBBY
        // you don't have to join a lobby, to make use of it. JoinRandom and CreateRoom have parameters lobbyName and lobbyType
        PhotonNetwork.autoJoinLobby = false;
    }

    void Update()
    {
        if (ConnectInUpdate && AutoConnect)
        {
            ConnectInUpdate = false;

            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.JoinRandomRoom();");
            PhotonNetwork.ConnectUsingSettings("1");
        }
    }

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        //PhotonNetwork.JoinRandomRoom();

        // TYPED LOBBY
        // Even if we didn't join the specified lobby, we can join random rooms by it. Below we defined a skill filter of less than 6.
        PhotonNetwork.JoinRandomRoom("myLobby", LobbyType.SqlLobby, "C0 > 1");
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.");
        //PhotonNetwork.CreateRoom(null, true, true, 4);

        // TYPED LOBBY
        // Currently, room-properties have to be named "C0" to "C9" to be available in the JoinRandomRoom filters (as they are matched to SQL Table columns)!
        // The first value assigned to any room-property will define the type it accepts. Assigning a string to "C0" will cause an error once this game is created.
        // "C0" in this demo can be considered a skill level for this room
        Hashtable roomProps = new Hashtable() {{"C0", 5}};

        // Properties you want to use in a lobby's filter need to be defined by parameter propsToListInLobby. Else they won't match
        string[] propsInLobby = new string[] {"C0"};
        PhotonNetwork.CreateRoom(null, true, true, 0, roomProps, propsInLobby, "myLobby", LobbyType.SqlLobby);
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : NetworkLobbyManager
{
    public static Lobby instance;

    public GameObject lobby;
    public GameObject select;
    public GameObject playerList;
    public Button backButton;
    public GameObject connectingDisplay;

    private void Start()
    {
        instance = this;
    }

    // lobby client

    // the lobby must be displayed when the user connects
    public override void OnLobbyClientConnect(NetworkConnection conn)
    {
        lobby.SetActive(true);
        select.SetActive(false);
    }

    // if the server is full, the client must be booted
    public override void OnLobbyClientAddPlayerFailed()
    {
        base.OnLobbyClientAddPlayerFailed();
        StopClient();
    }

    // hide/shows the lobby changing between game/lobby scene
    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        lobby.SetActive(SceneManager.GetActiveScene().name == lobbyScene);

        if (SceneManager.GetActiveScene().name == lobbyScene)
        {

            // needs to set the lobby values for each PLAYER
            foreach(var playerController in conn.playerControllers)
            {
                playerController.unetView.GetComponent<LobbyPlayer>().SetLobbyValues();
            }

            backButton.onClick.RemoveAllListeners();
            if (conn.playerControllers[0].unetView.isServer)
            {
                backButton.onClick.AddListener(StopHost);
            }
            else
            {
                backButton.onClick.AddListener(StopClient);
            }
        }
    }

    // client

    // sets the back button to stop client, closes the "connecting" popup
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        backButton.onClick.AddListener(StopClient);
        connectingDisplay.SetActive(false);
    }

    // sets the back button to stop host
    public override void OnStartHost()
    {
        base.OnStartHost();
        backButton.onClick.AddListener(StopHost);
    }

    // when a client disconnects, show the select screen
    public override void OnStopClient()
    {
        lobby.SetActive(false);
        select.SetActive(true);
    }

    // if the client disconnects (including cannot find a server) "connecting" popup is closed
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        connectingDisplay.SetActive(false);
    }

    // server

    // sets up the values for the gamePlayer based on the values of the lobbyPlayer
        //run on the server, values show be syncVars or updated with an RPC
    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        lobby.SetActive(false);
        select.SetActive(false);

        //bool ret = base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
        gamePlayer.GetComponent<Player>().PlayerType = lobbyPlayer.GetComponent<LobbyPlayer>().playerType;
        gamePlayer.GetComponent<Player>().isHost = lobbyPlayer.GetComponent<LobbyPlayer>().isHost;

        return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);

    }

    

    
}

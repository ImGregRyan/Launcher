using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using TMPro;
using RestSharp;
using System;
using Newtonsoft.Json;
using System.Linq;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> ClientList = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocalPlayer { get; private set; }

    public string IsPartyLeader { get; set; }
    public string PartyName = "";

    public bool isConnected;
    public string accountName;

    private void OnDestroy()
    {
        ClientList.Remove(Id);
    }

    [MessageHandler((ushort)ServerToClientId.SendPlayerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetBool(), message.GetVector3());
    }

    public static void Spawn(ushort id, string accountName, bool isConnected, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocalPlayer = true;

            NetworkManager.Singleton.LocalPlayer = player;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.IsLocalPlayer = false;
        }
        player.name = $"Player {id} ({accountName})";
        player.Id = id;
        player.accountName = accountName;
        player.isConnected = isConnected;

        ClientList.Add(id, player);

        FriendManager.PlayerJoined(player.Id);
        FriendManager.GetAllFriendsOrIgnored();
        FriendManager.GetAllRequests();

    }

}

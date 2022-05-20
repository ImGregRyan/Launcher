using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum ServerToClientId : ushort
{
    SendPlayerSpawned = 1,
    SendChatMessage,
    SendAlertMessage,
    SendUpdateRequests,
    SendPartyInvite,
    SendPartyList,
}

public enum ClientToServerId : ushort
{
    SendAccountInfo = 1,
    SendChatMessage,
    SendRequestAnswer,
    SendPartyInviteAnswer,
    SendRemoveFromParty,
    SendDisconnectMe,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }


    public static string apiUrl = "https://eb00-173-56-224-38.ngrok.io/";

    public Client Client { get; private set; }
    public Player LocalPlayer;

    [SerializeField] private string ip;
    [SerializeField] private ushort port;


    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        //Client.ClientConnected += PlayerJoined;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
        ClientData.Singleton.IsConnected = false;
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        ClientData.Singleton.IsConnected = true;
        MenuManager.Singleton.loginWaitMenu.SetActive(false);

        ClientData.Singleton.SendAccountInfo();

        MenuManager.Singleton.homeMenu.SetActive(true);
        MenuManager.Singleton.loginMenu.SetActive(false);
        MenuManager.Singleton.homeLoginMenu.SetActive(false);

    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        MenuManager.Singleton.loginWaitMenu.SetActive(false);

        MenuManager.Singleton.loginMenu.SetActive(true);
        MenuManager.Singleton.backgroundVideo.SetActive(true);

        MenuManager.Singleton.alertMenu.SetActive(true);
        MenuManager.Singleton.alertMenuText.text = "Could not connect to game server.";
    }
    private void PlayerJoined(object sender, ClientConnectedEventArgs e)
    {

        var ListToUse = FriendManager.FriendBoxes["OfflineFriends"];
        ushort serverClientId = e.Id;

        var accountName = FriendManager.GetAccountNameByClientId(serverClientId);
        var localClientPlayer = Player.ClientList.FirstOrDefault(x => x.Value.accountName == accountName).Value;


        // If they are in our offline list
        if (ListToUse.Any(r => r.name == accountName))
        {
            var objectToDestroy = ListToUse.FirstOrDefault(x => x.name == accountName).gameObject;
            ListToUse.Remove(objectToDestroy);
            Destroy(objectToDestroy);

            // Add to offline list
            FriendManager.Singleton.friendObject = Instantiate(FriendManager.Singleton.OnlineFriendPrefab, FriendManager.Singleton.OnlineFriendBox.transform);
            FriendManager.Singleton.friendObject.name = accountName;
            FriendManager.Singleton.friendObject.GetComponentInChildren<TMP_Text>().text = accountName;

            ListToUse = FriendManager.FriendBoxes["OnlineFriends"];
            ListToUse.Add(FriendManager.Singleton.friendObject);
        }
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        var ListToUse = FriendManager.FriendBoxes["OnlineFriends"];
        ushort serverClientId = e.Id;
        var accountName = FriendManager.GetAccountNameByClientId(serverClientId);
        var localClientPlayer = Player.ClientList.FirstOrDefault(x => x.Value.accountName == accountName).Value;

        if(localClientPlayer != null)
        {
            Destroy(localClientPlayer.gameObject);
        }

        // If they are in our online list
        if (ListToUse.Any(r => r.name == accountName))
        {
            var objectToDestroy = ListToUse.FirstOrDefault(x => x.name == accountName).gameObject;
            ListToUse.Remove(objectToDestroy);
            Destroy(objectToDestroy);

            // Add to offline list
            FriendManager.Singleton.friendObject = Instantiate(FriendManager.Singleton.OfflineFriendPrefab, FriendManager.Singleton.OfflineFriendBox.transform);
            FriendManager.Singleton.friendObject.name = accountName;
            FriendManager.Singleton.friendObject.GetComponentInChildren<TMP_Text>().text = accountName;

            ListToUse = FriendManager.FriendBoxes["OfflineFriends"];
            ListToUse.Add(FriendManager.Singleton.friendObject);

            FriendManager.Singleton.OfflineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Offline Friends({FriendManager.FriendBoxes["OfflineFriends"].Count})";

            FriendManager.Singleton.OnlineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Online Friends({FriendManager.FriendBoxes["OnlineFriends"].Count})";

        }
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        ClientData.Singleton.IsConnected = false;
        FriendManager.firstFriendListLoad = true;
        FriendManager.firstRequestListLoad = true;

        foreach (KeyValuePair<string, List<PartyPlayer>> partyPlayerObject in PartyMenu.PartyBoxes)
        {
            foreach (var playerObject in partyPlayerObject.Value)
            {
                Destroy(playerObject.gameObject);
            }
        }
        PartyMenu.PartyBoxes.Clear();
        MenuManager.Singleton.partyMenu.SetActive(false);
        ChatManager.Singleton.ChatBoxContainer.transform.Find("Global").gameObject.SetActive(true);

        foreach (KeyValuePair<ushort, Player> Client in Player.ClientList)
        {
            Destroy(Client.Value.gameObject);
        }

        foreach (KeyValuePair<string, List<GameObject>> ChatBox in ChatManager.ChatBoxes)
        {
            if(ChatBox.Key != "Global")
            {
                var chatWindow = ChatManager.Singleton.ChatBoxContainer.transform.Find(ChatBox.Key).gameObject;
                var anotherChatTab = ChatManager.Singleton.ChatTabContainer.transform.Find(ChatBox.Key).gameObject;
                
                Destroy(chatWindow);
                Destroy(anotherChatTab);
                
            }
            else if(ChatBox.Key == "Global")
            {
                var chatWindow = ChatManager.Singleton.ChatBoxContainer.transform.Find(ChatBox.Key).gameObject;
                foreach (GameObject messageObject in ChatBox.Value)
                {
                    Destroy(messageObject);
                }

                ChatBox.Value.Clear();
                chatWindow.SetActive(true);
            }
        }
        foreach (KeyValuePair<string, List<GameObject>> FriendBox in FriendManager.FriendBoxes)
        {
            foreach(GameObject friendObject in FriendBox.Value)
            {              
                Destroy (friendObject);
            }

            FriendBox.Value.Clear();
        }


        HomeLoginMenu.Singleton.GetSaveData();
        HomeMenu.Singleton._accountNameText.gameObject.SetActive(false);
        MenuManager.Singleton.homeLoginMenu.SetActive(true);
        MenuManager.Singleton.alertMenu.SetActive(true);
        MenuManager.Singleton.alertMenuText.text = "You have been disconnected.";

        FriendManager.Singleton.OfflineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Offline Friends({FriendManager.FriendBoxes["OfflineFriends"].Count})";

        FriendManager.Singleton.OnlineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Online Friends({FriendManager.FriendBoxes["OnlineFriends"].Count})";

        FriendManager.Singleton.IgnoreBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Ignored({FriendManager.FriendBoxes["Ignored"].Count})";

        FriendManager.Singleton.IncomingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Incoming({FriendManager.FriendBoxes["IncomingRequests"].Count})";

        FriendManager.Singleton.OutgoingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Outgoing({FriendManager.FriendBoxes["OutgoingRequests"].Count})";

    }
}
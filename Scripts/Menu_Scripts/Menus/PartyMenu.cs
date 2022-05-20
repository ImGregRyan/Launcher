using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PartyMenu : MonoBehaviour
{
    private static PartyMenu _singleton;
    public static PartyMenu Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(PartyMenu)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }


    public static Dictionary<string, List<PartyPlayer>> PartyBoxes = new Dictionary<string, List<PartyPlayer>>();



    [Header("Prefabs")]
    [SerializeField] private GameObject partyPrefab;
    [SerializeField] private GameObject partyBox;

    public GameObject PartyPrefab => partyPrefab;
    public GameObject PartyBox => partyBox;




    private static string partyName;



    [MessageHandler((ushort)ServerToClientId.SendPartyList)]
    private static void DealWithPartyList(Message message)
    {
        var clientAccountNameList = message.GetString();
        partyName = message.GetString();
        List<PartyPlayer> partyBox;

        NetworkManager.Singleton.LocalPlayer.PartyName = partyName;

        // Found old box
        if (PartyBoxes.ContainsKey(partyName))
        {
            partyBox = PartyBoxes[partyName];

            // Delete the gameobjects 
            foreach (PartyPlayer partyPlayerObject in PartyBoxes[partyName])
            {
                Destroy(partyPlayerObject.gameObject);
            }

            partyBox.Clear();
        }
        // Made a new box
        else
        {
            PartyBoxes.Add(partyName, new List<PartyPlayer>());
            partyBox = PartyBoxes[partyName];
        }

        // See if the party list is empty
        string[] AccountNames = clientAccountNameList.Split(',');

        // There is a party
        if(AccountNames.Length - 1 > 1)
        {
            MenuManager.Singleton.partyMenu.SetActive(true);
            ChatManager.Singleton.MakePartyChatWindow(partyName);
        }
        // Party has ended
        else
        {
            PartyBoxes.Remove(partyName);
            ChatManager.ChatBoxes.Remove(partyName);

            var partyChatBoxObject = ChatManager.Singleton.ChatBoxContainer.transform.Find(partyName).gameObject;
            Destroy(partyChatBoxObject);

            var partyChatTabObject = ChatManager.Singleton.ChatTabContainer.transform.Find(partyName).gameObject;
            Destroy(partyChatTabObject);

            MenuManager.Singleton.partyMenu.SetActive(false);
            ChatManager.Singleton.ChatBoxContainer.transform.Find("Global").gameObject.SetActive(true);
            return;
        }

        foreach (var AccountName in AccountNames)
        {
            if (AccountName == "")
            {
                continue;
            }

            var accountNameLeaderStatusConnectionStatus = AccountName.Split('-');

            var accountName = accountNameLeaderStatusConnectionStatus[0];
            var leaderStatus = accountNameLeaderStatusConnectionStatus[1];
            var connectionStatus = accountNameLeaderStatusConnectionStatus[2];

            if(accountName == NetworkManager.Singleton.LocalPlayer.accountName)
            {
                NetworkManager.Singleton.LocalPlayer.IsPartyLeader = leaderStatus;
            }
        }

        // Add the party player prefabs
        foreach (var AccountName in AccountNames)
        {
            if (AccountName == "")
            {
                continue;
            }

            var accountNameLeaderStatusConnectionStatus = AccountName.Split('-');

            var accountName = accountNameLeaderStatusConnectionStatus[0];
            var leaderStatus = accountNameLeaderStatusConnectionStatus[1];
            var connectionStatus = accountNameLeaderStatusConnectionStatus[2];

            var partyPlayer = Instantiate(Singleton.PartyPrefab, Singleton.PartyBox.transform).GetComponent<PartyPlayer>();

            // Local player is leader
            if (NetworkManager.Singleton.LocalPlayer.IsPartyLeader == "True")
            {
                partyPlayer.playerName = accountName;
                partyPlayer.name = accountName;
                partyPlayer.GetComponentInChildren<TMP_Text>().text = accountName;

                // If self
                if(accountName == NetworkManager.Singleton.LocalPlayer.accountName)
                {
                    partyPlayer.isPartyLeader = "True";
                    partyPlayer.partyLeaderImage.SetActive(true);
                }
                // If other
                else
                {
                    partyPlayer.removeButton.SetActive(true);
                }
            }
            // Local is not leader
            if (NetworkManager.Singleton.LocalPlayer.IsPartyLeader == "False" || NetworkManager.Singleton.LocalPlayer.IsPartyLeader == null)
            {
                partyPlayer.playerName = accountName;
                partyPlayer.name = accountName;
                partyPlayer.isPartyLeader = leaderStatus;
                partyPlayer.GetComponentInChildren<TMP_Text>().text = accountName;

                // If self
                if (leaderStatus == "True")
                {
                    partyPlayer.partyLeaderImage.SetActive(true);
                }
            }

            if(connectionStatus == "True")
            {
                partyPlayer.connectedImage.SetActive(true);
                partyPlayer.disconnectedImage.SetActive(false);
            }
            else
            {
                partyPlayer.connectedImage.SetActive(false);
                partyPlayer.disconnectedImage.SetActive(true);
            }
            partyBox.Add(partyPlayer);
        }

        //Sort request list by name
        if (partyBox.Count > 1)
        {
            PartyPlayer[] players = (
            from directChild in Singleton.PartyBox.GetComponentsInChildren<PartyPlayer>(true)
            where directChild.isPartyLeader != "True"
            select directChild).ToArray();

            var partyLeader = partyBox.FirstOrDefault(x => x.isPartyLeader == "True");
            partyLeader.transform.SetSiblingIndex(0);

            PartyPlayer[] playersInOrder = players.OrderBy(c => c.name).ToArray();

            for (int i = 0; i < playersInOrder.Length; i++)
            {
                playersInOrder[i].transform.SetSiblingIndex(i + 1);
            }

        }

    }

    public void LeaveButton()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendRemoveFromParty);
        message.AddUShort(NetworkManager.Singleton.LocalPlayer.Id);
        message.AddString(partyName);
        message.AddString("LEAVE");
        NetworkManager.Singleton.Client.Send(message);

        MenuManager.Singleton.partyMenu.SetActive(false);
    }
}

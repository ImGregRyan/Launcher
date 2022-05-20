using Newtonsoft.Json;
using RestSharp;
using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    private static ChatManager _singleton;
    public static ChatManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(ChatManager)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }

    public enum CommandName
    {
        FRIENDS,
        F,
        HELP,
        H,
        PARTY,
        P,
        PARTYADD,
        PA,
        PARTYLEADER,
        PL,
        PARTYQUIT,
        PQ,
        PARTYREMOVE,
        PR,
        IGNORE,
        IGNOREREMOVE,
        ROLL,
        WHISPER,
        W,
    }

    public static Dictionary<string, List<GameObject>> ChatBoxes = new Dictionary<string, List<GameObject>>();
    public static List<GameObject> ChatCommands = new List<GameObject>();


    [Header("Prefabs")]
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject globalChatBox;
    [SerializeField] private GameObject additionalChatBoxPrefab;
    [SerializeField] private GameObject tabCommandPrefab;
    [SerializeField] private GameObject chatBoxContainer;
    [SerializeField] private GameObject chatTabPrefab;
    [SerializeField] private GameObject chatTabContainer;
    [SerializeField] private GameObject tabCommandContainer;
    [SerializeField] private Sprite chatTabNotificationImage;
    public GameObject MessagePrefab => messagePrefab;
    public GameObject GlobalChatBox => globalChatBox;
    public GameObject AdditionalChatBoxPrefab => additionalChatBoxPrefab;
    public GameObject TabCommandPrefab => tabCommandPrefab;
    public GameObject ChatBoxContainer => chatBoxContainer;
    public GameObject ChatTabPrefab => chatTabPrefab;
    public GameObject ChatTabContainer => chatTabContainer;
    public GameObject TabCommandContainer => tabCommandContainer;
    public Sprite ChatTabNotificationImage => chatTabNotificationImage;


    public static int maxMessageListSize = 25;

    GameObject activeChatWindow;
    private static string fromAccountName;

    private void Start()
    {
        ChatBoxes.Add("Global", new List<GameObject>());
    }

    public void SendChatMessage()
    {
        fromAccountName = NetworkManager.Singleton.LocalPlayer.accountName;
        var messageToSend = HomeMenu.Singleton.messageInput.text;
        var messageType = "";
        GetActiveChatWindow();

        if (CheckForCommand(fromAccountName, messageToSend))
        {
            AddTabCommand();

            if (commandName == CommandName.HELP.ToString() || commandName == CommandName.H.ToString())
            {
                DealWithHelpMenu(fixedSubCommand);
                return;
            }
            if (commandName == CommandName.PARTY.ToString() || commandName == CommandName.P.ToString())
            {
                if(NetworkManager.Singleton.LocalPlayer.PartyName == null || NetworkManager.Singleton.LocalPlayer.PartyName == "")
                {
                    return;
                }
                messageToSend = $"/p {NetworkManager.Singleton.LocalPlayer.PartyName} {subCommand} {commandText}";
            }
            if (commandName == CommandName.WHISPER.ToString() || commandName == CommandName.W.ToString())
            {
                SendDatabaseWhisperMessage("WHISPER", fromAccountName, subCommandAccountName, commandText);
            }
        }

        if (!CheckForCommand(fromAccountName, messageToSend))
        {
            if (activeChatWindow.GetComponent<ChatBox>().privateWhisper == true)
            {
                messageType = "Whisper";
                GetNameFromChatBox(activeChatWindow.name);

                SendDatabaseWhisperMessage("WHISPER", fromAccountName, nameFromChatBox, messageToSend);

                messageToSend = $"/w {nameFromChatBox} {messageToSend}";
            }
            if (activeChatWindow.GetComponent<ChatBox>().partyChat == true)
            {
                messageType = "Party";
                GetNameFromChatBox($"FakeDataForFunction {activeChatWindow.name}");
                messageToSend = $"/p {nameFromChatBox} {messageToSend}";
            }
        }

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendChatMessage);
        message.AddString(fromAccountName);
        message.AddString(messageToSend);
        message.AddString(messageType);
        NetworkManager.Singleton.Client.Send(message);
    }
    public void SendTextCommandMessage(string chatCommandToSend, string messageType)
    {
        fromAccountName = NetworkManager.Singleton.LocalPlayer.accountName;

        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendChatMessage);
        message.AddString(fromAccountName);
        message.AddString(chatCommandToSend);
        message.AddString(messageType);
        NetworkManager.Singleton.Client.Send(message);
    }

    // Check chat message for command inputs
    string commandName;
    string commandNameAccountName;
    string subCommand;
    string subCommandAccountName;
    string fixedSubCommand;
    string commandText;
    string commandTextAccountName;
    string fixedCommandText;

    bool onePartCommand;
    bool twoPartCommand;

    private bool CheckForCommand(string fromAccountName, string chatMessage)
    {
        if (chatMessage.Trim().StartsWith("/"))
        {
            int lastLocation = chatMessage.IndexOf("/");

            if (lastLocation >= 0)
            {
                chatMessage = chatMessage.Substring(lastLocation + 1);// message minus the "/"

                // Split the remaining message into 3 parts, at the first two " ".
                string[] words = chatMessage.Split(new string[] { " " }, 3, StringSplitOptions.None);

                // Set the split to variables
                commandName = words[0].ToUpper();
                commandNameAccountName = char.ToUpper(commandName[0]) + commandName.Substring(1).ToLower();

                if (words.Length >= 2)
                {
                    subCommand = words[1];

                    if(subCommand != null && subCommand != "")
                    {
                        subCommandAccountName = char.ToUpper(subCommand[0]) + subCommand.Substring(1).ToLower();
                    }

                    fixedSubCommand = subCommand.ToUpper();
                }
                else
                { 
                    subCommand = null;
                    subCommandAccountName = null;
                    fixedSubCommand = null;
                }

                if (words.Length >= 3)
                {
                    commandText = words[2];

                    if (commandText != null && commandText != "")
                    {
                        commandTextAccountName = char.ToUpper(commandText[0]) + commandText.Substring(1).ToLower();
                    }

                    fixedCommandText = commandText.ToUpper();
                }
                else
                { 
                    commandText = null; 
                    commandTextAccountName = null;
                    fixedCommandText = null;
                }
                return true;
            }
            return true;
        }
        return false;
    }

    private void AddTabCommand()
    {
        GameObject tabCommand = null;

        if (commandName == CommandName.ROLL.ToString())
        {
            if (ChatCommands.Any(x => x.gameObject.name == $"/{commandNameAccountName}"))
            {
                return;
            }

            tabCommand = Instantiate(TabCommandPrefab, TabCommandContainer.transform);
            tabCommand.name = $"/{commandNameAccountName}";
            tabCommand.GetComponentInChildren<TMP_Text>().text = $"/{commandNameAccountName}";
        }

        if (commandName == CommandName.WHISPER.ToString() || commandName == CommandName.W.ToString())
        {
            if (ChatCommands.Any(x => x.gameObject.name == $"/{commandNameAccountName} {subCommandAccountName}"))
            {
                return;
            }

            tabCommand = Instantiate(TabCommandPrefab, TabCommandContainer.transform);
            tabCommand.name = $"/{commandNameAccountName} {subCommandAccountName}";
            tabCommand.GetComponentInChildren<TMP_Text>().text = $"/{commandNameAccountName} {subCommandAccountName}";
        }

        if (tabCommand != null)
        {
            ChatCommands.Add(tabCommand);

            if (ChatCommands.Count >= 5)
            {
                Destroy(ChatCommands[0].gameObject);
                ChatCommands.Remove(ChatCommands[0]);
            }
        }

    }


    /// <summary>
    /// Handle incoming chat messages 
    /// </summary>
    string chatBoxType;
    string nameFromChatBox;
    public static bool loadingChat;
    private void GetNameFromChatBox(string chatBoxName)
    {
        string[] words = chatBoxName.Split(new string[] { " " }, 2, StringSplitOptions.None);

        chatBoxType = words[0].ToUpper();
        nameFromChatBox = words[1];
    }

    [MessageHandler((ushort)ServerToClientId.SendChatMessage)]
    private static void ReceiveChatMessage(Message message)
    {
        var id = message.GetUShort();
        var fromAccountName = message.GetString();
        var _message = message.GetString();
        var messageType = message.GetString();

        var foundIgnored = false;

        foreach (var item in FriendManager.FriendBoxes["Ignored"])
        {
            if (item.name == fromAccountName)
            {
                foundIgnored = true;
            }
        }

        if (foundIgnored == false)
        {
            DisplayChatMessage(id, fromAccountName, _message, messageType);
        }
    }

    public static void DisplayChatMessage(ushort id, string fromAccountName, string message, string messageType)
    {
        // Filter by message type sent from the server
        if (messageType == "Whisper" || messageType == "ISentWhisper")
        {
            DealWithWhispers(fromAccountName, message, messageType);
        }

        if (messageType == "Global" || messageType == "Server")
        {
            DealWithGlobals(fromAccountName, message, messageType);
        }

        if (messageType == "Party")
        {
            DealWithPartyChat(fromAccountName, message, id);
        }
    }

    public static bool hadChatHistory;

    public static async void DealWithWhispers(string otherAccountName, string message, string messageType)
    {
        GameObject newMessage;
        GameObject chatBoxFound;
        GameObject chatTab;
        string chatBoxName = $"WHISPER {otherAccountName}";
        List<GameObject> ListToUse;
        bool chatExists = ChatBoxes.ContainsKey(chatBoxName);
        hadChatHistory = false;

        // Check for message List in the Dictionary that matches
        if (chatExists)
        {
            ListToUse = ChatBoxes[chatBoxName];
        }
        else// If we didnt find a List, we make that shit
        {
            ChatBoxes.Add(chatBoxName, new List<GameObject>());
            ListToUse = ChatBoxes[chatBoxName];
        }

        // Check for the Message container in unity
        if (Singleton.ChatBoxContainer.transform.Find(chatBoxName))
        {
            chatBoxFound = Singleton.ChatBoxContainer.transform.Find(chatBoxName).gameObject;
        }
        else // If we didnt find a chat box, we make that shit
        {
            // Every chat box needs a navigation tab
            chatTab = Instantiate(Singleton.ChatTabPrefab, Singleton.ChatTabContainer.transform);
            chatTab.name = chatBoxName;
            chatTab.GetComponentInChildren<TMP_Text>().text = otherAccountName;

            // Make new chat box
            chatBoxFound = Instantiate(Singleton.AdditionalChatBoxPrefab, Singleton.ChatBoxContainer.transform);
            chatBoxFound.name = chatBoxName;
            chatBoxFound.GetComponent<ChatBox>().privateWhisper = true;

            chatBoxFound.SetActive(false);
            Singleton.GlobalChatBox.SetActive(true);

            await GetChatHistory("WHISPER", otherAccountName, ListToUse, chatBoxFound);

            if (!chatBoxFound.activeInHierarchy && !Singleton.GlobalChatBox.activeInHierarchy)
            {
                chatTab = Singleton.ChatTabContainer.transform.Find(chatBoxName).gameObject;

                chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
            }

            if (SettingsMenu.showWhispersInGlobalStatus == false && !chatBoxFound.activeInHierarchy)
            {
                chatTab = Singleton.ChatTabContainer.transform.Find(chatBoxName).gameObject;

                chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
            }
        }

        // Found our chatbox, time to add message
        if(hadChatHistory == false)
        {
            newMessage = Instantiate(Singleton.MessagePrefab, chatBoxFound.transform.Find("Viewport/Content"));
            newMessage.GetComponent<ClickableText>().otherAccountName = otherAccountName;
            FinishWhisper(newMessage, messageType, otherAccountName, message, ListToUse);
        }

        // If we want global whispers
        if (SettingsMenu.showWhispersInGlobalStatus == true)
        {
            var anotherListToUse = ChatBoxes["Global"];
            var anotherNewMessage = Instantiate(Singleton.MessagePrefab, Singleton.GlobalChatBox.transform);
            anotherNewMessage.GetComponent<ClickableText>().otherAccountName = otherAccountName;
            FinishWhisper(anotherNewMessage, messageType, otherAccountName, message, anotherListToUse);
        }

        // Flash a notification image if neither global or private chatbox are active
        if(!chatBoxFound.activeInHierarchy && !Singleton.GlobalChatBox.activeInHierarchy)
        {
            chatTab = Singleton.ChatTabContainer.transform.Find(chatBoxName).gameObject;

            chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
        }

        if(SettingsMenu.showWhispersInGlobalStatus == false && !chatBoxFound.activeInHierarchy)
        {
            chatTab = Singleton.ChatTabContainer.transform.Find(chatBoxName).gameObject;

            chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
        }

    }

    public static void FinishWhisper(GameObject newMessage, string messageType, string otherAccountName, string message, List<GameObject> ListToUse)
    {
        newMessage.GetComponentInChildren<TMP_Text>().color = Color.cyan;
        // Make to/from messages display differently so it reads better
        if (messageType == "Whisper")
        {
            if (SettingsMenu.timeStampStatus == true)
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"({HomeMenu.currentTime}) [<link=accountName>{otherAccountName}</link>]: {message}";
            }
            else
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"[<link=accountName>{otherAccountName}</link>]: {message}";
            }
        }
        else // Otherwise it would be "ISentWhisper"
        {
            if (SettingsMenu.timeStampStatus == true)
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"({HomeMenu.currentTime}) To [<link=accountName>{otherAccountName}</link>]: {message}";
            }
            else
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"To [<link=accountName>{otherAccountName}</link>]: {message}";
            }
        }

        ListToUse.Add(newMessage);

        if (ListToUse.Count >= maxMessageListSize)
        {
            Destroy(ListToUse[0].gameObject);
            ListToUse.Remove(ListToUse[0]);
        }
    }
    public static void LoadWhispers(string time, GameObject newMessage, string messageType, string otherAccountName, string message, List<GameObject> ListToUse)
    {
        newMessage.GetComponentInChildren<TMP_Text>().color = Color.cyan;
        // Make to/from messages display differently so it reads better
        if (messageType == "Whisper")
        {
            if (SettingsMenu.timeStampStatus == true)
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"({time}) [<link=accountName>{otherAccountName}</link>]: {message}";
            }
            else
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"[<link=accountName>{otherAccountName}</link>]: {message}";
            }
        }
        else // Otherwise it would be "ISentWhisper"
        {
            if (SettingsMenu.timeStampStatus == true)
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"({time}) To [<link=accountName>{otherAccountName}</link>]: {message}";
            }
            else
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"To [<link=accountName>{otherAccountName}</link>]: {message}";
            }
        }

        ListToUse.Add(newMessage);

        if (ListToUse.Count >= maxMessageListSize)
        {
           // Destroy(ListToUse[0].gameObject);
            Destroy(ListToUse[0]);
            ListToUse.Remove(ListToUse[0]);
        }
    }
    public static void DealWithGlobals(string accountName, string message, string messageType)
    {
        GameObject newMessage;
        List<GameObject> ListToUse = ChatBoxes["Global"];
        newMessage = Instantiate(Singleton.MessagePrefab, Singleton.GlobalChatBox.transform);
        newMessage.GetComponent<ClickableText>().otherAccountName = accountName;

        if (messageType == "Global")
        {
            newMessage.GetComponentInChildren<TMP_Text>().color = Color.white;
            if (SettingsMenu.timeStampStatus == true)
            {
                newMessage.GetComponentInChildren<TMP_Text>().text =
                $"({HomeMenu.currentTime}) [{messageType}] [<link=accountName>{accountName}</link>]: {message}";
            }
            else
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"[{messageType}] [<link=accountName>{accountName}</link>]: {message}";
            }
        }
        else //message type would be "Server"
        {
            newMessage.GetComponentInChildren<TMP_Text>().color = Color.yellow;
            if (SettingsMenu.timeStampStatus == true)
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"({HomeMenu.currentTime}) [{accountName}]: {message}";
            }
            else
            {
                newMessage.GetComponentInChildren<TMP_Text>().text = $"[{accountName}]: {message}";
            }
        }

        ListToUse.Add(newMessage);

        if (ListToUse.Count >= maxMessageListSize)
        {
            Destroy(ListToUse[0].gameObject);
            ListToUse.Remove(ListToUse[0]);
        }
    }

    public static void DealWithPartyChat(string fromAccountName, string message, ushort fromClientId)
    {
        var fromClient = Player.ClientList[fromClientId];

        GameObject newMessage;
        GameObject chatBoxFound;
        GameObject chatTab;
        List<GameObject> ListToUse;
        bool chatExists = ChatBoxes.ContainsKey(NetworkManager.Singleton.LocalPlayer.PartyName);


        // Check for message List in the Dictionary that matches
        if (chatExists)
        {
            ListToUse = ChatBoxes[NetworkManager.Singleton.LocalPlayer.PartyName];
        }
        else// If we didnt find a List, we make that shit
        {
            ChatBoxes.Add(NetworkManager.Singleton.LocalPlayer.PartyName, new List<GameObject>());
            ListToUse = ChatBoxes[NetworkManager.Singleton.LocalPlayer.PartyName];
        }

        // Check for the Message container in unity
        if (Singleton.ChatBoxContainer.transform.Find(NetworkManager.Singleton.LocalPlayer.PartyName))
        {
            chatBoxFound = Singleton.ChatBoxContainer.transform.Find(NetworkManager.Singleton.LocalPlayer.PartyName).gameObject;
        }
        else // If we didnt find a chat box, we make that shit
        {
            // Every chat box needs a navigation tab
            chatTab = Instantiate(Singleton.ChatTabPrefab, Singleton.ChatTabContainer.transform);
            chatTab.name = NetworkManager.Singleton.LocalPlayer.PartyName;
            chatTab.GetComponentInChildren<TMP_Text>().text = "Party";

            // Make new chat box
            chatBoxFound = Instantiate(Singleton.AdditionalChatBoxPrefab, Singleton.ChatBoxContainer.transform);
            chatBoxFound.name = NetworkManager.Singleton.LocalPlayer.PartyName;
            chatBoxFound.GetComponent<ChatBox>().partyChat = true;

            chatBoxFound.SetActive(false);
            Singleton.GlobalChatBox.SetActive(true);

            if (!chatBoxFound.activeInHierarchy && !Singleton.GlobalChatBox.activeInHierarchy)
            {
                chatTab = Singleton.ChatTabContainer.transform.Find(NetworkManager.Singleton.LocalPlayer.PartyName).gameObject;

                chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
            }

            if (SettingsMenu.showWhispersInGlobalStatus == false && !chatBoxFound.activeInHierarchy)
            {
                chatTab = Singleton.ChatTabContainer.transform.Find(NetworkManager.Singleton.LocalPlayer.PartyName).gameObject;

                chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
            }
        }

        // Found our chatbox, time to add message
        newMessage = Instantiate(Singleton.MessagePrefab, chatBoxFound.transform.Find("Viewport/Content"));
        newMessage.GetComponent<ClickableText>().otherAccountName = fromAccountName;
        FinishPartyChat(newMessage, fromClient.accountName, message, ListToUse);


        // If we want global whispers
        if (SettingsMenu.showWhispersInGlobalStatus == true)
        {
            var anotherListToUse = ChatBoxes["Global"];
            var anotherNewMessage = Instantiate(Singleton.MessagePrefab, Singleton.GlobalChatBox.transform);
            anotherNewMessage.GetComponent<ClickableText>().otherAccountName = fromAccountName;
            FinishPartyChat(anotherNewMessage, fromClient.accountName, message, anotherListToUse);
        }

        // Flash a notification image if neither global or private chatbox are active
        if (!chatBoxFound.activeInHierarchy && !Singleton.GlobalChatBox.activeInHierarchy)
        {
            chatTab = Singleton.ChatTabContainer.transform.Find(NetworkManager.Singleton.LocalPlayer.PartyName).gameObject;

            chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
        }

        if (SettingsMenu.showWhispersInGlobalStatus == false && !chatBoxFound.activeInHierarchy)
        {
            chatTab = Singleton.ChatTabContainer.transform.Find(NetworkManager.Singleton.LocalPlayer.PartyName).gameObject;

            chatTab.GetComponent<ButtonBehaviour>().activeNotification = true;
        }

    }
    public static void FinishPartyChat(GameObject newMessage, string toAccountName, string message, List<GameObject> ListToUse)
    {
        newMessage.GetComponentInChildren<TMP_Text>().color = Color.green;

        if (SettingsMenu.timeStampStatus == true)
        {
            newMessage.GetComponentInChildren<TMP_Text>().text = $"({HomeMenu.currentTime}) [Party] [<link=accountName>{toAccountName}</link>]: {message}";
        }
        else
        {
            newMessage.GetComponentInChildren<TMP_Text>().text = $"[Party] [<link=accountName>{toAccountName}</link>]: {message}";
        }
        

        ListToUse.Add(newMessage);

        if (ListToUse.Count >= maxMessageListSize)
        {
            Destroy(ListToUse[0].gameObject);
            ListToUse.Remove(ListToUse[0]);
        }
    }

    public static void DealWithHelpMenu(string fixedSubCommand)
    {
        var helpMenuMessage = "Help command not found.";
        // was just /help
        if (fixedSubCommand == null || fixedSubCommand == "")
        {
            helpMenuMessage =
                $"\n /Help Commands" +
                $"\n /Help Friends" +
                $"\n /Help Ignore" +
                $"\n /Help Party" +
                $"\n /Help Whisper";

        }
        if (fixedSubCommand == "COMMANDS")
        {
            helpMenuMessage =
            $"\n /Roll";
        }
        if (fixedSubCommand == "FRIENDS")
        {
            helpMenuMessage =
            $"\n /Friends Add AccountName" +
            $"\n /Friends Remove AccountName";
        }
        if (fixedSubCommand == "IGNORE")
        {
            helpMenuMessage =
            $"\n /Ignore AccountName" +
            $"\n /IgnoreRemove AccountName";
        }
        if (fixedSubCommand == "PARTY")
        {
            helpMenuMessage =
            $"\n /Party (Your chat message.)" +
            $"\n /PartyAdd AccountName" +
            $"\n /PartyRemove AccountName" +
            $"\n /PartyLeader AccountName" +
            $"\n /PartyLeave";
        }
        if (fixedSubCommand == "WHISPER")
        {
            helpMenuMessage =
            $"\n /Whisper AccountName (Your chat message.)";
        }

        DealWithGlobals("Server", helpMenuMessage, "Server");
    }

    public void GetActiveChatWindow()
    {
        for (int i = 0; i < ChatBoxContainer.transform.childCount; i++)
        {
            if (ChatBoxContainer.transform.GetChild(i).gameObject.activeSelf == true)
            {
                activeChatWindow = ChatBoxContainer.transform.GetChild(i).gameObject;
            }
        }
    }

    public void MakePartyChatWindow(string partyName)
    {
        GameObject chatBoxFound;
        GameObject chatTab;

        bool chatExists = ChatBoxes.ContainsKey(partyName);

        // Check for message List in the Dictionary that matches
        if (!chatExists)
        {
            ChatBoxes.Add(partyName, new List<GameObject>());
        }

        if (!Singleton.ChatBoxContainer.transform.Find(partyName))
        {
            //chatBoxFound = Singleton.ChatBoxContainer.transform.Find(partyName).gameObject;

            // Every chat box needs a navigation tab
            chatTab = Instantiate(Singleton.ChatTabPrefab, Singleton.ChatTabContainer.transform);
            chatTab.name = partyName;
            chatTab.GetComponentInChildren<TMP_Text>().text = "Party";

            // Make new chat box
            chatBoxFound = Instantiate(Singleton.AdditionalChatBoxPrefab, Singleton.ChatBoxContainer.transform);
            chatBoxFound.name = partyName;
            chatBoxFound.GetComponent<ChatBox>().partyChat = true;

            chatBoxFound.SetActive(false);
            Singleton.GlobalChatBox.SetActive(true);

        }
    }

    /// <summary>
    /// Database calls
    /// </summary>

    public static async void SendDatabaseWhisperMessage(string messageType, string fromAccountName, string toAccountName, string messageText)
    {
        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/SendDatabaseWhisperMessage", Method.Post);

            request.AddBody(new ChatMessageModel
            {
                MessageType = messageType,
                FromAccountName = fromAccountName,
                ToAccountName = toAccountName,
                Message = messageText,
            });
            RestResponse response = await client.ExecuteAsync(request);

            if (response.ErrorException != null)
            {
                const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                var MyException = new ApplicationException(errmsg, response.ErrorException);
                throw MyException;
            }
            if (response.Content == "-1")
            {
                Debug.Log("Error sending register request.");
            }
            if (response.Content == "1")
            {

            }

        }
        catch (Exception)
        {
            return;
        }
    }

    // Called on chatbox creation
    public static async Task GetChatHistory(string messageType, string toAccountName, List<GameObject> ListToUse, GameObject chatBoxFound)
    {
        GameObject newMessage;
        fromAccountName = NetworkManager.Singleton.LocalPlayer.accountName;
        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/GetChatHistory", Method.Post);

            request.AddBody(new ChatMessageModel
            {
                MessageType = messageType,
                FromAccountName = fromAccountName,
                ToAccountName = toAccountName
            });
            RestResponse response = await client.ExecuteAsync(request);
            var content = JsonConvert.DeserializeObject<List<ChatMessageModel>>(response.Content);

            if (response.ErrorException != null)
            {
                const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                var MyException = new ApplicationException(errmsg, response.ErrorException);
                throw MyException;
            }
            if (response.Content == "-1")
            {
                Debug.Log("Error sending register request.");
            }
            if (response.Content == "1")
            {
                Debug.Log("Chat message added.");
            }


            if (content.Count >= 1)
            {
                hadChatHistory = true;
                // Sort list by message creation date
                ChatMessageModel[] messagesInOrder = content.OrderBy(d => d.MessageTime).ToArray();

                foreach (var whisperMessage in messagesInOrder)
                {
                    newMessage = Instantiate(Singleton.MessagePrefab, chatBoxFound.transform.Find("Viewport/Content"));

                    var messageTime = whisperMessage.MessageTime.ToString("h:mm tt");

                    if (whisperMessage.FromAccountName == ClientData.Singleton.AccountName)
                    {
                        //LoadWhispers(messageTime, newMessage, "ISentWhisper", whisperMessage.ToAccountName, $"To [{whisperMessage.ToAccountName}]: {whisperMessage.Message}", ListToUse);
                        LoadWhispers(messageTime, newMessage, "ISentWhisper", whisperMessage.ToAccountName, whisperMessage.Message, ListToUse);
                    }
                    else
                    {
                        LoadWhispers(messageTime, newMessage, "Whisper", whisperMessage.FromAccountName, whisperMessage.Message, ListToUse);
                    }
                }
            }
        }
        catch (Exception)
        {
            return;
            
        }      
    }
}

public class ChatMessageModel
{
#nullable enable
    public int? MessageId { get; set; }
    public string? MessageType { get; set; }
    public string? FromAccountName { get; set; }
    public string? ToAccountName { get; set; }
    public string? Message { get; set; }
    public DateTime MessageTime { get; set; }
    public int? ChatGroupId { get; set; }
}
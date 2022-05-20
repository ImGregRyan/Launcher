using Newtonsoft.Json;
using RestSharp;
using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FriendManager : MonoBehaviour
{
    private static FriendManager _singleton;
    public static FriendManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(FriendManager)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }

    public static Dictionary<string, List<GameObject>> FriendBoxes = new Dictionary<string, List<GameObject>>();


    [Header("Prefabs")]
    [SerializeField] private GameObject onlineFriendPrefab;
    [SerializeField] private GameObject offlineFriendPrefab;
    [SerializeField] private GameObject onlineFriendBox;
    [SerializeField] private GameObject offlineFriendBox;
    [SerializeField] private GameObject outgoingRequestPrefab;
    [SerializeField] private GameObject incomingRequestPrefab;
    [SerializeField] private GameObject outgoingRequestBox;
    [SerializeField] private GameObject incomingRequestBox;
    [SerializeField] private GameObject friendBoxContainer;
    [SerializeField] private GameObject requestBoxContainer;
    [SerializeField] private GameObject ignorePrefab;
    [SerializeField] private GameObject ignoreBox;
    [SerializeField] private GameObject friendTab;
    [SerializeField] private GameObject requestTab;
    public GameObject OnlineFriendPrefab => onlineFriendPrefab;
    public GameObject OfflineFriendPrefab => offlineFriendPrefab;
    public GameObject OnlineFriendBox => onlineFriendBox;
    public GameObject OfflineFriendBox => offlineFriendBox;
    public GameObject OutgoingRequestPrefab => outgoingRequestPrefab;
    public GameObject IncomingRequestPrefab => incomingRequestPrefab;
    public GameObject OutgoingRequestBox => outgoingRequestBox;
    public GameObject IncomingRequestBox => incomingRequestBox;
    public GameObject FriendBoxContainer => friendBoxContainer;
    public GameObject RequestBoxContainer => requestBoxContainer;
    public GameObject IgnorePrefab => ignorePrefab;
    public GameObject IgnoreBox => ignoreBox;
    public GameObject FriendTab => friendTab;
    public GameObject RequestTab => requestTab;

    public GameObject requestObject;
    public GameObject friendObject;

    public static int maxMessageListSize = 25;

    public static bool firstFriendListLoad = true;
    public static bool firstRequestListLoad = true;

    private void Start()
    {
        // Make Friends lists
        FriendBoxes.Add("OnlineFriends", new List<GameObject>());
        FriendBoxes.Add("OfflineFriends", new List<GameObject>());
        FriendBoxes.Add("Ignored", new List<GameObject>());
        // Make request lists
        FriendBoxes.Add("IncomingRequests", new List<GameObject>());
        FriendBoxes.Add("OutgoingRequests", new List<GameObject>());
    }

    public static async void GetAllRequests()
    {
        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/GetAllRequests", Method.Post);

            request.AddBody(new RequestModel
            {
                FromAccountName = ClientData.Singleton.AccountName,
            });
            RestResponse response = await client.ExecuteAsync(request);
            var content = JsonConvert.DeserializeObject<List<RequestModel>>(response.Content);

            if (response.ErrorException != null)
            {
                const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                var MyException = new ApplicationException(errmsg, response.ErrorException);
                throw MyException;
            }
            if(response.Content == "[]")
            {
                foreach (GameObject requestObject in FriendBoxes["IncomingRequests"])
                {
                    Destroy(requestObject);
                }
                foreach (GameObject requestObject in FriendBoxes["OutgoingRequests"])
                {
                    Destroy(requestObject);
                }
                FriendBoxes["IncomingRequests"].Clear();
                FriendBoxes["OutgoingRequests"].Clear();

                firstRequestListLoad = false;

                Singleton.IncomingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                    $"Incoming({FriendBoxes["IncomingRequests"].Count})";

                Singleton.OutgoingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                    $"Outgoing({FriendBoxes["OutgoingRequests"].Count})";
            }
            if (response.Content != "[]")
            {
                RemoveUselessRequestObjects(content);

                foreach (var _request in content)
                {
                    if (_request.RequestType == "FRIEND")
                    {
                        List<GameObject> ListToUse = new List<GameObject>();

                        if (_request.ToAccountName == ClientData.Singleton.AccountName)
                        {
                            ListToUse = FriendBoxes["IncomingRequests"];

                            if (!ListToUse.Any(r => r.name == $"{_request.RequestType} {_request.FromAccountName}"))
                            {
                                Singleton.requestObject = Instantiate(Singleton.IncomingRequestPrefab, Singleton.IncomingRequestBox.transform);
                                Singleton.requestObject.name = $"{_request.RequestType} {_request.FromAccountName}";
                                Singleton.requestObject.GetComponentInChildren<TMP_Text>().text = _request.FromAccountName;

                                ListToUse.Add(Singleton.requestObject);
                                SetRequestInfo(Singleton.requestObject, _request, "IncomingRequest");

                                //if (!Singleton.IncomingRequestBox.activeInHierarchy && firstRequestListLoad == false)
                                if (!Singleton.RequestBoxContainer.activeInHierarchy && firstRequestListLoad == false)
                                {
                                    Singleton.RequestTab.GetComponent<ButtonBehaviour>().activeNotification = true;
                                }

                                //Singleton.IncomingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                                //    $"Incoming({FriendBoxes["IncomingRequests"].Count})";

                            }

                            //Sort request list by name
                            if (ListToUse.Count > 1)
                            {
                                IncomingRequest[] requests = (
                                from directChild in Singleton.IncomingRequestBox.GetComponentsInChildren<IncomingRequest>(true)
                                where directChild.transform.name != "TitleButton"
                                select directChild).ToArray();

                                IncomingRequest[] requestsInOrder = requests.OrderBy(c => c.name).ToArray();

                                for (int i = 0; i < requestsInOrder.Length; i++)
                                {
                                    requestsInOrder[i].transform.SetSiblingIndex(i + 1);
                                }
                            }
                        }
                        if (_request.FromAccountName == ClientData.Singleton.AccountName)
                        {
                            ListToUse = FriendBoxes["OutgoingRequests"];

                            if (!ListToUse.Any(r => r.name == $"{_request.RequestType} {_request.ToAccountName}"))
                            {
                                Singleton.requestObject = Instantiate(Singleton.OutgoingRequestPrefab, Singleton.OutgoingRequestBox.transform);
                                Singleton.requestObject.name = $"{_request.RequestType} {_request.ToAccountName}";
                                Singleton.requestObject.GetComponentInChildren<TMP_Text>().text = _request.ToAccountName;

                                ListToUse.Add(Singleton.requestObject);
                                SetRequestInfo(Singleton.requestObject, _request, "OutgoingRequest");

                                //Singleton.OutgoingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                                //    $"Outgoing({FriendBoxes["OutgoingRequests"].Count})";
                            }

                            //Sort request list by name
                            if (ListToUse.Count > 1)
                            {
                                OutgoingRequest[] requests = (
                                from directChild in Singleton.OutgoingRequestBox.GetComponentsInChildren<OutgoingRequest>(true)
                                where directChild.transform.name != "TitleButton"
                                select directChild).ToArray();

                                OutgoingRequest[] requestsInOrder = requests.OrderBy(c => c.name).ToArray();

                                for (int i = 0; i < requestsInOrder.Length; i++)
                                {
                                    requestsInOrder[i].transform.SetSiblingIndex(i + 1);
                                }
                            }
                        }
                    }
                }
            }

            Singleton.IncomingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Incoming({FriendBoxes["IncomingRequests"].Count})";

            Singleton.OutgoingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Outgoing({FriendBoxes["OutgoingRequests"].Count})";


            firstRequestListLoad = false;
        }
        catch (Exception ex)
        {
            throw ex;
        }


    }
    public static async void GetAllFriendsOrIgnored()
    {
        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/GetAllFriendsOrIgnored", Method.Post);

            request.AddBody(new FriendsOrIgnoredModel
            {
                FirstAccountName = ClientData.Singleton.AccountName,
            });
            RestResponse response = await client.ExecuteAsync(request);
            var content = JsonConvert.DeserializeObject<List<FriendsOrIgnoredModel>>(response.Content);

            if (response.ErrorException != null)
            {
                const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                var MyException = new ApplicationException(errmsg, response.ErrorException);
                throw MyException;
            }
            if (response.Content == "[]")
            {
                foreach (GameObject requestObject in FriendBoxes["OnlineFriends"])
                {
                    Destroy(requestObject);
                }
                foreach (GameObject requestObject in FriendBoxes["OfflineFriends"])
                {
                    Destroy(requestObject);
                }
                foreach (GameObject requestObject in FriendBoxes["Ignored"])
                {
                    Destroy(requestObject);
                }

                FriendBoxes["OnlineFriends"].Clear();
                FriendBoxes["OfflineFriends"].Clear();
                FriendBoxes["Ignored"].Clear();

                firstFriendListLoad = false;
            }
            if (response.Content != "[]")
            {
                RemoveUselessFriendsOrIgnoredObjects(content);

                foreach (var _friendship in content)
                {
                    if (_friendship.IsFriend == true)
                    {
                        ushort friendIsOnline = 0; // Friend not found on client list
                        friendIsOnline = GetClientByDatabaseName(_friendship.SecondAccountName);

                        if (friendIsOnline == 0)
                        {
                            List<GameObject> ListToUse = new List<GameObject>();
                            ListToUse = FriendBoxes["OfflineFriends"];

                            if (!ListToUse.Any(r => r.name == $"{_friendship.SecondAccountName}"))
                            {
                                Singleton.friendObject = Instantiate(Singleton.OfflineFriendPrefab, Singleton.OfflineFriendBox.transform);
                                Singleton.friendObject.name = $"{_friendship.SecondAccountName}";
                                Singleton.friendObject.GetComponentInChildren<TMP_Text>().text = _friendship.SecondAccountName;

                                ListToUse.Add(Singleton.friendObject);
                            }

                            //Sort friend list by name
                            if (ListToUse.Count > 1)
                            {
                                OfflineFriends[] friends = (
                                from directChild in Singleton.OfflineFriendBox.GetComponentsInChildren<OfflineFriends>(true)
                                where directChild.transform.name != "TitleButton"
                                select directChild).ToArray();

                                OfflineFriends[] friendsInOrder = friends.OrderBy(c => c.name).ToArray();

                                for (int i = 0; i < friendsInOrder.Length; i++)
                                {
                                    friendsInOrder[i].transform.SetSiblingIndex(i + 1);
                                }
                            }
                        }
                        if (friendIsOnline != 0)
                        {
                            List<GameObject> ListToUse = new List<GameObject>();
                            ListToUse = FriendBoxes["OnlineFriends"];

                            if (!ListToUse.Any(r => r.name == $"{_friendship.SecondAccountName}"))
                            {
                                Singleton.friendObject = Instantiate(Singleton.OnlineFriendPrefab, Singleton.OnlineFriendBox.transform);
                                Singleton.friendObject.name = $"{_friendship.SecondAccountName}";
                                Singleton.friendObject.GetComponentInChildren<TMP_Text>().text = _friendship.SecondAccountName;

                                ListToUse.Add(Singleton.friendObject);

                                if (!Singleton.OnlineFriendBox.activeInHierarchy && firstFriendListLoad == false)
                                {
                                    Singleton.FriendTab.GetComponent<ButtonBehaviour>().activeNotification = true;
                                }
                            }

                            //Sort friend list by name
                            if (ListToUse.Count > 1)
                            {
                                OnlineFriends[] friends = (
                                    from directChild in Singleton.OnlineFriendBox.GetComponentsInChildren<OnlineFriends>(true)
                                    where directChild.transform.name != "TitleButton"
                                    select directChild).ToArray();

                                OnlineFriends[] friendsInOrder = friends.OrderBy(c => c.name).ToArray();

                                for (int i = 0; i < friendsInOrder.Length; i++)
                                {
                                    friendsInOrder[i].transform.SetSiblingIndex(i + 1);
                                }
                            }
                        }
                    }
                    if (_friendship.IsIgnored == true)
                    {
                        List<GameObject> ListToUse = new List<GameObject>();
                        ListToUse = FriendBoxes["Ignored"];

                        if (!ListToUse.Any(r => r.name == $"{_friendship.SecondAccountName}"))
                        {
                            Singleton.friendObject = Instantiate(Singleton.IgnorePrefab, Singleton.IgnoreBox.transform);
                            Singleton.friendObject.name = $"{_friendship.SecondAccountName}";
                            Singleton.friendObject.GetComponentInChildren<TMP_Text>().text = _friendship.SecondAccountName;
                            Singleton.friendObject.GetComponent<Ignored>().secondAccountName = _friendship.SecondAccountName;

                            ListToUse.Add(Singleton.friendObject);
                        }

                        //Sort ignore list by name
                        if (ListToUse.Count > 1)
                        {
                            Ignored[] ignores = (
                                from directChild in Singleton.IgnoreBox.GetComponentsInChildren<Ignored>(true)
                                where directChild.transform.name != "TitleButton"
                                select directChild).ToArray();

                            Ignored[] ignoresInOrder = ignores.OrderBy(c => c.name).ToArray();

                            for (int i = 0; i < ignoresInOrder.Length; i++)
                            {
                                ignoresInOrder[i].transform.SetSiblingIndex(i + 1);
                            }
                        }
                    }
                }
            }
            Singleton.OfflineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Offline Friends({FriendBoxes["OfflineFriends"].Count})";

            Singleton.OnlineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Online Friends({FriendBoxes["OnlineFriends"].Count})";

            Singleton.IgnoreBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Ignored({FriendBoxes["Ignored"].Count})";

            firstFriendListLoad = false;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public static void RemoveUselessRequestObjects(List<RequestModel> content)
    {
        foreach (GameObject requestObject in FriendBoxes["IncomingRequests"])
        {
            bool requestFound = false;

            foreach (var _request in content)
            {

                // If we didnt already find our request and if we are getting the request
                if (_request.RequestType == "FRIEND" && _request.ToAccountName == ClientData.Singleton.AccountName)
                {
                    // If any of the request objects match our request name
                    if (requestObject.name == $"{_request.RequestType} {_request.FromAccountName}")
                    {
                        requestFound = true;
                    }
                }
            }

            // Person is no longer a friend, destroy object and remove from list
            if (requestFound == false)
            {
                Destroy(requestObject); // Gets removed from list using OnDestroy trigger
            }
        }

        foreach (GameObject requestObject in FriendBoxes["OutgoingRequests"])
        {
            bool requestFound = false;

            foreach (var _request in content)
            {
                // If we didnt already find our request and if we are sending the request
                if (_request.RequestType == "FRIEND" && _request.FromAccountName == ClientData.Singleton.AccountName)
                {
                    // If any of the request objects match our request name
                    if (requestObject.name == $"{_request.RequestType} {_request.ToAccountName}")
                    {
                        requestFound = true;
                    }
                }
            }

            // Person is no longer a friend, destroy object and remove from list
            if (requestFound == false)
            {
                Destroy(requestObject); // Gets removed from list using OnDestroy trigger
            }
        }

    }
    public static void RemoveUselessFriendsOrIgnoredObjects(List<FriendsOrIgnoredModel> content)
    {
        foreach (GameObject offlineFriendObject in FriendBoxes["OfflineFriends"])
        {
            bool friendFound = false;

            foreach (var _friendship in content)
            {

                // If we didnt already find our friend and if were friends and not ignored
                if (_friendship.IsFriend == true && friendFound == false)
                {
                    // If any of the friend models match our offline friend name
                    if (offlineFriendObject.name == _friendship.SecondAccountName)
                    {
                        friendFound = true;
                    }
                }
            }

            // Person is no longer a friend, destroy object and remove from list
            if (friendFound == false)
            {
                Destroy(offlineFriendObject); // Gets removed from list using OnDestroy trigger
            }
        }

        foreach (GameObject onlineFriendObject in FriendBoxes["OnlineFriends"])
        {
            bool friendFound = false;

            foreach (var _friendship in content)
            {
                // If we didnt already find our friend, and if were friends and not ignored
                if (_friendship.IsFriend == true && friendFound == false)
                {
                    // If any of the friend models match our offline friend name
                    if (onlineFriendObject.name == _friendship.SecondAccountName)
                    {
                        friendFound = true;
                    }
                }
            }
            // Person is no longer a friend, destroy object and remove from list
            if (friendFound == false)
            {
                Destroy(onlineFriendObject); // Gets removed from list using OnDestroy trigger
            }
        }

        foreach (GameObject ignoreObject in FriendBoxes["Ignored"])
        {
            bool ignoreFound = false;

            foreach (var _friendship in content)
            {
                // If we didnt already find our ignore, and if ignored and not friends
                if (_friendship.IsIgnored == true && ignoreFound == false)
                {
                    if (ignoreObject.name == _friendship.SecondAccountName)
                    {
                        ignoreFound = true;
                    }
                }
            }
            // Person is no longer ignored, destroy object and remove from list
            if (ignoreFound == false)
            {
                Destroy(ignoreObject); // Gets removed from list using OnDestroy trigger
            }
        }
    }

    public static void SetRequestInfo(GameObject requestObject, RequestModel _request, string incomingOrOutgoing)
    {
        if (incomingOrOutgoing == "IncomingRequest")
        {
            requestObject.GetComponent<IncomingRequest>().RequestId = _request.RequestId;
            requestObject.GetComponent<IncomingRequest>().FromAccountId = _request.FromAccountId;
            requestObject.GetComponent<IncomingRequest>().ToAccountId = _request.ToAccountId;
            requestObject.GetComponent<IncomingRequest>().FromAccountName = _request.FromAccountName;
            requestObject.GetComponent<IncomingRequest>().ToAccountName = _request.ToAccountName;
            requestObject.GetComponent<IncomingRequest>().RequestType = _request.RequestType;
            requestObject.GetComponent<IncomingRequest>().ClanName = _request.ClanName;
            requestObject.GetComponent<IncomingRequest>().PartyName = _request.PartyName;
        }
        if (incomingOrOutgoing == "OutgoingRequest")
        {
            requestObject.GetComponent<OutgoingRequest>().RequestId = _request.RequestId;
            requestObject.GetComponent<OutgoingRequest>().FromAccountId = _request.FromAccountId;
            requestObject.GetComponent<OutgoingRequest>().ToAccountId = _request.ToAccountId;
            requestObject.GetComponent<OutgoingRequest>().FromAccountName = _request.FromAccountName;
            requestObject.GetComponent<OutgoingRequest>().ToAccountName = _request.ToAccountName;
            requestObject.GetComponent<OutgoingRequest>().RequestType = _request.RequestType;
            requestObject.GetComponent<OutgoingRequest>().ClanName = _request.ClanName;
            requestObject.GetComponent<OutgoingRequest>().PartyName = _request.PartyName;
        }
    }


    [MessageHandler((ushort)ServerToClientId.SendUpdateRequests)]
    private static void UpdateRequests(Message message)
    {
        message.GetBool();
        GetAllRequests();
        GetAllFriendsOrIgnored();
    }

    public static ushort GetClientByDatabaseName(string databaseName)
    {
        //var player = Player.ClientList.FirstOrDefault(x => x.Value.accountName.Contains(databaseName)).Key;
        var player = Player.ClientList.FirstOrDefault(x => x.Value.accountName == databaseName).Key;
        return player;
    }
    public static Player GetPlayerByDatabaseName(string databaseName)
    {
        //var player = Player.ClientList.FirstOrDefault(x => x.Value.accountName.Contains(databaseName)).Value;
        var player = Player.ClientList.FirstOrDefault(x => x.Value.accountName == databaseName).Value;
        return player;
    }
    public static string GetAccountNameByClientId(ushort clientId)
    {
        Player player;
        bool hasValue = Player.ClientList.TryGetValue(clientId, out player);
        if (hasValue)
        {
            return player.accountName;
        }
        else
        {
            Debug.Log("Key not present");
            return "";
        }
    }

    public static void PlayerJoined(ushort e)
    {
        var ListToUse = FriendBoxes["OfflineFriends"];
        ushort serverClientId = e;

        var accountName = GetAccountNameByClientId(serverClientId);
        var localClientPlayer = Player.ClientList.FirstOrDefault(x => x.Value.accountName == accountName).Value;


        // If they are in our offline list
        if (ListToUse.Any(r => r.name == accountName))
        {
            var objectToDestroy = ListToUse.FirstOrDefault(x => x.name == accountName).gameObject;
            ListToUse.Remove(objectToDestroy);
            Destroy(objectToDestroy);

            // Add to online list
            Singleton.friendObject = Instantiate(Singleton.OnlineFriendPrefab, Singleton.OnlineFriendBox.transform);
            Singleton.friendObject.name = accountName;
            Singleton.friendObject.GetComponentInChildren<TMP_Text>().text = accountName;

            ListToUse = FriendBoxes["OnlineFriends"];
            ListToUse.Add(Singleton.friendObject);

            Singleton.OfflineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Offline Friends({FriendBoxes["OfflineFriends"].Count})";

            Singleton.OnlineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
                $"Online Friends({FriendBoxes["OnlineFriends"].Count})";
        }
    }
}

public class RequestModel
{
#nullable enable
    public int? RequestId { get; set; }
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public string? FromAccountName { get; set; }
    public string? ToAccountName { get; set; }
    public string? RequestType { get; set; }
    public string? ClanName { get; set; }
    public string? PartyName { get; set; }
}

public class FriendsOrIgnoredModel
{
#nullable enable
    public int? FriendshipId { get; set; }
    public int? FirstAccountId { get; set; }
    public int? SecondAccountId { get; set; }
    public string? FirstAccountName { get; set; }
    public string? SecondAccountName { get; set; }
    public bool? IsFriend { get; set; }
    public bool? IsIgnored { get; set; }
}
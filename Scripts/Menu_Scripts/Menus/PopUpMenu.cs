using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopUpMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static PopUpMenu _singleton;
    public static PopUpMenu Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(PopUpMenu)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }

    public TMP_Text titleText;
    public bool mouseOnPopUpMenu;

    public void Update()
    {
        if (Input.GetMouseButton(0) && mouseOnPopUpMenu == false)
        {
            MenuManager.Singleton.popUpMenu.SetActive(false);
        }
    }

    [Header("Menu Button Options")]
    [SerializeField] public GameObject whisperButton;
    [SerializeField] public GameObject partyInviteButton;
    [SerializeField] public GameObject partyRemoveButton;
    [SerializeField] public GameObject partyLeaderButton;
    [SerializeField] public GameObject clanInviteButton;
    [SerializeField] public GameObject clanRemoveButton;
    [SerializeField] public GameObject addFriendButton;
    [SerializeField] public GameObject removeFriendButton;
    [SerializeField] public GameObject ignoreButton;
    [SerializeField] public GameObject removeIgnoreButton;


    public enum MenuType
    {
        ChatMessage,
        Ignored,
        OnlineFriend,
        OfflineFriend,
        PartyLeader,
        PartyMember,
        ClanLeader,
        ClanMember,
    }

    public string targetName = "";

    public void OpenMenu(MenuType menu)
    {
        MenuManager.Singleton.popUpMenu.SetActive(true);

        switch (menu)
        {
            case MenuType.ChatMessage:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(true);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(true);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(true);
                removeFriendButton.SetActive(false);
                ignoreButton.SetActive(true);
                removeIgnoreButton.SetActive(false);
                break;
            case MenuType.Ignored:
                whisperButton.SetActive(false);
                partyInviteButton.SetActive(false);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(false);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(false);
                removeFriendButton.SetActive(false);
                ignoreButton.SetActive(false);
                removeIgnoreButton.SetActive(true);
                break;
            case MenuType.OnlineFriend:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(true);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(true);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(false);
                removeFriendButton.SetActive(true);
                ignoreButton.SetActive(false);
                removeIgnoreButton.SetActive(false);
                break;
            case MenuType.OfflineFriend:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(false);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(true);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(false);
                removeFriendButton.SetActive(true);
                ignoreButton.SetActive(false);
                removeIgnoreButton.SetActive(false);
                break;
            case MenuType.PartyLeader:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(false);
                partyRemoveButton.SetActive(true);
                partyLeaderButton.SetActive(true);
                clanInviteButton.SetActive(true);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(true);
                removeFriendButton.SetActive(false);
                ignoreButton.SetActive(true);
                removeIgnoreButton.SetActive(false);
                break;
            case MenuType.PartyMember:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(false);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(true);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(true);
                removeFriendButton.SetActive(false);
                ignoreButton.SetActive(true);
                removeIgnoreButton.SetActive(false);
                break;
            case MenuType.ClanLeader:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(true);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(false);
                clanRemoveButton.SetActive(true);
                addFriendButton.SetActive(true);
                removeFriendButton.SetActive(false);
                ignoreButton.SetActive(true);
                removeIgnoreButton.SetActive(false);
                break;
            case MenuType.ClanMember:
                whisperButton.SetActive(true);
                partyInviteButton.SetActive(true);
                partyRemoveButton.SetActive(false);
                partyLeaderButton.SetActive(false);
                clanInviteButton.SetActive(false);
                clanRemoveButton.SetActive(false);
                addFriendButton.SetActive(true);
                removeFriendButton.SetActive(false);
                ignoreButton.SetActive(true);
                removeIgnoreButton.SetActive(false);
                break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>());
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        foreach (GameObject hoveredItems in pointerEventData.hovered)
        {
            if (hoveredItems == MenuManager.Singleton.popUpMenu)
            {
                if (MenuManager.Singleton.popUpMenu.activeInHierarchy)
                {
                    mouseOnPopUpMenu = true;
                    Debug.Log(mouseOnPopUpMenu.ToString());
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {

        if (mouseOnPopUpMenu == true && MenuManager.Singleton.popUpMenu.activeInHierarchy)
        {
            mouseOnPopUpMenu = false;
            Debug.Log(mouseOnPopUpMenu.ToString());
        }

    }


    public Canvas canvas;
    public RectTransform popUpMenu;
    private Vector3 offset;
    private float padding;

    public void SetMenuPosition()
    {
        popUpMenu = MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>();

        // I want Top-Left corner of box to mouse position
        Vector3 newVector3 = new Vector3(
        Input.mousePosition.x + (MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>().rect.width * canvas.scaleFactor / 2),
        Input.mousePosition.y - (MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>().rect.height * canvas.scaleFactor / 2),
        Input.mousePosition.z);

        // If the box will not fit in the window
        Vector3 newPosition = newVector3 + offset;
        newPosition.z = 0f;

        float rightEdgeToScreenEdgeDistance = Screen.width - (newPosition.x + popUpMenu.rect.width * canvas.scaleFactor / 2) - padding;
        float leftEdgeToScreenEdgeDistance = 0 - (newPosition.x - popUpMenu.rect.width * canvas.scaleFactor / 2) + padding;
        float topEdgeToScreenEdgeDistance = Screen.height - (newPosition.y + popUpMenu.rect.height * canvas.scaleFactor / 2) - padding;
        float bottomEdgeToScreenEdgeDistance = 0 - (newPosition.y - popUpMenu.rect.height * canvas.scaleFactor / 2) + padding;

        if (rightEdgeToScreenEdgeDistance < 0)
        {
            // Will put box to the right edge screen
            //newPosition.x += rightEdgeToScreenEdgeDistance;

            // Will put rightside of box to mouse position
            newPosition.x = Input.mousePosition.x - (MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>().rect.width * canvas.scaleFactor / 2);
        }
        if (leftEdgeToScreenEdgeDistance > 0)
        {
            // Will put box to the left edge screen
            newPosition.x += leftEdgeToScreenEdgeDistance;

            // Will put leftside of box to mouse position
            //newPosition.x = Input.mousePosition.x + (MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>().rect.width * canvas.scaleFactor / 2);
        }
        if (topEdgeToScreenEdgeDistance < 0)
        {
            // Will put box to the top edge screen
            newPosition.y += topEdgeToScreenEdgeDistance;

            // Will put top of box to mouse position
            //newPosition.y = Input.mousePosition.y - (MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>().rect.height * canvas.scaleFactor / 2);
        }
        if (bottomEdgeToScreenEdgeDistance > 0)
        {
            // Will put box to the bottom edge screen
            //newPosition.y += bottomEdgeToScreenEdgeDistance;

            // Will put bottom of box to mouse position
            newPosition.y = Input.mousePosition.y + (MenuManager.Singleton.popUpMenu.GetComponent<RectTransform>().rect.height * canvas.scaleFactor / 2);
        }
        popUpMenu.transform.position = newPosition;
        MenuManager.Singleton.popUpMenu.SetActive(true);
    }


    public void WhisperButton()
    {
        HomeMenu.Singleton.messageInput.text = $"/whisper {targetName} ";
        HomeMenu.Singleton.messageInput.Select();
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public void PartyInviteButton()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/partyadd {targetName}", "Command");
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public void PartyRemoveButton()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/partyremove {targetName}", "Command");
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public void PartyLeaderButton()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/partyleader {targetName}", "Command");
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public void ClanInviteButton()
    {

    }
    public void ClanRemoveButton()
    {

    }
    public void AddFriendButton()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/friends add {targetName}", "Command");
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public void RemoveFriendButton()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/friends remove {targetName}", "Command");
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public async void IgnoreButton()
    {
        //ChatManager.Singleton.SendTextCommandMessage($"/ignore {targetName}", "Command");
        //MenuManager.Singleton.popUpMenu.SetActive(false);

        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/AddIgnore", Method.Post);

            request.AddBody(new FriendsOrIgnoredModel
            {
                FirstAccountName = ClientData.Singleton.AccountName,
                SecondAccountName = targetName,
                IsIgnored = true
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
            }
            if (response.Content == "1")
            {
                FriendManager.GetAllFriendsOrIgnored();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
    public async void RemoveIgnoreButton()
    {
        var ListToUse = FriendManager.FriendBoxes["Ignored"];
        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/RemoveIgnore", Method.Post);

            request.AddBody(new FriendsOrIgnoredModel
            {
                FirstAccountName = ClientData.Singleton.AccountName,
                SecondAccountName = targetName,
                IsIgnored = true
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
            }
            if (response.Content == "1")
            {
                var ignoreObject = ListToUse.FirstOrDefault(x => x.name == targetName);
                Destroy(ignoreObject);

                FriendManager.GetAllFriendsOrIgnored();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        MenuManager.Singleton.popUpMenu.SetActive(false);
    }
}

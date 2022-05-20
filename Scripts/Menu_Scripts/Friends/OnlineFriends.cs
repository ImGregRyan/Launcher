using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnlineFriends : MonoBehaviour, IPointerClickHandler
{
    public void OnDestroy()
    {
        List<GameObject> ListToUse = FriendManager.FriendBoxes["OnlineFriends"];
        ListToUse.Remove(gameObject);


        FriendManager.Singleton.OnlineFriendBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Online Friends({FriendManager.FriendBoxes["OnlineFriends"].Count})";
    }

    float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            PopUpMenu.Singleton.targetName = name;
            MenuManager.Singleton.GetComponent<PopUpMenu>().titleText.text = name;
            PopUpMenu.Singleton.OpenMenu(PopUpMenu.MenuType.OnlineFriend);
            PopUpMenu.Singleton.SetMenuPosition();   
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;

            if (clicked > 1 && Time.time - clicktime < clickdelay)
            {
                clicked = 0;
                clicktime = 0;
                Debug.Log("Double Click: " + this.GetComponent<RectTransform>().name);
                OpenChatWithFriend();
            }
            else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
        }
    }



    public void InviteFriendToParty()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/pa {name}", "Command");
    }

    public async void OpenChatWithFriend()
    {
        GameObject chatBoxFound;
        GameObject chatTab;
        string chatBoxName = $"WHISPER {name}";
        List<GameObject> ListToUse;
        bool chatExists = ChatManager.ChatBoxes.ContainsKey(chatBoxName);

        // Check for message List in the Dictionary that matches
        if (chatExists)
        {
            ListToUse = ChatManager.ChatBoxes[chatBoxName];
        }
        else// If we didnt find a List, we make that shit
        {
            ChatManager.ChatBoxes.Add(chatBoxName, new List<GameObject>());
            ListToUse = ChatManager.ChatBoxes[chatBoxName];
        }

        // Check for the Message container in unity
        if (ChatManager.Singleton.ChatBoxContainer.transform.Find(chatBoxName))
        {
            chatBoxFound = ChatManager.Singleton.ChatBoxContainer.transform.Find(chatBoxName).gameObject;
            chatTab = ChatManager.Singleton.ChatBoxContainer.transform.Find(chatBoxName).gameObject;
        }
        else // If we didnt find a chat box, we make that shit
        {
            // Every chat box needs a navigation tab
            chatTab = Instantiate(ChatManager.Singleton.ChatTabPrefab, ChatManager.Singleton.ChatTabContainer.transform);
            chatTab.name = chatBoxName;
            chatTab.GetComponentInChildren<TMP_Text>().text = name;

            // Make new chat box
            chatBoxFound = Instantiate(ChatManager.Singleton.AdditionalChatBoxPrefab, ChatManager.Singleton.ChatBoxContainer.transform);
            chatBoxFound.name = chatBoxName;
            chatBoxFound.GetComponent<ChatBox>().privateWhisper = true;

            chatBoxFound.SetActive(false);
            ChatManager.Singleton.GlobalChatBox.SetActive(true);

            await ChatManager.GetChatHistory("WHISPER", name, ListToUse, chatBoxFound);
        }

        chatTab.GetComponent<ChatTab>().ChatTabClicked();

    }
}

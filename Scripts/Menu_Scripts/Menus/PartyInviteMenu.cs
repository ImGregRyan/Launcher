using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyInviteMenu : MonoBehaviour
{
    static ushort senderClientId;
    static string senderAccountName;
    static string partyName;
    static GameObject partyInviteMenu;


    private static float currentTime = 0;
    private static float executedTime = 0;
    private static float timeToWait = 30;
    private static bool windowTrigger;

    private void Update()
    {
        currentTime = Time.time;
        if(executedTime != 0)
        {
            partyInviteMenu.SetActive(true);

            if (currentTime - executedTime > timeToWait)
            {
                executedTime = 0;
                DeclineButton();
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.SendPartyInvite)]
    private static void IncomingPartyInvite(Message message)
    {
        senderClientId = message.GetUShort();
        senderAccountName = message.GetString();
        partyName = message.GetString();
        partyInviteMenu = MenuManager.Singleton.partyInviteMenu;
        partyInviteMenu.GetComponentInChildren<TMP_Text>().text = $"{senderAccountName} has invited you to a party.";

        var foundIgnored = false;

        foreach (var item in FriendManager.FriendBoxes["Ignored"])
        {
            if (item.name == senderAccountName)
            {
                foundIgnored = true;
                DeclineButton();
            }
        }

        if (foundIgnored == false)
        {
            executedTime = currentTime;
        }

        //executedTime = currentTime;
    }

    public void AcceptButton()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendPartyInviteAnswer);
        message.AddUShort(senderClientId);
        message.AddString(ClientData.Singleton.AccountName);
        message.AddString(partyName);
        message.AddString("ACCEPT");

        NetworkManager.Singleton.Client.Send(message);

        executedTime = 0;
        MenuManager.Singleton.partyInviteMenu.SetActive(false);
    }

    public static void DeclineButton()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendPartyInviteAnswer);
        message.AddUShort(senderClientId);
        message.AddString(ClientData.Singleton.AccountName);
        message.AddString(partyName);
        message.AddString("DECLINE");

        NetworkManager.Singleton.Client.Send(message);

        executedTime = 0;
        MenuManager.Singleton.partyInviteMenu.SetActive(false);
    }
}

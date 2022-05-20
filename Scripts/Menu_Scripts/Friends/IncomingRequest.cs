using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncomingRequest : MonoBehaviour
{
    public void OnDestroy()
    {
        List<GameObject> ListToUse = FriendManager.FriendBoxes["IncomingRequests"];
        ListToUse.Remove(gameObject);

        FriendManager.Singleton.IncomingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Incoming({ListToUse.Count})";
    }

#nullable enable
    public int? RequestId;
    public int? FromAccountId;
    public int? ToAccountId;
    public string? FromAccountName;
    public string? ToAccountName;
    public string? RequestType;
    public string? ClanName;
    public string? PartyName;

    public void SendRequestAnswerAccept()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendRequestAnswer);
        message.AddString("ACCEPT");
        message.AddString(RequestType);
        message.AddString(FromAccountName);// from account
        message.AddString(ToAccountName);// to account
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendRequestAnswerDecline()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendRequestAnswer);
        message.AddString("DECLINE");
        message.AddString(RequestType);
        message.AddString(FromAccountName);// from account
        message.AddString(ToAccountName);// to account
        NetworkManager.Singleton.Client.Send(message);
    }
}

using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OutgoingRequest : MonoBehaviour
{
    public void OnDestroy()
    {
        List<GameObject> ListToUse = FriendManager.FriendBoxes["OutgoingRequests"];
        ListToUse.Remove(gameObject);

        FriendManager.Singleton.OutgoingRequestBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Outgoing({ListToUse.Count})";
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


    public void SendRequestAnswerCancel()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendRequestAnswer);
        message.AddString("CANCEL");
        message.AddString(RequestType);
        message.AddString(FromAccountName);// from account
        message.AddString(ToAccountName);// to account
        NetworkManager.Singleton.Client.Send(message);
    }

}

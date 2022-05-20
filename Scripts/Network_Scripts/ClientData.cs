using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData : MonoBehaviour
{
    private static ClientData _singleton;
    public static ClientData Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(ClientData)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }

    public int DatabaseId;
    public string AccountName;
    public bool IsConnected;

    public void SendAccountInfo()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendAccountInfo);
        message.AddInt(Singleton.DatabaseId);
        message.AddString(Singleton.AccountName);
        NetworkManager.Singleton.Client.Send(message);
    }

}
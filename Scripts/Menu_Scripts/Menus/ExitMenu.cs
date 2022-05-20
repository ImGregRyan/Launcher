using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    public void LogoutButton()
    {
        if(ClientData.Singleton.IsConnected == true)
        {
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.SendDisconnectMe);
            message.AddString("DisconnectMe");
            NetworkManager.Singleton.Client.Send(message);

            MenuManager.Singleton.exitMenu.SetActive(false);
        }
    }
    public void ExitProgramButton()
    {
        Application.Quit();
    }
    public void ExitButton()
    {
        MenuManager.Singleton.exitMenu.SetActive(false);
    }
}

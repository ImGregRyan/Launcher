using Newtonsoft.Json;
using RestSharp;
using RiptideNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class HomeMenu : MonoBehaviour
{
    private static HomeMenu _singleton;
    public static HomeMenu Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(HomeMenu)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }

    public GameObject _notConnected;
    public GameObject _isConnected;
    public TMP_Text _accountNameText;

    private bool allowEnter;
    public TMP_InputField messageInput;
    

    public static string currentTime;

    void Update()
    {
        currentTime = DateTime.Now.ToString("h:mm tt");

        // Can press enter to send message
        if (allowEnter == true && Input.GetKeyDown(KeyCode.Return) && !MenuManager.Singleton.tabCommandMenu.activeInHierarchy)
        {
            if (messageInput.text == "")
            {
                return;
            }
            else
            {
                allowEnter = false;
                ChatManager.Singleton.SendChatMessage();

                messageInput.text = "";
                messageInput.Select();
                messageInput.ActivateInputField();
            }
        }
        else
        {
            allowEnter = messageInput.isFocused == true;
        }

        // Swap connection status image
        if (ClientData.Singleton.IsConnected == true)
        {
            _notConnected.SetActive(false);
            _isConnected.SetActive(true);
        }
        else
        {
            _notConnected.SetActive(true);
            _isConnected.SetActive(false);
        }
    }

    public void SettingsButton()
    {
        MenuManager.Singleton.settingsMenu.SetActive(true);
    }

    public void ExitButton()
    {
        MenuManager.Singleton.exitMenu.SetActive(true);
    }
}

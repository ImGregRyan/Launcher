using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private static MenuManager _singleton;
    public static MenuManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(MenuManager)} instance already exsists, destroying this duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Singleton = this;
    }

    [Header("Menu GameObjects")]
    [SerializeField] public GameObject menuManager;
    [SerializeField] public GameObject launcherUpdateMenu;
    [SerializeField] public GameObject loginMenu;
    [SerializeField] public GameObject sendPasswordKeyMenu;
    [SerializeField] public GameObject resetPasswordMenu;
    [SerializeField] public GameObject forgotAccountNameMenu;
    [SerializeField] public GameObject loginWaitMenu;
    [SerializeField] public GameObject registerMenu;
    [SerializeField] public GameObject homeMenu;
    [SerializeField] public GameObject homeLoginMenu;
    [SerializeField] public GameObject settingsMenu;
    [SerializeField] public GameObject exitMenu;
    [SerializeField] public GameObject alertMenu;
    [SerializeField] public GameObject partyMenu;
    [SerializeField] public GameObject partyInviteMenu;
    [SerializeField] public GameObject popUpMenu;
    [SerializeField] public GameObject tabCommandMenu;
    [SerializeField] public TMP_Text alertMenuText;
    [SerializeField] public GameObject backgroundVideo;
    [SerializeField] public GameObject openingVideo;

    public enum Menu
    {
        LoginMenu,
        HomeMenu,
        RegisterMenu,
        SettingsMenu,
        ExitMenu,
        AlertMenu,
    }
    public void OpenMenu(Menu menu)
    {
        switch (menu)
        {
            case Menu.HomeMenu:
                homeMenu.SetActive(true);
                registerMenu.SetActive(false);
                settingsMenu.SetActive(false);
                exitMenu.SetActive(false);
                break;
            case Menu.RegisterMenu:
                homeMenu.SetActive(true);
                registerMenu.SetActive(true);
                settingsMenu.SetActive(false);
                exitMenu.SetActive(false);
                break;
            case Menu.SettingsMenu:
                homeMenu.SetActive(true);
                registerMenu.SetActive(false);
                settingsMenu.SetActive(true);
                exitMenu.SetActive(false);
                break;
            case Menu.ExitMenu:
                homeMenu.SetActive(true);
                registerMenu.SetActive(false);
                settingsMenu.SetActive(false);
                exitMenu.SetActive(true);
                break;
            case Menu.AlertMenu:
                alertMenu.SetActive(true);
                break;
        }
    }
}
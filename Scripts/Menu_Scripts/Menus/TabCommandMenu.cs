using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabCommandMenu : MonoBehaviour, IPointerClickHandler
{
    public Sprite selectedImage;
    public Sprite deselectedImage;

    private int commandSelected = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Tab through fields
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Nothing to show yet
            if (ChatManager.ChatCommands.Count == 0)
            {
                return;
            }
            // Popup window is not up yet
            if (HomeMenu.Singleton.messageInput.isFocused && !MenuManager.Singleton.tabCommandMenu.gameObject.activeInHierarchy)
            {
                MenuManager.Singleton.tabCommandMenu.SetActive(true);
                if(commandSelected == -1)
                {
                    commandSelected = ChatManager.ChatCommands.Count - 1;
                }
            }
            // Popup window is already up, and theres more than one command, cycle through commands
            else if (MenuManager.Singleton.tabCommandMenu.gameObject.activeInHierarchy && ChatManager.ChatCommands.Count >= 2)
            {
                commandSelected++;
                if (commandSelected > ChatManager.ChatCommands.Count - 1)
                    commandSelected = 0;
            }

            foreach (var tabCommand in ChatManager.ChatCommands)
            {
                if (commandSelected == ChatManager.ChatCommands.IndexOf(tabCommand))
                {
                    tabCommand.GetComponentInChildren<Image>().sprite = selectedImage;
                }
                else
                {
                    tabCommand.GetComponentInChildren<Image>().sprite = deselectedImage;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Return) && MenuManager.Singleton.tabCommandMenu.activeInHierarchy)
        {
            SetTabCommandText();
            //foreach (var tabCommand in ChatManager.ChatCommands)
            //{
            //    if (commandSelected == ChatManager.ChatCommands.IndexOf(tabCommand))
            //    {
            //        HomeMenu.Singleton.messageInput.text = tabCommand.name + " ";
            //        HomeMenu.Singleton.messageInput.Select();
            //    } 
            //}
            //MenuManager.Singleton.tabCommandMenu.SetActive(false);
        }

        if(Input.GetKeyDown(KeyCode.Escape) && MenuManager.Singleton.tabCommandMenu.activeInHierarchy)
        {
            MenuManager.Singleton.tabCommandMenu.SetActive(false);
            HomeMenu.Singleton.messageInput.Select();
            HomeMenu.Singleton.messageInput.ActivateInputField();
            HomeMenu.Singleton.messageInput.caretPosition = HomeMenu.Singleton.messageInput.text.Length;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && MenuManager.Singleton.tabCommandMenu.activeInHierarchy)
        {
            MenuManager.Singleton.tabCommandMenu.SetActive(false);
        }
    }

    private void SetTabCommandText()
    {
        foreach (var tabCommand in ChatManager.ChatCommands)
        {
            if (commandSelected == ChatManager.ChatCommands.IndexOf(tabCommand))
            {
                HomeMenu.Singleton.messageInput.text = tabCommand.name + " ";
                continue;
            }
        }
        MenuManager.Singleton.tabCommandMenu.SetActive(false);
        HomeMenu.Singleton.messageInput.Select();
        HomeMenu.Singleton.messageInput.ActivateInputField();
        HomeMenu.Singleton.messageInput.caretPosition = HomeMenu.Singleton.messageInput.text.Length;
    }
}

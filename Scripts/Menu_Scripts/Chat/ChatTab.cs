using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatTab : MonoBehaviour
{

    public void ChatTabClicked()
    {
        // Trying to highlight the tab for the open chat box
        foreach (Transform chatTab in ChatManager.Singleton.ChatTabContainer.transform)
        {
            if(chatTab.name == name)
            {
                chatTab.GetComponent<Image>().color = new Color32(198,172,42,255);
            }
            else
            {
                chatTab.GetComponent<Image>().color = new Color32(255,255,255,255);
            }
        }

        foreach (KeyValuePair<string, List<GameObject>> ChatBox in ChatManager.ChatBoxes)
        {
            if (ChatBox.Key == name)
            {
                // Set active the one chat box we want
                var chatWindow = ChatManager.Singleton.ChatBoxContainer.transform.Find(name).gameObject;
                chatWindow.SetActive(true);

                // Set a input message ref
                var messageInput = HomeMenu.Singleton.messageInput;

                // Select the field and put carrot in
                messageInput.Select();
                messageInput.ActivateInputField();

            }
            if (ChatBox.Key != name)
            {
                //Debug.Log($"ChatBoxKey: {ChatBox.Key} != TabName: {name}");
                ChatManager.Singleton.ChatBoxContainer.transform.Find(ChatBox.Key).gameObject.SetActive(false);
            }
        }
    }
}

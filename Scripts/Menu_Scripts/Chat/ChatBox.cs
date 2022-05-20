using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    public bool generalChat;
    public bool privateWhisper;
    public bool partyChat;

    public void OnDestroy()
    {
        ChatManager.ChatBoxes.Remove(name);
    }

    public void ExitButton()
    {
        var chatTab = ChatManager.Singleton.ChatTabContainer.transform.Find(name);
        Destroy(chatTab.gameObject);

        ChatManager.ChatBoxes.Remove(this.name);

        Destroy(this.gameObject);

        //ChatManager.Singleton.ChatBoxContainer.transform.Find("Global").gameObject.SetActive(true);
        ChatManager.Singleton.ChatTabContainer.transform.Find("Global").GetComponent<ChatTab>().ChatTabClicked();
    }
}

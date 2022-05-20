using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PartyPlayer : MonoBehaviour, IPointerClickHandler
{
    public GameObject partyLeaderImage;
    public GameObject removeButton;
    public GameObject connectedImage;
    public GameObject disconnectedImage;

    public string playerName;
    public string isPartyLeader;


    public void RemoveFromParty()
    {
        ChatManager.Singleton.SendTextCommandMessage($"/pr {name}", "Command");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if(NetworkManager.Singleton.LocalPlayer.accountName == name)
            {
                return;
            }
            if(NetworkManager.Singleton.LocalPlayer.IsPartyLeader == "True")
            {
                PopUpMenu.Singleton.targetName = name;
                MenuManager.Singleton.GetComponent<PopUpMenu>().titleText.text = name;
                PopUpMenu.Singleton.OpenMenu(PopUpMenu.MenuType.PartyLeader);
                PopUpMenu.Singleton.SetMenuPosition();
            }
            if(NetworkManager.Singleton.LocalPlayer.IsPartyLeader == "False")
            {
                PopUpMenu.Singleton.targetName = name;
                MenuManager.Singleton.GetComponent<PopUpMenu>().titleText.text = name;
                PopUpMenu.Singleton.OpenMenu(PopUpMenu.MenuType.PartyMember);
                PopUpMenu.Singleton.SetMenuPosition();
            }
        }
    }
}

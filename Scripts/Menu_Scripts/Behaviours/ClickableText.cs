using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=lyZuCnARNSw
// Clickable Chat Text in Unity3D - For Item tooltips or anything else


public class ClickableText : MonoBehaviour, IPointerClickHandler
{
    public GameObject chatInputField;
    public string otherAccountName;
    bool getShift;

    public void Awake()
    {
        chatInputField = GameObject.Find("ChatInputField");
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            getShift = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            getShift = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var text = GetComponent<TMP_Text>();

        if (getShift == true && eventData.button == PointerEventData.InputButton.Left)
        {
            int wordIndex = TMP_TextUtilities.FindIntersectingWord(text, Input.mousePosition, null);
            if (wordIndex > -1)
            {
                var wordInfo = text.textInfo.wordInfo[wordIndex].GetWord();

                chatInputField.GetComponent<TMP_InputField>().text = chatInputField.GetComponent<TMP_InputField>().text + wordInfo + " ";

                chatInputField.GetComponent<TMP_InputField>().Select();
            }
            chatInputField.GetComponent<TMP_InputField>().Select();
        }


        if (eventData.button == PointerEventData.InputButton.Right)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

            if (linkIndex > -1)
            {
                var linkInfo = text.textInfo.linkInfo[linkIndex];

                MenuManager.Singleton.GetComponent<PopUpMenu>().titleText.text = linkInfo.GetLinkText();
                MenuManager.Singleton.GetComponent<PopUpMenu>().targetName = linkInfo.GetLinkText();

                PopUpMenu.Singleton.targetName = otherAccountName;
                PopUpMenu.Singleton.OpenMenu(PopUpMenu.MenuType.ChatMessage);
                PopUpMenu.Singleton.SetMenuPosition();

            }
        }


        //Example text that would be read  for item linking:
        //"Here is a <link=shortsword><color=green>short sword.<color></link>"


        //Left click only, show an item link or something
        //if(eventData.button == PointerEventData.InputButton.Left)
        //{
        //    int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

        //    if(linkIndex >-1)
        //    {
        //        var linkInfo = text.textInfo.linkInfo[linkIndex];
        //        var linkId = linkInfo.GetLinkID();

        //        var itemData = FindObjectOfType<ItemDataController>().Get(linkId);

        //        PopupPanel.Show(itemData);
        //    }
        //}

    }
}


//Sample classes to use the link setup
//public class ItemDataController : MonoBehaviour
//{
//    [SerializeField]
//    private ItemData[] items;

//    public ItemData Get(string key)
//    {
//        return items.FirstOrDefault(t => t.Key == key);
//    }
//}

//[Serializable]
//public struct ItemData
//{
//    public string Key;
//    public string Name;
//    public string Description;
//    public int Damage;
//    public Sprite Sprite;
//}

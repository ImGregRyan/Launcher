using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ignored : MonoBehaviour, IPointerClickHandler
{
    public string secondAccountName;


    public void OnDestroy()
    {
        List<GameObject> ListToUse = FriendManager.FriendBoxes["Ignored"];
        ListToUse.Remove(gameObject);

        FriendManager.Singleton.IgnoreBox.transform.Find("TitleButton").GetComponentInChildren<TMP_Text>().text =
            $"Ignored({ListToUse.Count})";
    }

    public async void RemoveIgnore()
    {
        var ListToUse = FriendManager.FriendBoxes["Ignored"];
        try
        {
            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/RemoveIgnore", Method.Post);

            request.AddBody(new FriendsOrIgnoredModel
            {
                FirstAccountName = ClientData.Singleton.AccountName,
                SecondAccountName = secondAccountName,
                IsIgnored = true
            });
            RestResponse response = await client.ExecuteAsync(request);

            if (response.ErrorException != null)
            {
                const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                var MyException = new ApplicationException(errmsg, response.ErrorException);
                throw MyException;
            }
            if (response.Content == "-1")
            {
            }
            if (response.Content == "1")
            {
                var ignoreObject = ListToUse.FirstOrDefault(x => x.name == secondAccountName);
                Destroy(ignoreObject);

                FriendManager.GetAllFriendsOrIgnored();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            PopUpMenu.Singleton.targetName = name;
            MenuManager.Singleton.GetComponent<PopUpMenu>().titleText.text = name;
            PopUpMenu.Singleton.OpenMenu(PopUpMenu.MenuType.Ignored);
            PopUpMenu.Singleton.SetMenuPosition();
        }
    }
}

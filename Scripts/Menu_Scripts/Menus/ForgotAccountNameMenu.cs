using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ForgotAccountNameMenu : MonoBehaviour
{
    public TMP_InputField _email;

    private bool allowEnter;

    void Update()
    {
        //submit with enter button
        if (allowEnter == true && Input.GetKeyDown(KeyCode.Return))
        {
            allowEnter = false;
            ForgotAccountNameButton();
        }
        else
        {
            allowEnter = _email.isFocused == true;
        }
    }

    public async void ForgotAccountNameButton()
    {
        if (_email.text == "")
        {
            MenuManager.Singleton.alertMenu.SetActive(true);
            MenuManager.Singleton.alertMenuText.text = "You forgot to do something!";
            return;
        }
        else
        {
            try
            {
                RestClient client = new RestClient(NetworkManager.apiUrl);
                RestRequest request = new RestRequest("AccountApi/ForgotAccountName", Method.Post);
                request.AddBody(new AccountInfoModel
                {
                    AccountEmail = _email.text
                });

                RestResponse response = await client.ExecuteAsync(request);

                Debug.Log(response.Content);

                if (response.ErrorException != null)
                {
                    const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                    var MyException = new ApplicationException(errmsg, response.ErrorException);
                    throw MyException;
                }

                if (response.Content == "1")
                {
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.forgotAccountNameMenu.SetActive(false);
                    MenuManager.Singleton.alertMenuText.text = "Your account name has been sent to your email.";
                    return;
                }
                else
                {
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "No account associated with that email.";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }


    public void ExitButton()
    {
        MenuManager.Singleton.forgotAccountNameMenu.SetActive(false);
    }
}

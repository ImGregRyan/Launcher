using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PasswordKeyMenu : MonoBehaviour
{
    public TMP_InputField _accountName; // 0
    public TMP_InputField _email;

    public int _inputSelected;
    private bool allowEnter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && (_accountName.isFocused == true || _email.isFocused == true))
        {
            _inputSelected++;
            if (_inputSelected > 1)
                _inputSelected = 0;
            SetInputField();
        }

        //submit with enter button
        if (allowEnter == true && Input.GetKeyDown(KeyCode.Return))
        {
            allowEnter = false;
            SendPasswordKeyButton();
        }
        else
        {
            allowEnter = _accountName.isFocused == true || _email.isFocused == true;
        }
    }
    // Called by an on click event in unity
    public void AccountNameSelected() => _inputSelected = 0;
    public void EmailSelected() => _inputSelected = 1;

    public void SetInputField()
    {
        switch (_inputSelected)
        {
            case 0:
                _accountName.Select();
                break;
            case 1:
                _email.Select();
                break;
        }
    }

    public async void SendPasswordKeyButton()
    {
        if (_accountName.text == "" || _email.text == "")
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
                RestRequest request = new RestRequest("AccountApi/SendPasswordKey", Method.Post);
                request.AddBody(new AccountInfoModel
                {
                    AccountName = _accountName.text,
                    AccountEmail = _email.text,

                });

                RestResponse response = await client.ExecuteAsync(request);
                Debug.Log(response.Content);

                if (response.ErrorException != null)
                {
                    const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                    var MyException = new ApplicationException(errmsg, response.ErrorException);
                    throw MyException;
                }
                if (response.Content == "-1")
                {
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Error sending request.";
                }
                if (response.Content == "1")
                {
                    MenuManager.Singleton.sendPasswordKeyMenu.SetActive(false);
                    MenuManager.Singleton.resetPasswordMenu.SetActive(true);
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Your password reset key has been sent to your email.";
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
        MenuManager.Singleton.sendPasswordKeyMenu.SetActive(false);
    }
}

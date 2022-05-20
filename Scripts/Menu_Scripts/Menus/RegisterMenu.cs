using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RegisterMenu : MonoBehaviour
{
    public TMP_InputField _accountName;
    public TMP_InputField _password;
    public TMP_InputField _confirmPassword;
    public TMP_InputField _email;

    public int _inputSelected;
    private bool allowEnter;

    void Update()
    {
        // Tab through input fields
        if (Input.GetKeyDown(KeyCode.Tab) && (_accountName.isFocused == true || _password.isFocused == true || _confirmPassword.isFocused == true || _email.isFocused == true))
        {
            _inputSelected++;
            if (_inputSelected > 3)
                _inputSelected = 0;
            SetInputField();
        }

        // Submit with enter button
        if (allowEnter == true && Input.GetKeyDown(KeyCode.Return))
        {
            allowEnter = false;
            RegisterButton();
        }
        else
        {
            allowEnter = _accountName.isFocused == true || _password.isFocused == true || _confirmPassword.isFocused == true || _email.isFocused == true;
        }
    }

    /// <summary>
    /// Tab through stuff
    /// </summary>

    // Called by an on click event in unity
    public void AccountNameSelected() => _inputSelected = 0;
    public void PasswordSelected() => _inputSelected = 1;
    public void ConfirmPasswordSelected() => _inputSelected = 2;
    public void EmailSelected() => _inputSelected = 3;

    public void SetInputField()
    {
        switch (_inputSelected)
        {
            case 0:
                _accountName.Select();
                break;
            case 1:
                _password.Select();
                break;
            case 2:
                _confirmPassword.Select();
                break;
            case 3:
                _email.Select();
                break;
        }
    }

    /// <summary>
    /// Button stuff
    /// </summary>

    string[] AccountNamesNotAllowed =
    {
        "ADMIN",
        "ADMINISTRATOR",
        "SERVER",
        "PLAYER",
        "PARTY",
        "GROUP",
        "CLAN",
    };


    public async void RegisterButton()
    {
        bool accountNameNotAllowed = AccountNamesNotAllowed.Contains(_accountName.text.ToUpper());

        if (_accountName.text == "" || _password.text == "" || _confirmPassword.text == "" || _email.text == "")
        {
            MenuManager.Singleton.alertMenu.SetActive(true);

            MenuManager.Singleton.alertMenuText.text = "You forgot to do something!";

            return;
        }
        else if (accountNameNotAllowed == true)
        {
            MenuManager.Singleton.alertMenu.SetActive(true);

            MenuManager.Singleton.alertMenuText.text = "Account name is not allowed.";

            return;
        }
        else if (_password.text != _confirmPassword.text)
        {
            MenuManager.Singleton.alertMenu.SetActive(true);
            MenuManager.Singleton.alertMenuText.text = "Your passwords did not match!";

            return;
        }
        else
        {
            // Fix format of the account name text that was entered, Cllllll/Mumbles
            string toAccountName;

            if (_accountName.text.Length == 1)
            {
                toAccountName = _accountName.text.ToUpper();
            }
            else
            {
                toAccountName = char.ToUpper(_accountName.text[0]) + _accountName.text.Substring(1).ToLower();
            }
            
            // Send account register request
            try
            {
                RestClient client = new RestClient(NetworkManager.apiUrl);
                RestRequest request = new RestRequest("AccountApi/RegisterAccount", Method.Post);
                request.AddBody(new AccountInfoModel
                {
                    AccountName = toAccountName,
                    AccountPassword = _password.text,
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
                if (response.Content == "-1")
                {
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Error sending register request.";
                }
                if (response.Content == "1")
                {
                    MenuManager.Singleton.registerMenu.SetActive(false);
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Your Account Was Created!";
                }
                if (response.Content == "2")
                {
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Account name or Email already in use.";
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
        MenuManager.Singleton.registerMenu.SetActive(false);
    }
}

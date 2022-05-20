using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResetPasswordMenu : MonoBehaviour
{
    public TMP_InputField _accountName; // 0
    public TMP_InputField _email;
    public TMP_InputField _password;
    public TMP_InputField _confirmPassword;
    public TMP_InputField _passwordKey; // 4

    public int _inputSelected;
    private bool allowEnter;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && (_accountName.isFocused == true || _password.isFocused == true || _confirmPassword.isFocused == true || _email.isFocused == true || _passwordKey.isFocused == true))
        {
            _inputSelected++;
            if (_inputSelected > 4)
                _inputSelected = 0;
            SetInputField();
        }

        //submit with enter button
        if (allowEnter == true && Input.GetKeyDown(KeyCode.Return))
        {
            allowEnter = false;
            ResetPasswordButton();
        }
        else
        {
            allowEnter = _accountName.isFocused == true || _password.isFocused == true || _confirmPassword.isFocused == true || _email.isFocused == true || _passwordKey.isFocused == true;
        }
    }
    // Called by an on click event in unity
    public void AccountNameSelected() => _inputSelected = 0;
    public void EmailSelected() => _inputSelected = 1;
    public void PasswordSelected() => _inputSelected = 2;
    public void ConfirmPasswordSelected() => _inputSelected = 3;
    public void PasswordKeySelected() => _inputSelected = 4;

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
            case 2:
                _password.Select();               
                break;
            case 3:
                _confirmPassword.Select();
                break;
            case 4:
                _passwordKey.Select();
                break;
        }
    }

    public async void ResetPasswordButton()
    {
        if (_accountName.text == "" || _password.text == "" || _confirmPassword.text == "" || _email.text == "" || _passwordKey.text == "")
        {
            MenuManager.Singleton.alertMenu.SetActive(true);
            MenuManager.Singleton.alertMenuText.text = "You forgot to do something!";
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
            try
            {
                RestClient client = new RestClient(NetworkManager.apiUrl);
                RestRequest request = new RestRequest("AccountApi/ResetPassword", Method.Post);
                request.AddBody(new AccountInfoModel
                {
                    AccountName = _accountName.text,
                    AccountPassword = _password.text,
                    AccountPasswordKey = _passwordKey.text,
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
                    MenuManager.Singleton.alertMenuText.text = "Error sending register request.";
                }
                if (response.Content == "1")
                {
                    MenuManager.Singleton.resetPasswordMenu.SetActive(false);
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Your password has been reset.";
                }
                if (response.Content == "2")
                {
                    MenuManager.Singleton.alertMenu.SetActive(true);
                    MenuManager.Singleton.alertMenuText.text = "Account name associated with the Email not found.";
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
        MenuManager.Singleton.resetPasswordMenu.SetActive(false);
    }
}

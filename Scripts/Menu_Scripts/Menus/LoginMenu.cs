using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour
{
    public Toggle _accountToggle;
    public Toggle _passwordToggle;

    public TMP_InputField _accountName;
    public TMP_InputField _password;

    [SerializeField] private int _inputSelected;
    private bool _allowEnter;

    void Start()
    {
        // If login info was saved, get the info
        _accountName.text = PlayerPrefs.GetString("AccountName");
        _password.text = PlayerPrefs.GetString("Password");

        //Do something with account save results
        if (_accountName.text != "")
        {
            _accountToggle.isOn = true;
        }
        if (_accountName.text == "")
        {
            _accountToggle.isOn = false;
        }
        //Do something with password save results
        if (_password.text != "")
        {
            _passwordToggle.isOn = true;
        }
        if (_password.text == "")       
        {
            _passwordToggle.isOn = false;
        }
    }

    void Update()
    {
        // Tab through fields
        if (Input.GetKeyDown(KeyCode.Tab) && (_accountName.isFocused == true || _password.isFocused == true))
        {
            _inputSelected++;
            if (_inputSelected > 1)
                _inputSelected = 0;
            SetInputField();
        }

        // Submit with enter button
        if (_allowEnter == true && Input.GetKeyDown(KeyCode.Return))
        {
            _allowEnter = false;
            LoginButton();
        }
        else
        {
            _allowEnter = _accountName.isFocused == true || _password.isFocused == true;
        }
    }

    /// <summary>
    /// Tab through fields stuff
    /// </summary>

    // Called by on click event in unity
    public void AccountNameSelected() => _inputSelected = 0;
    public void PasswordSelected() => _inputSelected = 1;

    // Sets clicked input field
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
        }
    }

    /// <summary>
    /// Toggle stuff
    /// </summary>
    
    public void AccountToggle()
    {
        if (_accountToggle.isOn == true)
        {
            PlayerPrefs.SetString("AccountName", _accountName.text);
        }
        else
        {
            PlayerPrefs.SetString("AccountName", "");
        }
    }
    public void PasswordToggle()
    {
        if (_passwordToggle.isOn == true)
        {
            PlayerPrefs.SetString("Password", _password.text);
        }
        else
        {
            PlayerPrefs.SetString("Password", "");
        }
    }


    /// <summary>
    /// Button stuff
    /// </summary>
    public async void LoginButton()
    {
        if (_accountName.text == "" || _password.text == "")
        {
            MenuManager.Singleton.alertMenu.SetActive(true);
            MenuManager.Singleton.alertMenuText.text = "Your forgot to do something!";
            return;
        }

        try
        {
            MenuManager.Singleton.loginWaitMenu.SetActive(true);

            RestClient client = new RestClient(NetworkManager.apiUrl);
            RestRequest request = new RestRequest("AccountApi/LoginAccount", Method.Post);

            request.AddBody(new AccountInfoModel
            {
                AccountName = _accountName.text,
                AccountPassword = _password.text,
            });
            RestResponse response = await client.ExecuteAsync(request);
            var content = JsonConvert.DeserializeObject<List<AccountInfoModel>>(response.Content);

            if (response.ErrorException != null)
            {
                const string errmsg = "Error retrieving reponse. Check inner details for more info.";
                var MyException = new ApplicationException(errmsg, response.ErrorException);
                throw MyException;
            }
            // Set login response to our ClientData for later use
            ClientData.Singleton.DatabaseId = (int)content[0].AccountId;
            ClientData.Singleton.AccountName = content[0].AccountName;

        }
        catch (Exception)
        {
            MenuManager.Singleton.loginWaitMenu.SetActive(false);

            MenuManager.Singleton.alertMenu.SetActive(true);
            MenuManager.Singleton.alertMenuText.text = "Request failed.";
            return;
        }
        try
        {
            NetworkManager.Singleton.Connect();
            
            HomeMenu.Singleton._accountNameText.text = ClientData.Singleton.AccountName;
            HomeMenu.Singleton._accountNameText.gameObject.SetActive(true);
        }
        catch (Exception ex)
        {
            MenuManager.Singleton.loginWaitMenu.SetActive(false);
            throw ex;
        }
    }

    public void RegisterButton()
    {
        MenuManager.Singleton.registerMenu.SetActive(true);
    }
    public void ResetPasswordButton()
    {
        MenuManager.Singleton.sendPasswordKeyMenu.SetActive(true);
    }
    public void ForgotAccountNameButton()
    {
        MenuManager.Singleton.forgotAccountNameMenu.SetActive(true);
    }
    public void ExitButton()
    {
        MenuManager.Singleton.exitMenu.SetActive(true);
    }
    public void SettingsButton()
    {
        MenuManager.Singleton.settingsMenu.SetActive(true);
    }
}
public class AccountInfoModel
{
#nullable enable
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountPassword { get; set; }
    public string? AccountPasswordSalt { get; set; }
    public string? AccountPasswordKey{ get; set; }
    public string? AccountEmail { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? LastLogin { get; set; }
}
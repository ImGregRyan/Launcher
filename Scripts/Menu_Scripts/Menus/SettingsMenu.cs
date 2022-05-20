using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Toggle showTimeStamps;
    public static bool timeStampStatus = false;
    public static string timeStampAnswer = "false";

    public Toggle showWhispersInGlobal;
    public static bool showWhispersInGlobalStatus = true;
    public static string showWhispersInGlobalAnswer = "true";

    void Start()
    {
        // Get stored settings
        timeStampAnswer = PlayerPrefs.GetString("TimeStampStatus");

        showWhispersInGlobalAnswer = PlayerPrefs.GetString("ShowWhispersInGlobalStatus");

        //Do something with time stamp results
        if (timeStampAnswer == "true")
        {
            showTimeStamps.isOn = true;
            timeStampStatus = true;
        }
        if (timeStampAnswer == "false")
        {
            showTimeStamps.isOn = false;
            timeStampStatus = false;
        }
        //Do something with whispers in general results
        if (showWhispersInGlobalAnswer == "true" || showWhispersInGlobalAnswer == "")
        {
            showWhispersInGlobal.isOn = true;
            showWhispersInGlobalStatus = true;
        }
        if (showWhispersInGlobalAnswer == "false")
        {
            showWhispersInGlobal.isOn = false;
            showWhispersInGlobalStatus = false;
        }

    }

    public void ShowTimeStamps()
    {
        if (showTimeStamps.isOn == true)
        {
            timeStampStatus = true;
            PlayerPrefs.SetString("TimeStampStatus", "true");
        }
        else
        {
            timeStampStatus = false;
            PlayerPrefs.SetString("TimeStampStatus", "false");
        }
    }

    public void ShowWhispersInGlobal()
    {
        if (showWhispersInGlobal.isOn == true)
        {
            showWhispersInGlobalStatus = true;
            PlayerPrefs.SetString("ShowWhispersInGlobalStatus", "true");
        }
        else
        {
            showWhispersInGlobalStatus = false;
            PlayerPrefs.SetString("ShowWhispersInGlobalStatus", "false");
        }
    }

    public void ExitButton()
    {
        MenuManager.Singleton.settingsMenu.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlertMenu : MonoBehaviour
{
    public TMP_Text _alertText;

    public void ConfirmButton()
    {
        _alertText.text = "";
        MenuManager.Singleton.alertMenu.SetActive(false);
    }
}

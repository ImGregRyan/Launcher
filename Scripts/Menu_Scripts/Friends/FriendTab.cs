using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendTab : MonoBehaviour
{
    public void ChatTabClicked()
    {
        if (name == "Friends")
        {
            FriendManager.Singleton.FriendBoxContainer.SetActive(true);
            FriendManager.Singleton.RequestBoxContainer.SetActive(false);
        }
        else
        {
            FriendManager.Singleton.FriendBoxContainer.SetActive(false);
            FriendManager.Singleton.RequestBoxContainer.SetActive(true);
        }
    }
}

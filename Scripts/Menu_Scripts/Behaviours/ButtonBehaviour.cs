using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image buttonImage;

    public Sprite staticButtonImage;
    public Sprite hoveredButtonImage;
    public Sprite clickedButtonImage;

    public bool activeNotification;
    public Sprite notificationImage;
    public Sprite hoveredNotificationImage;

    public GameObject arrowDownImage;
    public GameObject arrowLeftImage;
    public bool itemsCollasped;

    private bool mouseOver = false;
    private bool mouseClicked = false;

    void Start()
    {
        buttonImage.sprite = staticButtonImage;
    }

    void Update()
    {
        // Notification behaviour
        if (activeNotification)
        {
            buttonImage.sprite = notificationImage;
        }
        if (mouseOver & !mouseClicked & activeNotification)
        {
            buttonImage.sprite = hoveredNotificationImage;
        }

        // Normal button behaviour
        if (mouseOver & !mouseClicked & !activeNotification)
        {
            buttonImage.sprite = hoveredButtonImage;
        }

        if (!mouseOver & !activeNotification)
        {
            buttonImage.sprite = staticButtonImage;
        }

        if (mouseOver & mouseClicked)
        {
            buttonImage.sprite = clickedButtonImage;
            activeNotification = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        mouseClicked = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        mouseClicked = false;
    }

    public void MoveOnlineFriendItems()
    {

        OnlineFriends[] items = (
        from directChild in transform.parent.GetComponentsInChildren<OnlineFriends>(true)
        where directChild.transform.name != "TitleButton"
        select directChild).ToArray();

        if(itemsCollasped == true)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(true);
            }
            arrowDownImage.SetActive(true);
            arrowLeftImage.SetActive(false);

            itemsCollasped = false;
            return;
        }

        if (itemsCollasped == false)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(false);
            }
            arrowDownImage.SetActive(false);
            arrowLeftImage.SetActive(true);

            itemsCollasped = true;
            return;
        }

    }
    public void MoveOfflineFriendItems()
    {

        OfflineFriends[] items = (
        from directChild in transform.parent.GetComponentsInChildren<OfflineFriends>(true)
        where directChild.transform.name != "TitleButton"
        select directChild).ToArray();

        if (itemsCollasped == true)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(true);
            }
            arrowDownImage.SetActive(true);
            arrowLeftImage.SetActive(false);

            itemsCollasped = false;
            return;
        }

        if (itemsCollasped == false)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(false);
            }
            arrowDownImage.SetActive(false);
            arrowLeftImage.SetActive(true);

            itemsCollasped = true;
            return;
        }

    }
    public void MoveIgnoredItems()
    {

        Ignored[] items = (
        from directChild in transform.parent.GetComponentsInChildren<Ignored>(true)
        where directChild.transform.name != "TitleButton"
        select directChild).ToArray();

        if (itemsCollasped == true)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(true);
            }
            arrowDownImage.SetActive(true);
            arrowLeftImage.SetActive(false);

            itemsCollasped = false;
            return;
        }

        if (itemsCollasped == false)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(false);
            }
            arrowDownImage.SetActive(false);
            arrowLeftImage.SetActive(true);

            itemsCollasped = true;
            return;
        }

    }
}
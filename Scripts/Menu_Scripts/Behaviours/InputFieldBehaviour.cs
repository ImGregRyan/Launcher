using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputFieldBehaviour : MonoBehaviour, ISelectHandler
{
    public TMP_InputField inputField;

    //void Start()
    //{
    //    inputField = gameObject.GetComponent<TMP_InputField>();
    //}

    // Called by code version if needed
    public void OnSelect()
    {
        StartCoroutine(disableHighlight());
    }

    // Called on mouse click events picked up by unity
    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(disableHighlight());
    }

    //Sets carrot to the end of the text
    IEnumerator disableHighlight()
    {
        Debug.Log("Selected!");

        //Get original selection color
        Color originalTextColor = inputField.selectionColor;
        //Remove alpha
        originalTextColor.a = 0f;

        //Apply new selection color without alpha
        inputField.selectionColor = originalTextColor;

        //Wait one Frame(MUST DO THIS!)
        yield return null;

        //Change the caret pos to the end of the text
        //inputField.caretPosition = inputField.text.Length;
        inputField.MoveTextEnd(false);

        //Return alpha
        originalTextColor.a = 1f;

        //Apply new selection color with alpha
        inputField.selectionColor = originalTextColor;
    }
}

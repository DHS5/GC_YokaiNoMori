using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardBackground : MonoBehaviour,
    IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        HumanController.CancelYokaiInput();
        Board.TryHideOptions();
    }
}

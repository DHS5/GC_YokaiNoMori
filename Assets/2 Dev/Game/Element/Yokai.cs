using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Yokai : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Global Members

    [Header("Yokai")]
    [SerializeField] private int playerIndex;
    [SerializeField] private YokaiData data;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public int PlayerIndex => playerIndex;

    public int YokaiIndex => data.Index;
    public Vector2Int StartPosition => data.StartPosition;

    #endregion

    #region Pointer Interfaces

    public void OnPointerClick(PointerEventData eventData)
    {
        HumanController.HumanYokaiInput(this, OnSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover begin");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hover end");
    }

    #endregion

    #region Selection

    private void OnSelected()
    {
        Debug.Log("On selected");
    }

    #endregion
}

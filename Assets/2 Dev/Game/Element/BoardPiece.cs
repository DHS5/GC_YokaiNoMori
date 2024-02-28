using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardPiece : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum State
    {
        NORMAL = 0,
        VALID = 1,
        UNVALID = 2,
        HOVERED = 3,
    }

    #region Global Members

    [Header("Board Piece")]
    [SerializeField] private Vector2Int position;
    [SerializeField] private BoardPieceData data;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;


    public Vector2Int Position => position;

    #endregion

    #region Appearance Methods

    public void SetState(State state)
    {
        spriteRenderer.sprite = data.GetSprite(state);
    }

    #endregion

    #region Position

    public void SetPosition(Vector2Int _position)
    {
        position = _position;
    }

    #endregion

    #region Pointer Interfaces

    public void OnPointerClick(PointerEventData eventData)
    {
        HumanController.HumanBoardPieceInput(this, OnSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetState(State.HOVERED);
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

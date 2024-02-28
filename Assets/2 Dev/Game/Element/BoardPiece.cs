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

    private State _currentState = State.NORMAL;
    public void SetState(State state)
    {
        _currentState = state;
        VerifyState();
    }
    private void VerifyState()
    {
        if (spriteRenderer != null && data != null)
        {
            spriteRenderer.color = (_currentState == State.VALID && _isHovered) ? data.HoveredColor : data.GetColor(_currentState);
        }
    }

    #endregion

    #region Position

    public void SetPosition(Vector2Int _position)
    {
        position = _position;
    }

    #endregion

    #region Pointer Interfaces

    private bool _isHovered = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        HumanController.BoardPieceInput(this, OnSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        VerifyState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        VerifyState();
    }

    #endregion

    #region Selection

    private void OnSelected()
    {
        Debug.Log("On selected " + this, this);
    }

    #endregion


    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        SetState(State.NORMAL);
    }

#endif

    #endregion
}

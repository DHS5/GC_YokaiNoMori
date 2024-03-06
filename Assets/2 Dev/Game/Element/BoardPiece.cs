using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using YokaiNoMori.Interface;

public class BoardPiece : MonoBehaviour, IBoardCase,
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
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private SpriteRenderer selectSpriteRenderer;


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
        if (data != null)
        {
            if (mainSpriteRenderer != null)
                mainSpriteRenderer.color = data.GetColor(_currentState);
            if (selectSpriteRenderer != null)
            {
                bool validAndHovered = _isHovered && _currentState == State.VALID;
                selectSpriteRenderer.color = validAndHovered ? data.HoveredColor : Color.white;
                selectSpriteRenderer.transform.localScale = Vector3.one * (validAndHovered ? 1.05f : 1f);
            }
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
        if (_currentState == State.VALID)
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
        //Debug.Log("On selected " + this, this);
    }

    #endregion


    #region IBoardCase

    /// <summary>
    /// Retrieves its position in a two-dimensional array.
    /// [0,0] being the first cell at bottom left.
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetPosition() => Position;


    /// <summary>
    /// Retrieves the pawn on this case
    /// </summary>
    /// <returns></returns>
    public IPawn GetPawnOnIt() => Board.GetYokaiAtPosition(Position);

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

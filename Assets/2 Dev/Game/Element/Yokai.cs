using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Yokai : MonoBehaviour, IPawn,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Global Members

    [Header("Yokai")]
    [SerializeField] private int playerIndex;
    [SerializeField] private YokaiData data;
    [SerializeField] private Vector2Int startPosition;

    [Header("References")]
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private SpriteRenderer outlineSpriteRenderer;

    [Header("Color")]
    [SerializeField] private Color hoveredColor;
    [SerializeField] private Color selectedColor;

    public int PlayerIndex
    {
        get => playerIndex;
        set => playerIndex = value;
    }

    public bool IsKing => data.IsKing;
    public Vector2Int StartPosition => startPosition;

    private int _yokaiIndex;
    public int YokaiIndex
    {
        get
        {
            if (_yokaiIndex == 0) _yokaiIndex = (playerIndex - 1) + data.Index;
            return _yokaiIndex;
        }
    }

    #endregion

    #region Core Behaviour

    private void Start()
    {
        SetSprite();
    }

    private void OnEnable()
    {
        GameManager.OnSetTurn += OnSetTurn;
        GameManager.OnYokaiSelected += CheckSelectability;
        GameManager.OnYokaiDeselected += CheckSelectability;
    }
    private void OnDisable()
    {
        GameManager.OnSetTurn -= OnSetTurn;
        GameManager.OnYokaiSelected -= CheckSelectability;
        GameManager.OnYokaiDeselected -= CheckSelectability;
    }

    #endregion

    #region Pointer Interfaces

    private bool _isHovered;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.CurrentPlayer == PlayerIndex && _hasValidNextPositions)
            HumanController.YokaiInput(this, OnSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        if (GameManager.CurrentPlayer == PlayerIndex)
        {
            if (!_computedNextPositions) Board.TryComputeNextPositions(this);
            if (_hasValidNextPositions) Board.TryShowNextPositions(_nextPositions);
        }
        SetOutline();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        SetOutline();
        if (!IsSelected)
            Board.TryHideOptions();
    }

    #endregion

    #region Selection

    private bool _isSelectable;
    public bool IsSelected { get; private set; }

    private void OnSetTurn(int _playerIndex)
    {
        _isSelectable = _playerIndex == PlayerIndex;
        _computedNextPositions = false;
        _hasValidNextPositions = false;
        SetOutline();
    }
    private void CheckSelectability()
    {
        _isSelectable = PlayerIndex == GameManager.CurrentPlayer;
        SetOutline();
    }

    private void OnSelected()
    {
        IsSelected = true;
        SetOutline();
    }
    public void Deselect()
    {
        IsSelected = false;
        SetOutline();
    }

    #endregion

    #region Position & Options

    public Vector2Int CurrentPosition { get; set; }

    private List<Vector2Int> _validDeltas = new();
    public List<Vector2Int> ValidDeltas
    {
        get
        {
            if (_validDeltas.Count == 0) ComputeDeltas();
            return _validDeltas;
        }
    }

    private void ComputeDeltas()
    {
        if (PlayerIndex == 1)
        {
            _validDeltas = data.GetValidDeltas(_isOnSecondFace);
            return;
        }

        _validDeltas.Clear();
        foreach (var delta in data.GetValidDeltas(_isOnSecondFace))
        {
            _validDeltas.Add(-delta);
        }
    }

    public bool CanEat(Vector2Int position)
    {
        Vector2Int delta = position - CurrentPosition;
        return ValidDeltas.Contains(delta);
    }

    private bool _computedNextPositions = false;
    private bool _hasValidNextPositions = false;
    private List<Vector2Int> _nextPositions = new();
    public void SetValidNextPositions(List<Vector2Int> nextPositions)
    {
        _computedNextPositions = true;
        _nextPositions = new(nextPositions);
        _hasValidNextPositions = _nextPositions.Count > 0;
    }

    #endregion

    #region Second Face

    private bool _isOnSecondFace = false;
    public void OnArriveOnLastRow()
    {
        if (data.HasSecondFace)
        {
            _isOnSecondFace = true;
            SetSprite();
            ComputeDeltas();
        }
    }

    #endregion

    #region Cemetery
    
    public void OnSentToCemetery()
    {
        if (_isOnSecondFace)
        {
            _isOnSecondFace = false;
            SetSprite();
        }
        ComputeDeltas();
    }

    #endregion


    #region IPawn

    /// <summary>
    /// Recovers all moves of the pawn concerned
    /// </summary>
    /// <returns>All possible directions (e.g. [0.1] for forward)</returns>
    public List<Vector2Int> GetDirections() => data.GetValidDeltas(_isOnSecondFace);

    /// <summary>
    /// Retrieves the owner of this pawn
    /// </summary>
    /// <returns>1 : Player 1 - 2 : Player 2</returns>
    public ICompetitor GetCurrentOwner() => PlayerManager.GetPlayerByIndex(playerIndex);

    /// <summary>
    /// Retrieve the square on which the pawn is located
    /// </summary>
    /// <returns>null is no case</returns>
    public IBoardCase GetCurrentBoardCase() => Board.GetBoardPiece(CurrentPosition);


    /// <summary>
    /// Retrieve pawn type
    /// </summary>
    /// <returns></returns>
    public EPawnType GetPawnType() => data.GetPawnType(_isOnSecondFace);

    public Vector2Int GetCurrentPosition()
    {
        return CurrentPosition;
    }

    #endregion


    #region Appearance

    private void SetSprite()
    {
        if (mainSpriteRenderer != null && data != null)
        {
            mainSpriteRenderer.sprite = data.GetSprite(_isOnSecondFace);
        }
    }
    private void SetOutline()
    {
        outlineSpriteRenderer.enabled = _isSelectable || IsSelected;
        outlineSpriteRenderer.color = IsSelected ? selectedColor : !_isHovered ? Color.white : _hasValidNextPositions ? hoveredColor : Color.grey;
    }

    #endregion


    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        SetSprite();
    }

#endif

    #endregion
}

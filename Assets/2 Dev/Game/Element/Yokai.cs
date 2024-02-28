using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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

    public int PlayerIndex
    {
        get => playerIndex;
        set => playerIndex = value;
    }

    public bool IsKing => data.IsKing;
    public Vector2Int StartPosition => data.StartPosition;

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

    #endregion

    #region Pointer Interfaces

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.CurrentPlayer == PlayerIndex)
            HumanController.YokaiInput(this, OnSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.CurrentPlayer == PlayerIndex)
        {
            Board.TryShowOptions(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected)
            Board.TryHideOptions();
    }

    #endregion

    #region Selection

    public bool IsSelected { get; private set; }

    private void OnSelected()
    {
        IsSelected = true;
    }
    public void Deselect()
    {
        IsSelected = false;
    }

    #endregion

    #region Position & Options

    public Vector2Int CurrentPosition { get; set; }

    private List<Vector2Int> _validDeltas;
    public List<Vector2Int> ValidDeltas
    {
        get
        {
            if (_validDeltas == null) _validDeltas = ComputeDeltas();
            return _validDeltas;
        }
    }

    private List<Vector2Int> ComputeDeltas()
    {
        if (PlayerIndex == 1) return data.GetValidDeltas();

        List<Vector2Int> deltas = new();
        foreach (var delta in data.GetValidDeltas())
        {
            deltas.Add(-delta);
        }

        return deltas;
    }

    public bool CanEat(Vector2Int position)
    {
        Vector2Int delta = position - CurrentPosition;
        return ValidDeltas.Contains(delta);
    }

    #endregion


    #region Appearance

    private void SetSprite()
    {
        if (spriteRenderer != null && data != null) spriteRenderer.sprite = data.Sprite;
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

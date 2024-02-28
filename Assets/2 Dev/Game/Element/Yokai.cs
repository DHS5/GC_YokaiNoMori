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

    public int YokaiIndex => (playerIndex - 1) + data.Index;
    public Vector2Int StartPosition => data.StartPosition;

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

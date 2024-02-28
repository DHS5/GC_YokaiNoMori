using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Input Struct

    public struct Input
    {
        public Yokai yokai;
        public Vector2Int newPosition;
    }

    #endregion

    #region Global Members

    [Header("Player")]
    [SerializeField] private int index;

    public int Index => index;

    #endregion

    #region Accessors

    public bool IsPlaying => GameManager.CurrentPlayer == Index;

    #endregion
}

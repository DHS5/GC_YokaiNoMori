using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public enum Mode
    {
        F3x4 = 0,
        F5x6 = 1
    }

    #region Structs

    [Serializable]
    private struct BoardLine4x3
    {
        [SerializeField] private BoardPiece leftPiece;
        [SerializeField] private BoardPiece centerPiece;
        [SerializeField] private BoardPiece rightPiece;

        public BoardPiece Get(int index)
        {
            switch (index)
            {
                case 0: return leftPiece;
                case 1: return centerPiece;
                case 2: return rightPiece;
            }
            Debug.LogError("Invalid index");
            return null;
        }
    }
    
    [Serializable]
    private struct BoardStructure4x3
    {
        [SerializeField] private BoardLine4x3 line1;
        [SerializeField] private BoardLine4x3 line2;
        [SerializeField] private BoardLine4x3 line3;
        [SerializeField] private BoardLine4x3 line4;

        public BoardPiece Get(Vector2Int position) => Get(position.x, position.y);
        public BoardPiece Get(int column, int line)
        {
            switch (line)
            {
                case 0: return line1.Get(column);
                case 1: return line2.Get(column);
                case 2: return line3.Get(column);
                case 3: return line4.Get(column);
            }
            Debug.LogError("Invalid index");
            return null;
        }
    }

    #endregion

    #region Global Members

    [Header("Board Elements")]
    [SerializeField] private Mode mode;
    [SerializeField] private BoardStructure4x3 structure;
    [SerializeField] private Vector2 boardPieceSize = new Vector2(1,1);
    [SerializeField] private Vector2 boardPieceSpacing = new Vector2(0.25f,0.25f);

    [Header("Yokais")]
    [SerializeField] private List<Yokai> yokaiList;

    #endregion

    #region Board Structure

    private void RepositionBoardPieces()
    {
        for (int line = 0; line < 4; line++)
        {
            for (int column = 0; column < 3; column++)
            {
                structure.Get(column, line).transform.position 
                    = new Vector3((column - 1) * (boardPieceSize.x + boardPieceSpacing.x),
                    boardPieceSize.y / 2 + line * (boardPieceSize.y + boardPieceSpacing.y),
                    0);
            }
        }
    }

    private void SetBoardPiecesPosition()
    {
        for (int line = 0; line < 4; line++)
        {
            for (int column = 0; column < 3; column++)
            {
                structure.Get(column, line).SetPosition(new Vector2Int(column, line));
            }
        }
    }

    #endregion


    #region Board Datas

    private int[,] _board;
    private Dictionary<int, Yokai> _yokaiDico = new();

    private void InitBoard()
    {
        _format = Format(mode);

        
    }

    #endregion

    #region Board Format

    private Vector2Int _format;
    private Vector2Int Format(Mode mode)
    {
        return mode switch
        {
            Mode.F3x4 => new Vector2Int(3, 4),
            Mode.F5x6 => new Vector2Int(5, 6),
        };
    }

    #endregion


    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        RepositionBoardPieces();
        SetBoardPiecesPosition();
    }

#endif

    #endregion
}

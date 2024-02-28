using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    #region Singleton

    private static Board Instance { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitBoard();
    }

    #endregion

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
    [SerializeField] private Transform player1Cemetery;
    [SerializeField] private Transform player2Cemetery;

    [Space(10f)]

    [SerializeField] private Vector2 boardPieceSize = new Vector2(1,1);
    [SerializeField] private Vector2 boardPieceSpacing = new Vector2(0.25f,0.25f);

    [Header("Yokais")]
    [SerializeField] private List<Yokai> yokaiList;

    [Header("Animations")]
    [SerializeField] private float yokaiMoveDuration = 1f;

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
                    (line - 1.5f) * (boardPieceSize.y + boardPieceSpacing.y),
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

    private BoardPiece GetBoardPiece(Vector2Int position)
    {
        return structure.Get(position.x, position.y);
    }
    private BoardPiece GetBoardPiece(int posX, int posY)
    {
        return structure.Get(posX, posY);
    }

    #endregion


    #region Board Datas

    private int[,] _board;
    private Dictionary<int, List<Yokai>> _cemetery = new();
    private Dictionary<int, Yokai> _yokaiDico = new();

    private void InitBoard()
    {
        // Init data structures
        _format = Format(mode);
        _board = new int[_format.x, _format.y];
        _cemetery.Clear();
        _yokaiDico.Clear();
        
        // Fill with 0
        for (int line = 0; line < _format.y; line++)
        {
            for (int column = 0; column < _format.x; column++)
            {
                _board[column, line] = 0;
            }
        }

        // Populate yokai dico and board
        Vector2Int pos;
        foreach (var yokai in yokaiList)
        {
            if (!_yokaiDico.TryAdd(yokai.YokaiIndex, yokai))
            {
                Debug.LogError("Yokai index redundance : " + yokai.YokaiIndex);
            }
            pos = GetCoords(yokai.PlayerIndex, yokai.StartPosition);
            _board[pos.x, pos.y] = yokai.YokaiIndex;

            MoveYokaiToPosition(yokai, pos);
        }
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
            _ => throw new NotImplementedException(),
        };
    }

    private Vector2Int GetCoords(int playerIndex, Vector2Int position)
    {
        if (playerIndex == 1) return position;

        return new Vector2Int(_format.x - 1 - position.x, _format.y - 1 - position.y);
    }

    private Vector3 GetYokaiRotation(int playerIndex)
    {
        return playerIndex == 1 ? Vector3.zero : new Vector3(0, 0, 180);
    }
    
    private Transform GetYokaiCemetery(int playerIndex)
    {
        return playerIndex == 1 ? player1Cemetery : player2Cemetery;
    }

    #endregion


    #region Board Modification

    public static void TryMakeMove(Player.Input input, Action onComplete)
    {
        if (Exist())
        {
            Instance.MakeMove(input, onComplete);
        }
    }
    private void MakeMove(Player.Input input, Action onComplete)
    {

    }


    public static bool IsMoveValid(Vector2Int currentPosition, Vector2Int delta)
    {
        Vector2Int newPosition = new Vector2Int(currentPosition.x + delta.x, currentPosition.y + delta.y);
        return IsPositionValid(newPosition);
    }
    public static bool IsPositionValid(Vector2Int position)
    {
        if (Exist()) return position.x >= 0 && position.x < Instance._format.x && position.y >= 0 && position.y < Instance._format.y;
        return false;
    }

    #endregion

    #region Board Animation

    private void MoveYokaiToPosition(Yokai yokai, Vector2Int newPosition, Action onComplete = null)
    {
        MoveYokaiToAnchor(yokai, GetBoardPiece(newPosition).transform, onComplete);
    }
    private void MoveYokaiToCemetery(Yokai yokai, Action onComplete = null)
    {
        MoveYokaiToAnchor(yokai, GetYokaiCemetery(yokai.PlayerIndex), onComplete);
    }

    private void MoveYokaiToAnchor(Yokai yokai, Transform anchor, Action onComplete = null)
    {
        yokai.transform.DOMove(anchor.position, yokaiMoveDuration);
        yokai.transform.DORotate(GetYokaiRotation(yokai.PlayerIndex), yokaiMoveDuration);

        if (onComplete != null)
        {
            DOVirtual.DelayedCall(yokaiMoveDuration, () => onComplete.Invoke());
        }
    }

    #endregion


    #region Utility

    private static bool Exist()
    {
        if (Instance != null) return true;

        Debug.LogError("No board found in the scene");
        return false;
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

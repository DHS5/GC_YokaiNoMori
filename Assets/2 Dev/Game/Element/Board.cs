using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
        InitCemetery();
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

    private int GetYokaiIndexAtPosition(Vector2Int position)
    {
        return _board[position.x, position.y];
    }
    private void SetYokaiIndexAtPosition(int yokaiIndex, Vector2Int position)
    {
        _board[position.x, position.y] = yokaiIndex;
    }
    public static Yokai GetYokaiAtPosition(Vector2Int position)
    {
        if (Exist())
        {
            int yokaiIndex = Instance.GetYokaiIndexAtPosition(position);
            if (yokaiIndex == 0) return null;
            return Instance._yokaiDico[yokaiIndex];
        }
        return null;
    }
    public static Yokai GetYokaiAtPosition(int posX, int posY)
    {
        return GetYokaiAtPosition(new Vector2Int(posX, posY));
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


    #region Board Hint

    public static void TryShowOptions(Yokai yokai)
    {
        if (Exist())
        {
            Instance.ShowOptions(yokai);
        }
    }
    private void ShowOptions(Yokai yokai)
    {
        Vector2Int pos = yokai.CurrentPosition;
        if (pos == Vector2Int.down)
        {
            ShowAllEmpty();
            return;
        }

        List<Vector2Int> validPositions = new();

        Vector2Int temp;
        foreach (var delta in yokai.ValidDeltas)
        {
            temp = pos + delta;
            if (IsPositionValid(temp) && !ContainsPlayer(temp, yokai.PlayerIndex))
            {
                validPositions.Add(temp);
            }
        }

        BoardPiece boardPiece;
        bool around;
        for (int line = 0; line < _format.y; line++)
        {
            for (int column = 0; column < _format.x; column++)
            {
                boardPiece = structure.Get(column, line);
                around = AreAround(pos, boardPiece.Position);
                if (around)
                {
                    boardPiece.SetState(validPositions.Contains(boardPiece.Position) ? BoardPiece.State.VALID : BoardPiece.State.UNVALID);
                }
                else
                {
                    boardPiece.SetState(BoardPiece.State.NORMAL);
                }
            }
        }
    }
    private void ShowAllEmpty()
    {
        for (int line = 0; line < _format.y; line++)
        {
            for (int column = 0; column < _format.x; column++)
            {
                structure.Get(column, line)
                    .SetState(IsEmpty(new Vector2Int(column, line)) ? BoardPiece.State.VALID : BoardPiece.State.UNVALID);
            }
        }
    }
    public static void TryHideOptions()
    {
        if (Exist())
        {
            Instance.HideOptions();
        }
    }
    private void HideOptions()
    {
        for (int line = 0; line < _format.y; line++)
        {
            for (int column = 0; column < _format.x; column++)
            {
                structure.Get(column, line).SetState(BoardPiece.State.NORMAL);
            }
        }
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
        HideOptions();

        Yokai yokai = GetYokaiAtPosition(input.newPosition);
        if (yokai != null)
        {
            MoveYokaiToCemetery(yokai);
        }

        MoveYokaiToPosition(input.yokai, input.newPosition, onComplete);
    }

    #endregion

    #region Board Tests

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
    public static bool AreAround(Vector2Int position1, Vector2Int position2)
    {
        return Mathf.Abs(position1.x - position2.x) <= 1 && Mathf.Abs(position1.y - position2.y) <= 1;
    }
    public static bool ContainsPlayer(Vector2Int position, int playerIndex)
    {
        if (Exist())
        {
            int yokaiIndex = Instance.GetYokaiIndexAtPosition(position);
            return yokaiIndex != 0 && Instance._yokaiDico[yokaiIndex].PlayerIndex == playerIndex;
        }
        return false;
    }
    public static bool IsEmpty(Vector2Int position)
    {
        return Exist() && Instance.GetYokaiIndexAtPosition(position) == 0;
    }

    #endregion

    #region Board Animation

    private void MoveYokaiToPosition(Yokai yokai, Vector2Int newPosition, Action onComplete = null)
    {
        if (IsPositionValid(yokai.CurrentPosition))
            SetYokaiIndexAtPosition(0, yokai.CurrentPosition);

        yokai.CurrentPosition = newPosition;
        SetYokaiIndexAtPosition(yokai.YokaiIndex, newPosition);
        MoveYokaiToAnchor(yokai, GetBoardPiece(newPosition).transform, onComplete);
    }
    private void MoveYokaiToCemetery(Yokai yokai, Action onComplete = null)
    {
        yokai.PlayerIndex = yokai.PlayerIndex == 1 ? 2 : 1;

        AddToCemetery(yokai);
        yokai.CurrentPosition = Vector2Int.down;
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

    #region Cemetery

    private void InitCemetery()
    {
        _cemetery.Clear();
        _cemetery.Add(1, new List<Yokai>());
        _cemetery.Add(2, new List<Yokai>());
    }
    private void AddToCemetery(Yokai yokai)
    {
        _cemetery[yokai.PlayerIndex].Add(yokai);
    }
    private void RemoveFromCemetery(Yokai yokai)
    {
        _cemetery[yokai.PlayerIndex].Remove(yokai);
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

    #region Debug

    private void DebugBoard()
    {
        for (int line = 0; line < _format.y; line++)
        {
            for (int column = 0; column < _format.x; column++)
            {

            }
        }
    }

    #endregion
}

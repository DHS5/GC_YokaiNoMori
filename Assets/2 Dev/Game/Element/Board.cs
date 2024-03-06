using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

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

    #region Structure Base

    public interface IBoardLine
    {
        public BoardPiece Get(int index);
    }
    public interface IBoardStructure
    {
        public Vector2Int Format { get; }

        public BoardPiece Get(Vector2Int position);
        public BoardPiece Get(int column, int line);

        public bool IsPromotionZoneForPlayer(Vector2Int position, int playerIndex);

        public bool IsPositionValid(Vector2Int position)
        {
            return IsPositionValid(position.x, position.y);
        }
        public bool IsPositionValid(int posX, int posY)
        {
            return posX >= 0 && posX < Format.x && posY >= 0 && posY < Format.y;
        }

        public Vector2Int GetCorrectCoordsForPlayer(Vector2Int position, int playerIndex)
        {
            if (playerIndex == 1) return position;

            return new Vector2Int(Format.x - 1 - position.x, Format.y - 1 - position.y);
        }
    }

    #endregion

    #region Structure 4x3

    [Serializable]
    private struct BoardLine4x3 : IBoardLine
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
    private struct BoardStructure4x3 : IBoardStructure
    {
        [SerializeField] private BoardLine4x3 line1;
        [SerializeField] private BoardLine4x3 line2;
        [SerializeField] private BoardLine4x3 line3;
        [SerializeField] private BoardLine4x3 line4;

        public Vector2Int Format => new Vector2Int(3, 4);

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

        public bool IsPromotionZoneForPlayer(Vector2Int position, int playerIndex)
        {
            return (position.y == 3 && playerIndex == 1)
                || (position.y == 0 && playerIndex == 2);
        }
    }

    #endregion

    #region Structure 6x5

    [Serializable]
    private struct BoardLine6x5 : IBoardLine
    {
        [SerializeField] private BoardPiece leftPiece;
        [SerializeField] private BoardPiece leftCenterPiece;
        [SerializeField] private BoardPiece centerPiece;
        [SerializeField] private BoardPiece rightCenterPiece;
        [SerializeField] private BoardPiece rightPiece;

        public BoardPiece Get(int index)
        {
            switch (index)
            {
                case 0: return leftPiece;
                case 1: return leftCenterPiece;
                case 2: return centerPiece;
                case 3: return rightCenterPiece;
                case 4: return rightPiece;
            }
            Debug.LogError("Invalid index");
            return null;
        }
    }

    [Serializable]
    private struct BoardStructure6x5 : IBoardStructure
    {
        [SerializeField] private BoardLine6x5 line1;
        [SerializeField] private BoardLine6x5 line2;
        [SerializeField] private BoardLine6x5 line3;
        [SerializeField] private BoardLine6x5 line4;
        [SerializeField] private BoardLine6x5 line5;
        [SerializeField] private BoardLine6x5 line6;

        public Vector2Int Format => new Vector2Int(5, 6);

        public BoardPiece Get(Vector2Int position) => Get(position.x, position.y);
        public BoardPiece Get(int column, int line)
        {
            switch (line)
            {
                case 0: return line1.Get(column);
                case 1: return line2.Get(column);
                case 2: return line3.Get(column);
                case 3: return line4.Get(column);
                case 4: return line5.Get(column);
                case 5: return line6.Get(column);
            }
            Debug.LogError("Invalid index");
            return null;
        }

        public bool IsPromotionZoneForPlayer(Vector2Int position, int playerIndex)
        {
            return ((position.y == 4 || position.y == 5) && playerIndex == 1)
                || ((position.y == 0 || position.y == 1) && playerIndex == 2);
        }
    }

    #endregion

    #region Global Members

    [Header("Board Elements")]
    [SerializeField] private Mode mode;
    [SerializeField] private BoardStructure4x3 structure4x3;
    [SerializeField] private BoardStructure6x5 structure6x5;
    [SerializeField] private Transform[] player1Cemetery;
    [SerializeField] private Transform[] player2Cemetery;

    [Space(10f)]

    [SerializeField] private Vector2 boardPieceSize = new Vector2(1,1);
    [SerializeField] private Vector2 boardPieceSpacing = new Vector2(0.25f,0.25f);

    [Header("Yokais")]
    [SerializeField] private List<Yokai> yokaiList;

    [Header("Animations")]
    [SerializeField] private float yokaiMoveDuration = 1f;


    private IBoardStructure Structure => mode == Mode.F3x4 ? structure4x3 : structure6x5;

    #endregion

    #region Board Structure

    private void RepositionBoardPieces()
    {
        for (int line = 0; line < Structure.Format.y; line++)
        {
            for (int column = 0; column < Structure.Format.x; column++)
            {
                Structure.Get(column, line).transform.position 
                    = new Vector3((column - (Structure.Format.x / 2)) * (boardPieceSize.x + boardPieceSpacing.x),
                    (line - (Structure.Format.y / 2 - 0.5f)) * (boardPieceSize.y + boardPieceSpacing.y),
                    0);
            }
        }
    }

    private void SetBoardPiecesPosition()
    {
        for (int line = 0; line < Structure.Format.y; line++)
        {
            for (int column = 0; column < Structure.Format.x; column++)
            {
                Structure.Get(column, line).SetPosition(new Vector2Int(column, line));
            }
        }
    }

    public static BoardPiece GetBoardPiece(Vector2Int position)
    {
        return Instance.Structure.Get(position.x, position.y);
    }
    public static BoardPiece GetBoardPiece(int posX, int posY)
    {
        return Instance.Structure.Get(posX, posY);
    }

    public static Vector2Int Format => Instance.Structure.Format;

    #endregion


    #region Board Datas

    private int[,] _board;
    private Dictionary<int, List<Yokai>> _cemetery = new();
    private Dictionary<int, Yokai> _yokaiDico = new();

    private void InitBoard()
    {
        _board = new int[Format.x, Format.y];
        InitCemetery();
        _yokaiDico.Clear();
        
        // Fill with 0
        for (int line = 0; line < Format.y; line++)
        {
            for (int column = 0; column < Format.x; column++)
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
            pos = Structure.GetCorrectCoordsForPlayer(yokai.StartPosition, yokai.PlayerIndex);
            SetYokaiIndexAtPosition(yokai.YokaiIndex, pos);
            yokai.CurrentPosition = pos;

            MoveYokaiToPosition(yokai, pos);
        }

        BoardRegister.Init();
        //DebugBoard();
    }

    public static int[,] GetCurrentBoard()
    {
        return Instance._board;
    }

    public static Yokai GetYokaiByIndex(int index)
    {
        if (index == 0) return null;
        return Instance._yokaiDico[index];
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
        int yokaiIndex = Instance.GetYokaiIndexAtPosition(position);
        if (yokaiIndex == 0) return null;
        return Instance._yokaiDico[yokaiIndex];
    }
    public static Yokai GetYokaiAtPosition(int posX, int posY)
    {
        return GetYokaiAtPosition(new Vector2Int(posX, posY));
    }

    #endregion

    #region Board Format

    private Vector3 GetYokaiRotation(int playerIndex)
    {
        return playerIndex == 1 ? Vector3.zero : new Vector3(0, 0, 180);
    }
    
    private Transform GetYokaiCemetery(int playerIndex)
    {
        return GetYokaiCemetery(playerIndex, _cemetery[playerIndex].Count);
    }
    private Transform GetYokaiCemetery(int playerIndex, int cemeteryIndex)
    {
        return playerIndex == 1 ? player1Cemetery[cemeteryIndex] : player2Cemetery[cemeteryIndex];
    }

    #endregion


    #region Board Hint

    public static void TryComputeNextPositions(Yokai yokai)
    {
        Instance.ComputeNextPositions(yokai);
    }
    private void ComputeNextPositions(Yokai yokai)
    {
        Vector2Int pos = yokai.CurrentPosition;
        if (pos == Vector2Int.down)
        {
            yokai.SetValidNextPositions(GetAllEmpty());
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

        yokai.SetValidNextPositions(validPositions);
    }
    public static void TryShowNextPositions(List<Vector2Int> nextPositions)
    {
        Instance.ShowNextPositions(nextPositions);
    }
    private void ShowNextPositions(List<Vector2Int> nextPositions)
    {
        BoardPiece boardPiece;
        for (int line = 0; line < Format.y; line++)
        {
            for (int column = 0; column < Format.x; column++)
            {
                boardPiece = GetBoardPiece(column, line);
                if (nextPositions.Contains(boardPiece.Position))
                {
                    boardPiece.SetState(BoardPiece.State.VALID);
                }
                else
                {
                    boardPiece.SetState(BoardPiece.State.NORMAL);
                }
            }
        }
    }

    private List<Vector2Int> GetAllEmpty()
    {
        List<Vector2Int> emptyPositions = new();
        Vector2Int pos;
        for (int line = 0; line < Format.y; line++)
        {
            for (int column = 0; column < Format.x; column++)
            {
                pos = new Vector2Int(column, line);
                if (IsEmpty(pos))
                {
                    emptyPositions.Add(pos);
                }
            }
        }
        return emptyPositions;
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
        for (int line = 0; line < Format.y; line++)
        {
            for (int column = 0; column < Format.x; column++)
            {
                GetBoardPiece(column, line).SetState(BoardPiece.State.NORMAL);
            }
        }
    }

    #endregion

    #region Board Modification

    public static void TryMakeMove(Move move, Action onComplete)
    {
        Instance.MakeMove(move, onComplete);
    }
    private void MakeMove(Move move, Action onComplete)
    {
        HideOptions();

        Yokai yokai = GetYokaiAtPosition(move.newPosition);
        if (yokai != null)
        {
            SendYokaiToCemetery(yokai);
        }

        SetYokaiNewPosition(move.yokai, move.newPosition, onComplete);

        BoardRegister.Register();
    }

    private void SetYokaiNewPosition(Yokai yokai, Vector2Int newPosition, Action onComplete = null)
    {
        bool parachute = !IsPositionValid(yokai.CurrentPosition);
        if (parachute)
            RemoveFromCemetery(yokai);
        else
            SetYokaiIndexAtPosition(0, yokai.CurrentPosition);

        yokai.CurrentPosition = newPosition;
        SetYokaiIndexAtPosition(yokai.YokaiIndex, newPosition);

        if (!parachute && Structure.IsPromotionZoneForPlayer(yokai.CurrentPosition, yokai.PlayerIndex))
        {
            yokai.OnArriveOnLastRow();
        }

        MoveYokaiToPosition(yokai, newPosition, onComplete);
    }

    private void SendYokaiToCemetery(Yokai yokai)
    {
        yokai.PlayerIndex = yokai.PlayerIndex == 1 ? 2 : 1;

        yokai.OnSentToCemetery();
        yokai.CurrentPosition = Vector2Int.down;

        MoveYokaiToCemetery(yokai);
        AddToCemetery(yokai);

        if (yokai.IsKing)
        {
            GameManager.Winner(yokai.PlayerIndex);
        }
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
        return Instance.Structure.IsPositionValid(position);
    }
    public static bool IsPositionValid(int posX, int posY)
    {
        return Instance.Structure.IsPositionValid(posX, posY);
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
        MoveYokaiToAnchor(yokai, GetBoardPiece(newPosition).transform.position, onComplete);
    }
    private void MoveYokaiToCemetery(Yokai yokai)
    {
        MoveYokaiToAnchor(yokai, GetYokaiCemetery(yokai.PlayerIndex).position);
    }

    private void MoveYokaiToAnchor(Yokai yokai, Vector3 anchor, Action onComplete = null)
    {
        yokai.transform.DOMove(anchor, yokaiMoveDuration);
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

        if (_cemetery.TryGetValue(yokai.PlayerIndex, out List<Yokai> list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                MoveYokaiToAnchor(list[i], GetYokaiCemetery(yokai.PlayerIndex, i).position);
            }
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

    #region Debug

    private void DebugBoard()
    {
        StringBuilder sb = new();
        for (int line = Format.y - 1; line >= 0; line--)
        {
            for (int column = 0; column < Format.x; column++)
            {
                sb.Append(GetYokaiIndexAtPosition(new Vector2Int(column, line)));
                sb.Append(' ');
            }
            sb.AppendLine();
        }

        sb.Append("Cemetery 1 : ");
        foreach (var yokai in _cemetery[1])
        {
            sb.Append(yokai.YokaiIndex);
            sb.Append(' ');
        }
        sb.AppendLine();

        sb.Append("Cemetery 2 : ");
        foreach (var yokai in _cemetery[2])
        {
            sb.Append(yokai.YokaiIndex);
            sb.Append(' ');
        }

        Debug.Log(sb.ToString());
    }

    #endregion
}

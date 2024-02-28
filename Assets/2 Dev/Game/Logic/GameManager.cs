using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Init();
    }

    #endregion

    #region Global Members

    [Header("Physic Interactions")]
    [SerializeField] private Physics2DRaycaster physicsRaycaster;
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private LayerMask yokaiLayer;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraRotationDuration = 2f;


    public static bool IsPlaying { get; private set; }

    #endregion

    #region Core Behaviour

    private void Init()
    {

    }

    private void Start()
    {
        OnGameStart?.Invoke();

        IsPlaying = true;
        SetTurn(1);
    }

    #endregion

    #region Game Events

    public static event Action OnGameStart;
    public static event Action<int> OnSetTurn;

    #endregion

    #region Static Accessors

    public static int CurrentPlayer { get; private set; }

    #endregion

    #region Game Flow

    private int _nextTurn;
    private void ChangeTurn() => SetTurn(_nextTurn);
    private void SetTurn(int playerIndex)
    {
        CurrentPlayer = playerIndex;
        _nextTurn = CurrentPlayer == 1 ? 2 : 1;

        bool currentPlayerHuman = PlayerManager.CurrentPlayer.IsHuman;
        EnablePhysicInteractions(currentPlayerHuman);
        if (currentPlayerHuman)
        {
            SetYokaiLayer();
        }

        OnSetTurn?.Invoke(CurrentPlayer);
    }
    

    public static void PlayerInput(Player.Input input)
    {
        if (Exist())
        {
            Instance.OnPlayerInput(input);
        }
    }
    private void OnPlayerInput(Player.Input input)
    {
        // Deactivate input
        EnablePhysicInteractions(false);

        // Set player to 0
        CurrentPlayer = 0;

        // Effectuate movement
        Board.TryMakeMove(input, CheckForWinner);
    }

    private void CheckForWinner()
    {
        if (!IsPlaying) return;

        if (HasWinner(Board.GetCurrentBoard(), out int winner))
        {
            Winner(winner);
            return;
        }

        ChangeSide();
    }

    private void ChangeSide()
    {
        // Rotate camera
        RotateCamera(ChangeTurn);
    }


    public static void SelectYokai()
    {
        if (Exist())
        {
            Instance.SetBoardLayer();
        }
    }
    public static void DeselectYokai()
    {
        if (Exist())
        {
            Instance.SetYokaiLayer();
        }
    }

    #endregion


    #region Game Rules

    private bool HasWinner(int[,] board, out int winner)
    {
        var format = Board.GetFormat();
        var formatX = format.x;
        var formatY = format.y;
        
        // check first and last line for kings
        for (var step = 0; step <= 1; step++)
        {
            var line = step == 0 ? formatY - 1 : 0;
            var playerToCheck = step == 0 ? 1 : 2;
        
            // search last line for kings
            for (var column = 0; column < formatX; column++)
            {
                var yokai = Board.GetYokaiByIndex(board[column, line]);
                if (yokai == null || !yokai.IsKing || yokai.PlayerIndex != playerToCheck) continue;
                
                // king found !
                
                // check around the king if there is a piece of the other player that can reach the king
                if (IsKingInDanger(board, playerToCheck, yokai))
                {
                    // The king is in danger
                    winner = playerToCheck == 1 ? 2 : 1;
                }
                else
                {
                    // The king is all the way to the end and there is no piece that can eat it
                    winner = playerToCheck;                   
                }
                return true;
            }            
        }
        // No winner
        winner = 0;
        return false;
    }

    private static bool IsKingInDanger(int[,] board, int playerToCheck, Yokai kingPiece)
    {
        var xKing = kingPiece.CurrentPosition.x;
        var yKing = kingPiece.CurrentPosition.y;
        for (var lineOffset = -1; lineOffset <= 1; lineOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (lineOffset == 0 && columnOffset == 0) continue;
                if (!Board.IsPositionValid(xKing + columnOffset, yKing + lineOffset)) continue;

                var piece = Board.GetYokaiByIndex(board[xKing + columnOffset, yKing + lineOffset]);
                if (piece != null && piece.PlayerIndex != playerToCheck && piece.CanEat(kingPiece.CurrentPosition))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int CheckPlayerWinner(int playerIndex, int[,] board, int lineToSearch, int columnNumber)
    {
        Yokai yokai;
        bool foundKing = false;
        Vector2Int kingPos = Vector2Int.zero;
        for (int column = 0; column < columnNumber; column++)
        {
            yokai = Board.GetYokaiByIndex(board[column, lineToSearch]);
            if (yokai != null && yokai.PlayerIndex == playerIndex && yokai.IsKing)
            {
                kingPos = new Vector2Int(column, lineToSearch);
                foundKing = true;
                break;
            }
        }

        if (foundKing)
        {
            for (int column = 0; column < columnNumber; column++)
            {
                yokai = Board.GetYokaiByIndex(board[column, lineToSearch]);
                if (yokai != null && yokai.CanEat(kingPos))
                {
                    return yokai.PlayerIndex;
                }
                yokai = Board.GetYokaiByIndex(board[column, lineToSearch + (playerIndex == 1 ? -1 : 1)]);
                if (yokai != null && yokai.CanEat(kingPos))
                {
                    return yokai.PlayerIndex;
                }
            }
            return playerIndex;
        }
        return 0;
    }

    #endregion

    #region Victory

    public static void Winner(int playerIndex)
    {
        if (playerIndex == 0)
        {
            Debug.Log("No Winner ! You suck !");
        }
        else
        {
            Debug.Log("Player " + playerIndex + " WIN !");
        }
        IsPlaying = false;
    }

    #endregion


    #region Environment

    private void EnablePhysicInteractions(bool enable)
    {
        physicsRaycaster.enabled = enable;
    }
    private void SetBoardLayer()
    {
        physicsRaycaster.eventMask = boardLayer;
    }
    private void SetYokaiLayer()
    {
        physicsRaycaster.eventMask = yokaiLayer;
    }

    private Vector3 _currentCameraRotation;
    private void RotateCamera(Action onComplete)
    {
        _currentCameraRotation = new Vector3(0, 0, _currentCameraRotation.z == 0 ? 180 : 0);
        mainCamera.transform.DORotate(_currentCameraRotation, cameraRotationDuration);

        DOVirtual.DelayedCall(cameraRotationDuration, () => onComplete?.Invoke());
    }

    #endregion

    #region Utility

    private static bool Exist()
    {
        if (Instance != null) return true;

        Debug.LogError("No GameManager found in the scene");
        return false;
    }

    #endregion
}

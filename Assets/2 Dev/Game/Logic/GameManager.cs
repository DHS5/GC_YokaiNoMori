using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
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

    #endregion

    #region Core Behaviour

    private void Init()
    {

    }

    private void Start()
    {
        OnGameStart?.Invoke();

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
        Debug.Log("check for winner");
        if (HasWinner(Board.GetCurrentBoard(), out int winner))
        {
            Debug.Log("Player " + winner + " WIN !");
        }
        else
        {
            Debug.Log("change side");
            ChangeSide();
        }
        Debug.Log("okayy");
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

    public static bool HasWinner(int[,] board, out int winner)
    {
        Vector2Int format = Board.GetFormat();
        int formatX = format.x;
        int formatY = format.y;
        
        // check first and last line for kings
        for (var step = 0; step <= 1; step++)
        {
            var line = step == 0 ? formatY - 1 : 0;
        
            for (var j = 0; j < formatX; j++)
            {
                var yokai = Board.GetYokaiByIndex(board[j, line]);
                if (yokai == null || !yokai.IsKing) continue;
                
                // check around the king if there is a piece of the other player that can reach the king
                var kingTeam = yokai.PlayerIndex;
                for (var i = -1; i <= 1; i++)
                {
                    for (var k = -1; k <= 1; k++)
                    {
                        if (i == 0 && k == 0) continue;
                        if (!Board.IsPositionValid(j + k, line + i)) continue;

                        var piece = Board.GetYokaiByIndex(board[j + k, line + i]);
                        if (piece != null && piece.PlayerIndex != kingTeam && piece.CanEat(yokai.CurrentPosition))
                        {
                            // oops, the king is in danger
                            winner = piece.PlayerIndex;
                            return true;
                        }
                    }
                }
                // The king is all the way to the end and there is no piece that can eat it
                winner = yokai.PlayerIndex;
                return true;
            }            
        }
        // no winner
        winner = 0;
        return false;
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

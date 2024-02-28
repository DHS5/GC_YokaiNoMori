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

    [Header("References")]
    [SerializeField] private Physics2DRaycaster physicsRaycaster;
    [SerializeField] private Camera mainCamera;

    [Header("Parameters")]
    [SerializeField] private float cameraRotationDuration = 2f;

    #endregion

    #region Core Behaviour

    private void Init()
    {

    }

    private void Start()
    {
        OnGameStart?.Invoke();
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

        OnSetTurn?.Invoke(CurrentPlayer);
    }
    

    private void OnPlayerInput(Player.Input input)
    {
        // Deactivate input
        EnablePhysicInteractions(false);

        // Set player to 0
        CurrentPlayer = 0;

        // Effectuate movement
        Board.TryMakeMove(input, ChangeSide);
    }

    private void ChangeSide()
    {
        // Rotate camera
        RotateCamera();

        // On Complete
        ChangeTurn();
    }

    #endregion


    #region Game Rules

    public static bool HasWinner(int[,] board, out int winner)
    {
        winner = 0;
        return false;
    }

    #endregion


    #region Environment

    private void EnablePhysicInteractions(bool enable)
    {
        physicsRaycaster.enabled = enable;
    }

    private Vector3 _currentCameraRotation;
    private void RotateCamera()
    {
        _currentCameraRotation = new Vector3(0, 0, _currentCameraRotation.z == 0 ? 180 : 0);
        mainCamera.transform.DORotate(_currentCameraRotation, cameraRotationDuration);
    }

    #endregion
}

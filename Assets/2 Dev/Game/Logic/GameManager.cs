using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


        // Set player to 0
        CurrentPlayer = 0;

        // Effectuate movement
    }

    private void ChangeSide()
    {
        // Turn camera


        // On Complete
        ChangeTurn();
    }

    #endregion
}

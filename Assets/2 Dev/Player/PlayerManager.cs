using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton

    private static PlayerManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region Global Members

    [Header("Player Manager")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    #endregion

    #region Controllers

    public static void AssignPlayerControllers(Controller c1, Controller c2)
    {
        if (Instance != null)
        {
            Instance.player1.AssignController(c1);
            Instance.player2.AssignController(c2);
        }
    }

    #endregion

    #region Player

    public static Player CurrentPlayer
    {
        get
        {
            if (Instance == null)
            {
                Debug.LogError("No PlayerManager found in the scene");
                return null;
            }
            int currentPlayer = GameManager.CurrentPlayer;
            if (currentPlayer == 0)
            {
                Debug.LogWarning("Current Player is 0");
                return null;
            }
            return currentPlayer == 1 ? Instance.player1 : Instance.player2;
        }
    }

    public static Player GetPlayerByIndex(int playerIndex)
    {
        if (Instance == null)
        {
            Debug.LogError("No PlayerManager found in the scene");
            return null;
        }

        return playerIndex == 1 ? Instance.player1 : Instance.player2;
    }

    #endregion
}

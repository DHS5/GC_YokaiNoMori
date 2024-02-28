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
}

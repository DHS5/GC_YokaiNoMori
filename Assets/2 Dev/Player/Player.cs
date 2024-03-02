using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Global Members

    [Header("Player")]
    [SerializeField] private int index;

    public int Index => index;

    #endregion

    #region Accessors

    public bool IsPlaying => GameManager.CurrentPlayer == Index;

    #endregion

    #region Controller

    private Controller _controller;

    public bool IsHuman => _controller.IsHuman;

    public void AssignController(Controller controller)
    {
        _controller = controller;
        _controller.AssignPlayer(this);
    }

    #endregion
}

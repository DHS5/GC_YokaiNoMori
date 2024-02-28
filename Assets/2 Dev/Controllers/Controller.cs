using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    #region Global Members

    protected Player _player;

    #endregion

    #region Core Behaviour

    private void OnEnable()
    {
        GameManager.OnSetTurn += CheckForTurn;
    }
    private void OnDisable()
    {
        GameManager.OnSetTurn -= CheckForTurn;
    }

    private void CheckForTurn(int playerIndex)
    {
        if (_player.Index == playerIndex)
        {
            PrepareInput();
        }
    }

    #endregion

    #region Input

    public abstract void PrepareInput();

    protected void SendInput(Player.Input input)
    {
        // TODO
    }

    #endregion
}

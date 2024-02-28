using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    #region Player

    [SerializeField] protected Player player;

    public void AssignPlayer(Player _player)
    {
        player = _player;

        OnAssignPlayer();
    }

    protected abstract void OnAssignPlayer();

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
        if (player.Index == playerIndex)
        {
            PrepareInput();
        }
    }

    #endregion

    #region Input

    public abstract void PrepareInput();

    protected void SendInput(Player.Input input)
    {
        GameManager.PlayerInput(input);
    }

    #endregion

    #region Human / AI

    public abstract bool IsHuman { get; }

    #endregion
}

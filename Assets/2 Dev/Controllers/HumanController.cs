using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : Controller
{
    #region Core Behaviour



    #endregion

    #region Registration

    private static HumanController _humanPlayer1 = null;
    private static HumanController _humanPlayer2 = null;

    private static void Register(HumanController humanController)
    {
        switch (humanController._player.Index)
        {
            case 1: _humanPlayer1 = humanController; return;
            case 2: _humanPlayer2 = humanController; return;
        }
    }

    public static void Clear()
    {
        _humanPlayer1 = null;
        _humanPlayer2 = null;
    }

    #endregion

    #region Static Functions

    public static void HumanYokaiInput(Yokai yokai, Action onInputValid)
    {
        if (yokai.PlayerIndex == 1 && _humanPlayer1 != null && _humanPlayer1._isWaitingForInput)
        {
            _humanPlayer1._input.yokai = yokai;
            onInputValid?.Invoke();
            _humanPlayer1._hasYokai = true;
        }
        else if (yokai.PlayerIndex == 2 && _humanPlayer2 != null && _humanPlayer2._isWaitingForInput)
        {
            _humanPlayer2._input.yokai = yokai;
            onInputValid?.Invoke();
            _humanPlayer2._hasYokai = true;
        }
    }
    public static void HumanBoardPieceInput(BoardPiece boardPiece, Action onInputValid)
    {
        if (_humanPlayer1 != null && _humanPlayer1._isWaitingForInput && _humanPlayer1._hasYokai)
        {
            _humanPlayer1._input.newPosition = boardPiece.Position;
            onInputValid?.Invoke();
            _humanPlayer1.SendInput();
        }
        else if (_humanPlayer2 != null && _humanPlayer2._isWaitingForInput && _humanPlayer2._hasYokai)
        {
            _humanPlayer2._input.newPosition = boardPiece.Position;
            onInputValid?.Invoke();
            _humanPlayer2.SendInput();
        }
    }

    #endregion

    #region Input

    private Player.Input _input;

    private bool _isWaitingForInput;
    private bool _hasYokai;

    public override void PrepareInput()
    {
        _isWaitingForInput = true;
        _hasYokai = false;
    }

    private void SendInput()
    {
        _isWaitingForInput = false;
        SendInput(_input);
    }

    #endregion

    #region Player



    #endregion
}

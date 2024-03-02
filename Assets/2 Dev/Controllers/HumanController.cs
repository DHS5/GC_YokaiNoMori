using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : Controller
{
    public override bool IsHuman => true;

    #region Core Behaviour


    #endregion

    #region Registration

    private static HumanController _humanPlayer1 = null;
    private static HumanController _humanPlayer2 = null;

    private static void Register(HumanController humanController)
    {
        switch (humanController.player.Index)
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

    public static void CancelYokaiInput()
    {
        if (_humanPlayer1 != null && _humanPlayer1._isWaitingForInput && _humanPlayer1._hasYokai)
        {
            _humanPlayer1._hasYokai = false;
            _humanPlayer1._input.yokai.Deselect();
            _humanPlayer1._input.yokai = null;
            GameManager.DeselectYokai();
        }
        else if (_humanPlayer2 != null && _humanPlayer2._isWaitingForInput && _humanPlayer2._hasYokai)
        {
            _humanPlayer2._hasYokai = false;
            _humanPlayer2._input.yokai.Deselect();
            _humanPlayer2._input.yokai = null;
            GameManager.DeselectYokai();
        }
    }

    public static void YokaiInput(Yokai yokai, Action onInputValid)
    {
        if (yokai.PlayerIndex == 1 && _humanPlayer1 != null && _humanPlayer1._isWaitingForInput)
        {
            _humanPlayer1._input.yokai = yokai;
            onInputValid?.Invoke();
            _humanPlayer1._hasYokai = true;
            GameManager.SelectYokai();
        }
        else if (yokai.PlayerIndex == 2 && _humanPlayer2 != null && _humanPlayer2._isWaitingForInput)
        {
            _humanPlayer2._input.yokai = yokai;
            onInputValid?.Invoke();
            _humanPlayer2._hasYokai = true;
            GameManager.SelectYokai();
        }
    }
    public static void BoardPieceInput(BoardPiece boardPiece, Action onInputValid)
    {
        if (_humanPlayer1 != null && _humanPlayer1._isWaitingForInput && _humanPlayer1._hasYokai)
        {
            _humanPlayer1._input.newPosition = boardPiece.Position;
            _humanPlayer1._input.yokai.Deselect();
            onInputValid?.Invoke();
            _humanPlayer1.SendInput();
        }
        else if (_humanPlayer2 != null && _humanPlayer2._isWaitingForInput && _humanPlayer2._hasYokai)
        {
            _humanPlayer2._input.newPosition = boardPiece.Position;
            _humanPlayer2._input.yokai.Deselect();
            onInputValid?.Invoke();
            _humanPlayer2.SendInput();
        }
    }

    #endregion

    #region Input

    private Move _input = new();

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

    protected override void OnAssignPlayer()
    {
        Register(this);
    }

    #endregion
}

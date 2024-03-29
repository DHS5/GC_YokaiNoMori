using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

public class Player : MonoBehaviour, ICompetitor
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

    #region ICompetitor

    public void Init(IGameManager igameManager, float timerForAI, ECampType currentCamp)
    {

    }

    /// <summary>
    /// Used before StartTurn() for getting data from the board
    /// </summary>
    public void GetDatas()
    {

    }

    /// <summary>
    /// Used by my UI
    /// </summary>
    /// <returns>Returns the name of this competitor's creator group</returns>
    public string GetName() => "Thomas DHAUSSY and Mathieu PONAL";

    /// <summary>
    /// Recovering the competitor's camp
    /// </summary>
    /// <returns></returns>
    public ECampType GetCamp() => (ECampType)Index;

    /// <summary>
    /// Allows my tournament manager to change the camp at the start of the game
    /// </summary>
    /// <param name="camp"></param>
    public void SetCamp(ECampType camp) => index = (int)camp;


    /// <summary>
    /// Called by the Game Manager to warn the competitor that it's his turn.
    /// </summary>
    public void StartTurn()
    {
        _controller.PrepareInput();
    }

    /// <summary>
    /// Called by the Game Manager to warn the competitor that his turn is over.
    /// </summary>
    public void StopTurn()
    {

    }

    #endregion
}

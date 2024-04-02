using Group15;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : Controller
{
    public static event Action OnAITurn;

    public override void PrepareInput()
    {
        OnAITurn?.Invoke();

        StartCoroutine(ComputeCR());
    }

    private IEnumerator ComputeCR()
    {
        yield return null;
        AI.ComputeMove();
    }

    #region Player

    private AICore AI { get; set; }

    public static AILevel Level;

    protected override void OnAssignPlayer()
    {
        AI = new(player.Index, Level, GameManager.Instance);
    }

    #endregion

    public override bool IsHuman => false;
}

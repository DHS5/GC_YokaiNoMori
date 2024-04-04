using Group15;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

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

    [SerializeField] private AIMovesImporter j1MovesImporter;
    [SerializeField] private AIMovesImporter j2MovesImporter;

    protected override void OnAssignPlayer()
    {
        if (Level == AILevel.INVINCIBLE)
            AI = new((ECampType)player.Index, GameManager.Instance, player.Index == 1 ? j1MovesImporter : j2MovesImporter);
        else
            AI = new((ECampType)player.Index, Level, GameManager.Instance);
    }

    #endregion

    public override bool IsHuman => false;
}

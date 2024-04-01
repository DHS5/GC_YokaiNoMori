using Group15;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : Controller
{
    public override void PrepareInput()
    {
        Vector2Int format = Board.Format;
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

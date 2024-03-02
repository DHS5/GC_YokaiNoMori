using Group15;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : Controller
{
    public override void PrepareInput()
    {
        Vector2Int format = Board.GetFormat();
        SendInput(AI.ComputeMove(Board.GetCurrentBoard(), format.x, format.y));
    }

    #region Player

    private AICore AI { get; set; }

    protected override void OnAssignPlayer()
    {
        AI = new(player.Index, AILevel.RANDOM);
    }

    #endregion

    public override bool IsHuman => false;
}

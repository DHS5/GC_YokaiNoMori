using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    #region Constructor

    public Move()
    {
        yokai = null;
        newPosition = Vector2Int.zero;
    }
    public Move(Yokai _yokai, Vector2Int _newPosition)
    {
        yokai = _yokai;
        newPosition = _newPosition;
    }

    #endregion

    #region Public Members

    public Yokai yokai;
    public Vector2Int newPosition;

    #endregion
}

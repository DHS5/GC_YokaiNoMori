using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BP_", menuName = "BoardPiece Data")]
public class BoardPieceData : ScriptableObject
{
    [Header("Sprites")]
    [SerializeField] private Color normalColor;
    [SerializeField] private Color validColor;
    [SerializeField] private Color unvalidColor;
    [SerializeField] private Color hoveredColor;

    public Color GetColor(BoardPiece.State state)
    {
        switch (state)
        {
            case BoardPiece.State.NORMAL:
                return normalColor;
            case BoardPiece.State.VALID:
                 return validColor;
            case BoardPiece.State.UNVALID:
                return normalColor;
        }
        return Color.white;
    }
    public Color HoveredColor => hoveredColor;
}

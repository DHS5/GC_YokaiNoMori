using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BP_", menuName = "BoardPiece Data")]
public class BoardPieceData : ScriptableObject
{
    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite validSprite;
    [SerializeField] private Sprite unvalidSprite;
    [SerializeField] private Sprite hoveredSprite;

    public Sprite GetSprite(BoardPiece.State state)
    {
        switch (state)
        {
            case BoardPiece.State.NORMAL:
                return normalSprite;
            case BoardPiece.State.VALID:
                 return validSprite;
            case BoardPiece.State.UNVALID:
                return unvalidSprite;
            case BoardPiece.State.HOVERED:
                return hoveredSprite;
        }
        return null;
    }
}

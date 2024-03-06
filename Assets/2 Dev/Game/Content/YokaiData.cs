using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

[CreateAssetMenu(fileName = "YK_", menuName = "Yokai Data")]
public class YokaiData : ScriptableObject
{
    [Header("Yokai Data")]
    [SerializeField] private EPawnType pawnType;
    [SerializeField] private bool isKing;
    [SerializeField] private int index;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite sprite;

    [Space(10f)]

    [SerializeField] private YokaiMovementGrid baseMovementGrid;

    [Space(15f)]

    [SerializeField] private bool hasSecondFace;
    [SerializeField] private Sprite secondSprite;
    [Space(5f)]
    [SerializeField] private YokaiMovementGrid secondMovementGrid;


    public EPawnType PawnType => pawnType;
    public bool IsKing => isKing;
    public int Index => index;
    public string DisplayName => displayName;

    public bool HasSecondFace => hasSecondFace;

    public Sprite GetSprite(bool secondFace)
    {
        if (!secondFace) return sprite;
        return secondSprite;
    }

    public List<Vector2Int> GetValidDeltas(bool secondFace)
    {
        if (HasSecondFace && secondFace) return secondMovementGrid.ValidDeltas;
        return baseMovementGrid.ValidDeltas;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        baseMovementGrid.ComputeValidDeltas();
        secondMovementGrid.ComputeValidDeltas();
    }
#endif
}

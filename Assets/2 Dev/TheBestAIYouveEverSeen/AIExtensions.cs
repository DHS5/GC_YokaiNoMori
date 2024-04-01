using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

namespace Group15
{
    public static class AIExtensions
    {
        #region Piece

        public static List<Vector2Int> GetDirections(this Piece piece)
        {
            switch (piece)
            {
                default: return null;
                case Piece.LION1:
                case Piece.LION2:
                    return new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.one, 
                        new Vector2Int(-1,1), new Vector2Int(-1,-1), new Vector2Int(1,-1) };
                case Piece.GIRAFFE1:
                case Piece.GIRAFFE2:
                    return new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                case Piece.ELEPHANT1:
                case Piece.ELEPHANT2:
                    return new List<Vector2Int>() { Vector2Int.one, new Vector2Int(-1,1), new Vector2Int(-1,-1), new Vector2Int(1,-1) };
                case Piece.CHICK1:
                case Piece.CHICK2:
                    return new List<Vector2Int>() { Vector2Int.up };
                case Piece.HEN1:
                case Piece.HEN2:
                    return new List<Vector2Int>() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.one, new Vector2Int(-1,1) };
            }
        }

        public static ECampType GetCamp(this Piece piece)
        {
            switch (piece)
            {
                case Piece.LION1:
                case Piece.GIRAFFE1:
                case Piece.ELEPHANT1:
                case Piece.CHICK1:
                case Piece.HEN1:
                    return ECampType.PLAYER_ONE;
                case Piece.LION2:
                case Piece.GIRAFFE2:
                case Piece.ELEPHANT2:
                case Piece.CHICK2:
                case Piece.HEN2:
                    return ECampType.PLAYER_TWO;
                default:
                    throw new NotImplementedException();
            }
        }

        public static Piece GetOpposite(this Piece piece)
        {
            switch (piece)
            {
                case Piece.LION1:
                    return Piece.LION2;
                case Piece.GIRAFFE1:
                    return Piece.GIRAFFE2;
                case Piece.ELEPHANT1:
                    return Piece.ELEPHANT2;
                case Piece.CHICK1:
                case Piece.HEN1:
                    return Piece.CHICK2;
                case Piece.LION2:
                    return Piece.LION1;
                case Piece.GIRAFFE2:
                    return Piece.GIRAFFE1;
                case Piece.ELEPHANT2:
                    return Piece.ELEPHANT1;
                case Piece.CHICK2:
                case Piece.HEN2:
                    return Piece.CHICK1;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool IsKingOfCamp(this Piece piece, ECampType camp)
        {
            return (camp == ECampType.PLAYER_ONE && piece == Piece.LION1) || (camp == ECampType.PLAYER_TWO && piece == Piece.LION2);
        }

        #endregion

        #region Position

        public static Vector2Int ToVector(this Position position)
        {
            return position switch
            {
                Position.X0Y0 => Vector2Int.zero,
                Position.X0Y1 => Vector2Int.up,
                Position.X0Y2 => new Vector2Int(0, 2),
                Position.X0Y3 => new Vector2Int(0, 3),
                Position.X1Y0 => Vector2Int.right,
                Position.X1Y1 => Vector2Int.one,
                Position.X1Y2 => new Vector2Int(1, 2),
                Position.X1Y3 => new Vector2Int(1, 3),
                Position.X2Y0 => new Vector2Int(2, 0),
                Position.X2Y1 => new Vector2Int(2, 1),
                Position.X2Y2 => new Vector2Int(2, 2),
                Position.X2Y3 => new Vector2Int(2, 3),
                _ => new Vector2Int(-1, -1)
            };
        }

        public static bool IsWinningRowForCamp(this Position position, ECampType camp)
        {
            switch (position)
            {
                case Position.X0Y0:
                case Position.X1Y0:
                case Position.X2Y0:
                    return camp == ECampType.PLAYER_TWO;
                case Position.X0Y3:
                case Position.X1Y3:
                case Position.X2Y3:
                    return camp == ECampType.PLAYER_ONE;
                default: return false;
            }
        }

        #endregion

        #region Camp

        public static ECampType OppositeCamp(this ECampType camp)
        {
            return camp == ECampType.PLAYER_ONE ? ECampType.PLAYER_TWO : ECampType.PLAYER_ONE;
        }

        #endregion

        #region Vector

        public static bool IsAround(this Vector2Int position, Vector2Int otherPos)
        {
            return Mathf.Abs(position.x - otherPos.x) < 2 && Mathf.Abs(position.y - otherPos.y) < 2;
        }

        public static bool IsInBoard(this Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < 3 && position.y < 4;
        }

        public static Position ToPosition(this Vector2Int position)
        {
            return GetPositionFromVector(position.x, position.y);
        }

        public static Position GetPositionFromVector(int x, int y)
        {
            switch (x)
            {
                case -1:
                    return Position.Dead;
                case 0:
                    {
                        switch (y)
                        {
                            case 0: return Position.X0Y0;
                            case 1: return Position.X0Y1;
                            case 2: return Position.X0Y2;
                            case 3: return Position.X0Y3;
                        }
                        break;
                    }
                case 1:
                    {
                        switch (y)
                        {
                            case 0: return Position.X1Y0;
                            case 1: return Position.X1Y1;
                            case 2: return Position.X1Y2;
                            case 3: return Position.X1Y3;
                        }
                        break;
                    }
                case 2:
                    {
                        switch (y)
                        {
                            case 0: return Position.X2Y0;
                            case 1: return Position.X2Y1;
                            case 2: return Position.X2Y2;
                            case 3: return Position.X2Y3;
                        }
                        break;
                    }
                default:
                    return Position.Dead;
            }
            return Position.Dead;
        }

        #endregion
    }
}

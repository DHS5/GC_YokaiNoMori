using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace Group15
{
    public enum Piece : byte
    {
        EMPTY = 0,
        LION1 = 1,
        LION2 = 2,
        ELEPHANT1 = 3,
        ELEPHANT2 = 4,
        GIRAFFE1 = 5,
        GIRAFFE2 = 6,
        CHICK1 = 7,
        CHICK2 = 8,
        HEN1 = 9,
        HEN2 = 10,
    }

    public enum Position : byte
    {
        X0Y0 = 0,
        X1Y0 = 1,
        X2Y0 = 2,
        X0Y1 = 3,
        X1Y1 = 4,
        X2Y1 = 5,
        X0Y2 = 6,
        X1Y2 = 7,
        X2Y2 = 8,
        X0Y3 = 9,
        X1Y3 = 10,
        X2Y3 = 11,
        Dead = 12,
    }

    public struct BoardState
    {
        #region Constructors

        public BoardState(List<IPawn> pawns)
        {
            board = 0;
            ComputeBoardFromState(pawns);
        }

        public BoardState(string hexaString)
        {
            board = Convert.ToUInt64(hexaString, 16);
        }

        #endregion

        private UInt64 board;

        public static implicit operator ulong(BoardState boardState)
        {
            return boardState.board;
        }

        #region Computations

        private void ComputeBoardFromState(List<IPawn> pawns)
        {
            List<(Piece, Position)> pieces = GetPiecesAndPosition(pawns);

            byte b1;
            byte b2;
            int shift;
            ulong mask;
            for (int i = 0; i < pieces.Count; i++)
            {
                b1 = (byte)pieces[i].Item1;
                b2 = (byte)pieces[i].Item2;
                byte pieceAndPos = (byte)((b1 << 4) + b2);
                shift = i * 8;
                mask = (ulong)0xff << shift;

                board = (board & ~mask) | ((ulong)pieceAndPos << shift);
            }
        }

        #endregion

        #region Utility

        public override string ToString()
        {
            return string.Format("0x{0:X}", board) + " (" + board.ToString() + ")";
        }

        private List<(Piece, Position)> GetPiecesAndPosition(List<IPawn> pawns)
        {
            List<(Piece, Position)> pieces = new();
            foreach (var pawn in pawns)
            {
                pieces.Add((GetPieceFromPawn(pawn), GetPositionFromVector(pawn.GetCurrentPosition())));
            }
            pieces.Sort((p1, p2) => ((byte)p1.Item1).CompareTo((byte)p2.Item1));
            return pieces;
        }

        private Piece GetPieceFromPawn(IPawn pawn)
        {
            bool p1 = pawn.GetCurrentOwner().GetCamp() == YokaiNoMori.Enumeration.ECampType.PLAYER_ONE;
            switch (pawn.GetPawnType())
            {
                case YokaiNoMori.Enumeration.EPawnType.Kodama:
                    return p1 ? Piece.CHICK1 : Piece.CHICK2;
                case YokaiNoMori.Enumeration.EPawnType.KodamaSamurai:
                    return p1 ? Piece.HEN1 : Piece.HEN2;
                case YokaiNoMori.Enumeration.EPawnType.Kitsune:
                    return p1 ? Piece.ELEPHANT1 : Piece.ELEPHANT2;
                case YokaiNoMori.Enumeration.EPawnType.Tanuki:
                    return p1 ? Piece.GIRAFFE1 : Piece.GIRAFFE2;
                case YokaiNoMori.Enumeration.EPawnType.Koropokkuru:
                    return p1 ? Piece.LION1 : Piece.LION2;
                default:
                    return Piece.EMPTY;
            }
        }

        private Position GetPositionFromVector(Vector2Int position)
        {
            switch (position.x)
            {
                case -1:
                    return Position.Dead;
                case 0:
                    {
                        switch (position.y)
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
                        switch (position.y)
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
                        switch (position.y)
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

        public static Vector2Int GetVectorFromPosition(Position position)
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

        public static EPawnType GetPawnTypeFromPiece(Piece piece)
        {
            switch (piece)
            {
                case Piece.LION1:
                case Piece.LION2:
                    return EPawnType.Koropokkuru;
                case Piece.GIRAFFE1:
                case Piece.GIRAFFE2:
                    return EPawnType.Tanuki;
                case Piece.ELEPHANT1:
                case Piece.ELEPHANT2:
                    return EPawnType.Kitsune;
                case Piece.CHICK1:
                case Piece.CHICK2:
                    return EPawnType.Kodama;
                case Piece.HEN1:
                case Piece.HEN2:
                    return EPawnType.KodamaSamurai;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}

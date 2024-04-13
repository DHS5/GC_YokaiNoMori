using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public BoardState(ulong uloong)
        {
            board = uloong;
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
            List<(Piece, Position)> pieces = GetPiecesAndPositionFromState(pawns);

            byte b1;
            byte b2;
            List<byte> bytes = new();
            for (int i = 0; i < pieces.Count; i++)
            {
                b1 = (byte)pieces[i].Item1;
                b2 = (byte)pieces[i].Item2;
                byte pieceAndPos = (byte)((b1 << 4) + b2);
                bytes.Add(pieceAndPos);
            }

            bytes.Sort((b1, b2) => b1.CompareTo(b2));

            board = BitConverter.ToUInt64(bytes.ToArray());
        }

        public BoardState ComputeChildFromNextMove(NextMove nextMove, ECampType camp)
        {
            (byte current, byte next) = nextMove.GetCurrentAndNext(camp);
            var newPos = next & 0x0f;

            byte[] currentBytes = BitConverter.GetBytes(board);
            byte[] newBytes = new byte[8];

            bool found = false;

            for (int i = 0; i < 8; i++)
            {
                if (!found && currentBytes[i] == current)
                {
                    newBytes[i] = next;
                    found = true;
                }
                else if ((currentBytes[i] & 0x0f) == newPos)
                {
                    byte piece = (byte)((Piece)((currentBytes[i] & 0xf0) >> 4)).GetOppositeExceptKing();
                    newBytes[i] = (byte)((piece << 4) + ((byte)Position.Dead));
                }
                else
                {
                    newBytes[i] = currentBytes[i];
                }
            }

            if (!found)
            {
                Debug.LogError("Found no correspondance between " + this + " and " + nextMove);
                return this;
            }
            List<byte> result = newBytes.ToList();
            result.Sort((b1, b2) => b1.CompareTo(b2));

            return new(BitConverter.ToUInt64(result.ToArray()));
        }

        public BoardState ComputeChildFromNextMoveOnEmptySpace(NextMove nextMove, ECampType camp)
        {
            (byte current, byte next) = nextMove.GetCurrentAndNext(camp);

            byte[] currentBytes = BitConverter.GetBytes(board);
            byte[] newBytes = new byte[8];

            bool found = false;

            for (int i = 0; i < 8; i++)
            {
                if (!found && currentBytes[i] == current)
                {
                    newBytes[i] = next;
                    found = true;
                }
                else
                {
                    newBytes[i] = currentBytes[i];
                }
            }

            if (!found)
            {
                Debug.LogError("Found no correspondance between " + this + " and " + nextMove);
                return this;
            }

            List<byte> result = newBytes.ToList();
            result.Sort((b1, b2) => b1.CompareTo(b2));

            return new(BitConverter.ToUInt64(result.ToArray()));
        }

        #endregion

        #region Win

        public bool HasWinner(ECampType playingCamp, out ECampType winnerCamp)
        {
            winnerCamp = ECampType.NONE;

            List<(Piece, Position)> piecesAndPos = GetPiecesAndPosition();

            ECampType enemyCamp = playingCamp.OppositeCamp();

            bool foundOwnKing = false;
            bool foundEnemyKing = false;

            bool kingOnWinningRow = false;
            Vector2Int kingPos = Vector2Int.zero;

            List<(Piece, Position)> enemies = new();

            foreach (var pap in piecesAndPos)
            {
                if (!foundEnemyKing && pap.Item1.IsKingOfCamp(enemyCamp))
                {
                    foundEnemyKing = true;
                    if (pap.Item2 == Position.Dead)
                    {
                        winnerCamp = playingCamp;
                        return true;
                    }
                    enemies.Add(pap);
                }

                else if (!foundOwnKing && pap.Item1.IsKingOfCamp(playingCamp))
                {
                    foundOwnKing = true;
                    if (pap.Item2.IsWinningRowForCamp(playingCamp))
                    {
                        kingOnWinningRow = true;
                        winnerCamp = playingCamp;
                    }
                    kingPos = pap.Item2.ToVector();
                }
                else if (pap.Item1.GetCamp() == enemyCamp)
                {
                    enemies.Add(pap);
                }
            }

            Vector2Int enemyPos;
            Vector2Int futurEnemyPos;
            foreach (var enemyPap in enemies)
            {
                enemyPos = enemyPap.Item2.ToVector();
                if (enemyPos.IsAround(kingPos))
                {
                    foreach (var dir in enemyPap.Item1.GetDirections())
                    {
                        futurEnemyPos = enemyPos + dir;
                        if (futurEnemyPos == kingPos)
                        {
                            winnerCamp = enemyCamp;
                            return true;
                        }
                    }
                }
            }


            return kingOnWinningRow;
        }

        #endregion

        #region Utility

        public override string ToString()
        {
            return string.Format("0x{0:X}", board) + " (" + board.ToString() + ")";
        }
        public void DebugBoardState()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var piecePos in GetPiecesAndPosition())
            {
                sb.AppendLine(piecePos.Item1 + " at " + piecePos.Item2);
            }
            Debug.Log(sb.ToString());
        }

        public List<(Piece, Position)> GetPiecesAndPosition()
        {
            List<(Piece, Position)> pieces = new();

            byte[] bytes = BitConverter.GetBytes(board);
            for (int i = 0; i < 8; i++)
            {
                pieces.Add(((Piece)((bytes[i] & 0xf0) >> 4), (Position)(bytes[i] & 0x0f)));
            }

            return pieces;
        }

        public static List<(Piece, Position)> GetPiecesAndPositionFromState(List<IPawn> pawns)
        {
            List<(Piece, Position)> pieces = new();
            foreach (var pawn in pawns)
            {
                pieces.Add((GetPieceFromPawn(pawn), pawn.GetCurrentPosition().ToPosition()));
            }
            pieces.Sort((p1, p2) => ((byte)p1.Item1).CompareTo((byte)p2.Item1));
            return pieces;
        }

        public static Piece GetPieceFromPawn(IPawn pawn)
        {
            bool p1 = pawn.GetCurrentOwner().GetCamp() == ECampType.PLAYER_ONE;
            switch (pawn.GetPawnType())
            {
                case EPawnType.Kodama:
                    return p1 ? Piece.CHICK1 : Piece.CHICK2;
                case EPawnType.KodamaSamurai:
                    return p1 ? Piece.HEN1 : Piece.HEN2;
                case EPawnType.Kitsune:
                    return p1 ? Piece.ELEPHANT1 : Piece.ELEPHANT2;
                case EPawnType.Tanuki:
                    return p1 ? Piece.GIRAFFE1 : Piece.GIRAFFE2;
                case EPawnType.Koropokkuru:
                    return p1 ? Piece.LION1 : Piece.LION2;
                default:
                    return Piece.EMPTY;
            }
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

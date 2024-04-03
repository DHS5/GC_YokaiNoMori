using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

namespace Group15
{
    public struct NextMove
    {
        #region Constructor

        public NextMove(string hexaStr)
        {
            move = Convert.ToUInt16(hexaStr, 16);
        }
        public NextMove(Piece piece, Position oldPos, Position newPos)
        {
            move = 0;
            SaveMove(piece, oldPos, newPos);
        }

        #endregion

        UInt16 move;

        public static implicit operator (Piece, Position, Position)(NextMove nextMove)
        {
            return nextMove.ComputeMove();
        }

        public static implicit operator NextMove(ushort shorty)
        {
            return new NextMove() { move = shorty };
        }
        public static implicit operator ushort(NextMove nextMove)
        {
            return nextMove.move;
        }

        //public (byte, byte) GetCurrentAndNext(ECampType camp)
        //{
        //    Position newPosition = (Position)(move & 0x00f);
        //    Position oldPosition = (Position)((move & 0x0f0) >> 4);
        //    Piece piece = (Piece)((move & 0xf00) >> 4);
        //
        //    byte b_piece = (byte)piece.TransformFromMovement(camp, oldPosition, newPosition);
        //
        //    return ((byte)(b_piece + (byte)oldPosition), (byte)(b_piece + (byte)newPosition));
        //}
        public (byte, byte) GetCurrentAndNext(ECampType camp)
        {
            Position newPosition = (Position)(move & 0x00f);
            Position oldPosition = (Position)((move & 0x0f0) >> 4);
            Piece piece = (Piece)((move & 0xf00) >> 8);

            byte old_piece = (byte)((byte)piece << 4);
            byte new_piece = (byte)((byte)piece.TransformFromMovement(camp, oldPosition, newPosition) << 4);

            return ((byte)(old_piece + (byte)oldPosition), (byte)(new_piece + (byte)newPosition));
        }

        #region Computation

        private (Piece, Position, Position) ComputeMove()
        {
            Position newPosition = (Position)(move & 0x00f);
            Position oldPosition = (Position)((move & 0x0f0) >> 4);
            Piece piece = (Piece)((move & 0xf00) >> 8);

            return (piece, oldPosition, newPosition);
        }

        private void SaveMove(Piece piece, Position oldPos, Position newPos)
        {
            int i_piece = (byte)piece << 8;
            int i_oldPos = (byte)oldPos << 4;
            int i_newPos = (ushort)newPos;
            move = (ushort)(i_piece + i_oldPos + i_newPos);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("0x{0:X}", move) + " (" + move.ToString() + ")";
        }
    }
}

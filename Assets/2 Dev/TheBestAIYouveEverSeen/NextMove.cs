using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Group15
{
    public struct NextMove
    {
        #region Constructor

        public NextMove(string intStr)
        {
            move = Convert.ToByte(intStr);
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

        #region Computation

        private (Piece, Position, Position) ComputeMove()
        {
            Position newPosition = (Position)(move & 0x00f);
            Position oldPosition = (Position)((move & 0x0f0) >> 4);
            Piece piece = (Piece)((move & 0xf00) >> 8);

            return (piece, oldPosition, newPosition);
        }

        #endregion
    }
}

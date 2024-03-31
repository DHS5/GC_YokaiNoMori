using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace Group15
{
    public class MoveTree
    {
        private struct Result
        {
            public Result(int winner, AICore.AIMove move)
            {
                winnerCamp = (ECampType)winner;
                bestMove = move;
                moves = null;
            }

            public ECampType winnerCamp;
            public AICore.AIMove bestMove;

            public Dictionary<AICore.AIMove, MoveTree> moves;
        }

        private IPawn[,] board = new IPawn[3,4];

        private MoveTree(List<IPawn> state, bool player1, int depth)
        {
            ComputeBoard(state);
            ComputePotentialMoves(player1, depth);
        }

        #region Board

        private void ComputeBoard(List<IPawn> state)
        {
            Vector2Int currentPos;
            foreach (IPawn pawn in state)
            {
                currentPos = pawn.GetCurrentPosition();
                if (currentPos.x == -1) continue;

                board[currentPos.x, currentPos.y] = pawn;
            }
        }

        #endregion

        #region Computations

        public List<AICore.AIMove> PotentialMoves { get; private set; }
        private Result result;

        private void ComputePotentialMoves(bool player1, int depth)
        {
            result = new Result() { moves = new() };

            // TODO
        }

        #endregion

        #region Accessors

        public static AICore.AIMove GetNextMove(List<IPawn> state, bool player1, int depth)
        {
            MoveTree moveTree = new MoveTree(state, player1, depth);

            return moveTree.result.bestMove;
        }

        #endregion
    }
}

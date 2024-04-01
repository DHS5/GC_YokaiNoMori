using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace Group15
{
    public class MoveTree
    {
        private struct Result
        {
            public Result(ECampType winner, NextMove move)
            {
                winnerCamp = winner;
                bestMove = move;
                moves = null;
            }

            public ECampType winnerCamp;
            public NextMove bestMove;

            public Dictionary<NextMove, MoveTree> moves;
        }

        public Piece[,] Board { get; private set; } = new Piece[3,4];
        public BoardState State { get; private set; }
        public List<(Piece, Position)> ProcessedState { get; private set; }
        public List<Position> EmptyPositions { get; private set; }

        #region Constructor

        private MoveTree(BoardState state, ECampType campType, int depth)
        {
            State = state;

            if (depth == 0)
            {
                result = new Result(ECampType.NONE, 0);
                return;
            }

            ComputeBoard(state);
            ComputePotentialMoves(campType, depth);
        }

        #endregion

        #region Board

        private void ComputeBoard(BoardState state)
        {
            ProcessedState = state.GetPiecesAndPosition();
            EmptyPositions = new();

            byte pos;
            foreach (var piecePos in ProcessedState)
            {
                if (piecePos.Item2 == Position.Dead) continue;

                pos = (byte)piecePos.Item2;
                Board[pos % 3, pos / 3] = piecePos.Item1;
            }

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (Board[x,y] == Piece.EMPTY)
                    {
                        EmptyPositions.Add(AIExtensions.GetPositionFromVector(x, y));
                    }
                }
            }
        }

        #endregion

        #region Computations

        public List<(NextMove, BoardState)> PotentialMoves { get; private set; }
        private Result result;

        private void ComputePotentialMoves(ECampType camp, int depth)
        {
            PotentialMoves = new();
            result = new Result() { moves = new() };

            NextMove nextMove;
            BoardState nextBoard;
            Vector2Int currentPos;
            Vector2Int newPos;
            Piece pieceAtNewPos;
            foreach (var piecePos in ProcessedState)
            {
                // Is mine
                if (piecePos.Item1.GetCamp() == camp)
                {
                    // If is dead -> parachute
                    if (piecePos.Item2 == Position.Dead)
                    {
                        foreach (var emptyPos in EmptyPositions)
                        {
                            nextMove = new(piecePos.Item1, piecePos.Item2, emptyPos);
                            PotentialMoves.Add((nextMove, State.ComputeChildFromNextMoveOnEmptySpace(nextMove)));
                        }
                    }
                    // Else
                    else
                    {
                        currentPos = piecePos.Item2.ToVector();
                        // Browse on possible moves
                        foreach (var dir in piecePos.Item1.GetDirections())
                        {
                            newPos = currentPos + dir;
                            // Check if the new position is in the board's bounds
                            if (newPos.IsInBoard())
                            {
                                pieceAtNewPos = Board[newPos.x, newPos.y];
                                // If no piece on new pos
                                if (pieceAtNewPos == Piece.EMPTY)
                                {
                                    nextMove = new(piecePos.Item1, piecePos.Item2, newPos.ToPosition());
                                    nextBoard = State.ComputeChildFromNextMoveOnEmptySpace(nextMove);

                                    if (nextBoard.HasWinner(camp, out ECampType winnerCamp))
                                    {
                                        // If there is a winner, check if it is us, if not don't add to potential moves
                                        if (winnerCamp == camp)
                                        {
                                            result = new Result(camp, nextMove);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        PotentialMoves.Add((nextMove, nextBoard));
                                    }
                                }
                                // If enemy piece on new pos
                                else if (pieceAtNewPos.GetCamp() != camp)
                                {
                                    nextMove = new(piecePos.Item1, piecePos.Item2, newPos.ToPosition());
                                    nextBoard = State.ComputeChildFromNextMove(nextMove);

                                    if (nextBoard.HasWinner(camp, out ECampType winnerCamp))
                                    {
                                        // If there is a winner, check if it is us, if not don't add to potential moves
                                        if (winnerCamp == camp)
                                        {
                                            result = new Result(camp, nextMove);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        PotentialMoves.Add((nextMove, nextBoard));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var move in PotentialMoves)
            {
                result.moves.TryAdd(move.Item1, new MoveTree(move.Item2, camp.OppositeCamp(), depth - 1));

                // get proba and next move here
            }
        }

        #endregion

        #region Accessors

        public static AICore.AIMove GetBestMove(List<IPawn> state, ECampType campType, int depth)
        {
            MoveTree moveTree = new MoveTree(new BoardState(state), campType, depth);

            //return moveTree.result.bestMove;
        }

        #endregion
    }
}

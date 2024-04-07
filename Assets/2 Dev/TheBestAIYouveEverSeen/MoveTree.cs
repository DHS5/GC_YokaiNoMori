using System;
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
                hasBestMove = true;
                moves = null;
                wins = Vector2Int.zero;
            }
            public Result(bool _hasBestMove)
            {
                winnerCamp = ECampType.NONE;
                hasBestMove = _hasBestMove;
                bestMove = default;
                moves = new();
                wins = Vector2Int.zero;
            }

            public ECampType winnerCamp;
            public NextMove bestMove;
            public bool hasBestMove;

            public Dictionary<NextMove, MoveTree> moves;

            public Vector2Int wins;

            public Vector2Int GetWins(ECampType playerCamp)
            {
                if (hasBestMove || moves == null)
                {
                    wins = winnerCamp == ECampType.NONE ? Vector2Int.zero : (winnerCamp == playerCamp ? Vector2Int.right : Vector2Int.up);
                    return wins;
                }

                foreach (var move in moves)
                {
                    wins += move.Value.result.wins;
                }

                return wins;
            }
            public Vector2Int ComputeBestMove(ECampType playerCamp)
            {
                if (hasBestMove || moves == null)
                {
                    wins = winnerCamp == ECampType.NONE ? Vector2Int.zero : (winnerCamp == playerCamp ? Vector2Int.right : Vector2Int.up);
                    return wins;
                }

                wins = new Vector2Int(int.MinValue, int.MaxValue);
                foreach (var move in moves)
                {
                    //if (Comparer(result, wins) <= 0)
                    if (move.Value.result.winnerCamp == playerCamp.OppositeCamp()) continue;
                    if (move.Value.result.winnerCamp == playerCamp)
                    {
                        hasBestMove = true;
                        bestMove = move.Key;
                        return Vector2Int.right;
                    }
                    if (bestMoveComparison.Invoke(move.Value.result.wins, wins) <= 0)
                    {
                        wins = move.Value.result.wins;
                        bestMove = move.Key;
                        hasBestMove = true;
                    }
                }

                if (!hasBestMove)
                {
                    winnerCamp = playerCamp.OppositeCamp();
                    return Vector2Int.up;
                }

                return wins;
            }
        }

        #region Global Members

        public int Depth { get; private set; }
        public bool Origin { get; private set; }
        public bool IsPlayer { get; private set; }
        public ECampType Camp { get; private set; }
        public Piece[,] Board { get; private set; }
        public BoardState State { get; private set; }
        public List<(Piece, Position)> ProcessedState { get; private set; }
        public List<Position> EmptyPositions { get; private set; }
        public List<(NextMove, BoardState)> PotentialMoves { get; private set; }

        private Result result;

        #endregion

        #region Constructor

        private MoveTree(BoardState state, ECampType camp, bool isPlayer, int depth)
        {
            State = state;
            Depth = depth;
            Camp = camp;
            IsPlayer = isPlayer;

            if (depth == 0)
            {
                result = new Result(true);
            }            
        }

        #endregion

        #region Depth Computation

        private bool hasComputedDepth = false;
        private void ComputeDepth(int depth, bool origin)
        {
            if (!hasComputedDepth && Depth == depth)
            {
                hasComputedDepth = true;
                Origin = origin;

                ComputeBoard();
                ComputePotentialMoves();
                visited[Camp].TryAdd(State, this);
                return;
            }

            if (result.moves != null)
            {
                foreach (var move in result.moves)
                {
                    if (move.Value.Depth < Depth)
                        move.Value.ComputeDepth(depth, false);
                }
                return;
            }
        }

        #endregion

        #region Best Move Computation

        private bool hasComputedBestMove = false;
        //public Vector2Int ComputeBestMove()
        //{
        //    if (hasComputedBestMove) return Vector2Int.zero;
        //
        //    hasComputedBestMove = true;
        //
        //    if (IsPlayer)
        //    {
        //        return result.ComputeBestMove(Camp);
        //    }
        //    return result.GetWins(Camp.OppositeCamp());
        //}
        private static Comparison<Vector2Int> bestMoveComparison;
        public void ComputeBestMove()
        {
            result.ComputeBestMove(Camp);
        }
        //public Vector2Int GetWins()
        //{
        //    if (hasComputedBestMove) return Vector2Int.zero;
        //
        //    hasComputedBestMove = true;
        //
        //    return result.GetWins(IsPlayer ? Camp : Camp.OppositeCamp());
        //}
        public void GetWins(int depth)
        {
            if (depth != Depth)
            {
                if (result.moves != null)
                {
                    foreach (var move in result.moves)
                    {
                        if (move.Value.Depth < Depth)
                        {
                            move.Value.GetWins(depth);
                        }
                    }
                    return;
                }
            }
            if (hasComputedBestMove || Origin) return;

            hasComputedBestMove = true;

            if (IsPlayer)
                result.ComputeBestMove(Camp);
            else
                result.GetWins(Camp.OppositeCamp());
        }

        #endregion

        #region Board

        private void ComputeBoard()
        {
            Board = new Piece[3, 4];
            ProcessedState = State.GetPiecesAndPosition();
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

        private void ComputePotentialMoves()
        {
            PotentialMoves = new();
            result = new Result(false);

            NextMove nextMove;
            BoardState nextBoard;
            Vector2Int currentPos;
            Vector2Int newPos;
            Piece pieceAtNewPos;
            List<Piece> parachutedPawns = new();
            foreach (var piecePos in ProcessedState)
            {
                // Is mine
                if (piecePos.Item1.GetCamp() == Camp)
                {
                    // If is dead -> parachute
                    if (piecePos.Item2 == Position.Dead && !parachutedPawns.Contains(piecePos.Item1))
                    {
                        parachutedPawns.Add(piecePos.Item1);
                        foreach (var emptyPos in EmptyPositions)
                        {
                            nextMove = new(piecePos.Item1, piecePos.Item2, emptyPos);
                            nextBoard = State.ComputeChildFromNextMoveOnEmptySpace(nextMove, Camp);

                            if (visited[Camp.OppositeCamp()].TryGetValue(nextBoard, out MoveTree tree))
                            {
                                if (tree.result.hasBestMove)
                                {
                                    result = new Result(tree.result.winnerCamp, nextMove);
                                    return;
                                }
                                else
                                {
                                    PotentialMoves.Add((nextMove, nextBoard));
                                }
                            }
                            else if (nextBoard.HasWinner(Camp, out ECampType winnerCamp))
                            {
                                // If there is a winner, check if it is us, if not don't add to potential moves
                                if (winnerCamp == Camp)
                                {
                                    result = new Result(Camp, nextMove);
                                    return;
                                }
                            }
                            else
                            {
                                PotentialMoves.Add((nextMove, nextBoard));
                            }
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
                                    nextBoard = State.ComputeChildFromNextMoveOnEmptySpace(nextMove, Camp);

                                    if (visited[Camp.OppositeCamp()].TryGetValue(nextBoard, out MoveTree tree))
                                    {
                                        if (tree.result.hasBestMove)
                                        {
                                            result = new Result(tree.result.winnerCamp, nextMove);
                                            return;
                                        }
                                        else
                                        {
                                            PotentialMoves.Add((nextMove, nextBoard));
                                        }
                                    }
                                    else if (nextBoard.HasWinner(Camp, out ECampType winnerCamp))
                                    {
                                        // If there is a winner, check if it is us, if not don't add to potential moves
                                        if (winnerCamp == Camp)
                                        {
                                            result = new Result(Camp, nextMove);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        PotentialMoves.Add((nextMove, nextBoard));
                                    }
                                }
                                // If enemy piece on new pos
                                else if (pieceAtNewPos.GetCamp() != Camp)
                                {
                                    nextMove = new(piecePos.Item1, piecePos.Item2, newPos.ToPosition());
                                    nextBoard = State.ComputeChildFromNextMove(nextMove, Camp);

                                    if (visited[Camp.OppositeCamp()].TryGetValue(nextBoard, out MoveTree tree))
                                    {
                                        if (tree.result.hasBestMove)
                                        {
                                            result = new Result(tree.result.winnerCamp, nextMove);
                                            return;
                                        }
                                        else
                                        {
                                            PotentialMoves.Add((nextMove, nextBoard));
                                        }
                                    }
                                    else if(nextBoard.HasWinner(Camp, out ECampType winnerCamp))
                                    {
                                        // If there is a winner, check if it is us, if not don't add to potential moves
                                        if (winnerCamp == Camp)
                                        {
                                            result = new Result(Camp, nextMove);
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

            if (PotentialMoves.Count == 0)
            {
                if (Origin) Debug.Log(Camp + " couldn't find moves in origin depth");
                result = new Result(Camp.OppositeCamp(), 0);
                return;
            }
            else if (PotentialMoves.Count == 1)
            {
                result = new Result(Camp, PotentialMoves[0].Item1);
                return;
            }

            foreach (var move in PotentialMoves)
            {
                if (visited[Camp.OppositeCamp()].TryGetValue(move.Item2, out MoveTree tree))
                {
                    result.moves.TryAdd(move.Item1, tree);
                }
                else
                {
                    result.moves.TryAdd(move.Item1, new MoveTree(move.Item2, Camp.OppositeCamp(), !IsPlayer, Depth - 1));
                }
            }
        }

        #endregion

        #region Static Accessor

        public enum Strategy { DEFENSE = 0, OFFENSE = 1, PROBA = 2 }

        private static Dictionary<ECampType, Dictionary<BoardState, MoveTree>> visited = new()
            {
                { ECampType.PLAYER_ONE, new() }, { ECampType.PLAYER_TWO, new() }
            };

        public static NextMove GetBestMove(List<IPawn> state, ECampType camp, int depth, Strategy strategy)
        {
            MoveTree moveTree = new MoveTree(new BoardState(state), camp, true, depth);

            visited[ECampType.PLAYER_ONE].Clear();
            visited[ECampType.PLAYER_TWO].Clear();

            moveTree.ComputeDepth(depth, true);

            if (moveTree.result.hasBestMove)
            {
                Debug.Log("Has perfect move for winner " + moveTree.result.winnerCamp);
                return moveTree.result.bestMove;
            }

            for (int i = depth - 1; i > 0; i--)
            {
                moveTree.ComputeDepth(i, false);
            }

            switch (strategy)
            {
                case Strategy.DEFENSE:
                    bestMoveComparison = DefensiveComparer; break;
                case Strategy.OFFENSE:
                    bestMoveComparison = OffensiveComparer; break;
                case Strategy.PROBA:
                    bestMoveComparison = ProbaComparer; break;
            }
            for (int i = 0; i < depth; i++)
            {
                moveTree.GetWins(i);
            }

            moveTree.ComputeBestMove();


            return moveTree.result.bestMove;


            int DefensiveComparer(Vector2Int v1, Vector2Int v2)
            {
                if (v1.y != v2.y) return v1.y.CompareTo(v2.y);

                return -v1.x.CompareTo(v2.x);
            }
            int OffensiveComparer(Vector2Int v1, Vector2Int v2)
            {
                if (v1.x != v2.x) return -v1.x.CompareTo(v2.x);

                return v1.y.CompareTo(v2.y);
            }
            int ProbaComparer(Vector2Int v1, Vector2Int v2)
            {
                if (v1.y == 0 && v2.y == 0) return -v1.x.CompareTo(v2.x);
                if (v1.y == 0) return -1;
                if (v2.y == 0) return 1;
                return ((float)v1.x / v1.y).CompareTo((float)v2.x/v2.y);
            }
        }

        public static List<NextMove> GetPotentialMoves(List<IPawn> state, ECampType camp)
        {
            visited[ECampType.PLAYER_ONE].Clear();
            visited[ECampType.PLAYER_TWO].Clear();

            MoveTree moveTree = new MoveTree(new BoardState(state), camp, true, 1);

            moveTree.ComputeBoard();
            moveTree.ComputePotentialMoves();

            if (moveTree.result.hasBestMove)
            {
                if (moveTree.result.winnerCamp == camp)
                    return new List<NextMove> { moveTree.result.bestMove };
                return null;
            }
                

            return moveTree.PotentialMoves.ConvertAll(m => m.Item1);
        }

        #endregion
    }
}

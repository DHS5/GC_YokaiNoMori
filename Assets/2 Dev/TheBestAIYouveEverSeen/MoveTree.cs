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
                hasBestMove = true;
                moves = null;
                wins = Vector2Int.zero;
                hasComputedWins = false;
            }
            public Result(bool _hasBestMove)
            {
                winnerCamp = ECampType.NONE;
                hasBestMove = _hasBestMove;
                bestMove = default;
                moves = new();
                wins = Vector2Int.zero;
                hasComputedWins = false;
            }

            public ECampType winnerCamp;
            public NextMove bestMove;
            public bool hasBestMove;

            public Dictionary<NextMove, MoveTree> moves;

            private Vector2Int wins;
            private bool hasComputedWins;

            public Vector2Int GetWins(ECampType playerCamp)
            {
                if (hasComputedWins) return wins;

                hasComputedWins = true;

                if (hasBestMove || moves == null)
                {
                    wins = winnerCamp == ECampType.NONE ? Vector2Int.zero : (winnerCamp == playerCamp ? Vector2Int.right : Vector2Int.up);
                    return wins;
                }

                foreach (var move in moves)
                {
                    wins += move.Value.result.ComputeBestMove(playerCamp);
                }

                return wins;
            }
            public Vector2Int ComputeBestMove(ECampType playerCamp)
            {
                if (hasComputedWins) return wins;

                hasComputedWins = true;

                if (hasBestMove || moves == null)
                {
                    wins = winnerCamp == ECampType.NONE ? Vector2Int.zero : (winnerCamp == playerCamp ? Vector2Int.right : Vector2Int.up);
                    return wins;
                }

                List<(Vector2Int, NextMove)> choices = new();
                foreach (var move in moves)
                {
                    choices.Add((move.Value.result.GetWins(playerCamp), move.Key));
                }
                choices.Sort((c1,c2) => Comparer(c1.Item1, c2.Item1));

                (wins, bestMove) = choices[0];
                return wins;

                // Local Comparer
                int Comparer(Vector2Int v1, Vector2Int v2)
                {
                    if (v1.y != v2.y) return v1.y.CompareTo(v2.y);

                    return -v1.x.CompareTo(v2.x);
                }
            }
        }

        #region Global Members

        public int Depth { get; private set; }
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

        #region Accessors

        private bool hasComputedDepth = false;
        private void ComputeDepth(Dictionary<ECampType, Dictionary<BoardState, MoveTree>> visited, int depth)
        {
            if (hasComputedDepth) return;
            
            hasComputedDepth = true;

            if (Depth == depth)
            {
                ComputeBoard();
                ComputePotentialMoves(visited, depth);
                visited[Camp].TryAdd(State, this);
            }
            else if (result.moves != null)
            {
                foreach (var move in result.moves)
                {
                    move.Value.ComputeDepth(visited, depth);
                }
            }
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

        private void ComputePotentialMoves(Dictionary<ECampType, Dictionary<BoardState, MoveTree>> visited, int depth)
        {
            PotentialMoves = new();
            result = new Result(false);

            NextMove nextMove;
            BoardState nextBoard;
            Vector2Int currentPos;
            Vector2Int newPos;
            Piece pieceAtNewPos;
            foreach (var piecePos in ProcessedState)
            {
                // Is mine
                if (piecePos.Item1.GetCamp() == Camp)
                {
                    // If is dead -> parachute
                    if (piecePos.Item2 == Position.Dead)
                    {
                        foreach (var emptyPos in EmptyPositions)
                        {
                            nextMove = new(piecePos.Item1, piecePos.Item2, emptyPos);
                            nextBoard = State.ComputeChildFromNextMoveOnEmptySpace(nextMove, Camp);
                            PotentialMoves.Add((nextMove, nextBoard));
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

                                    if (visited[Camp].TryGetValue(nextBoard, out MoveTree tree))
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

                                    if (visited[Camp].TryGetValue(nextBoard, out MoveTree tree))
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
                result = new Result(Camp.OppositeCamp(), 0);
                return;
            }

            foreach (var move in PotentialMoves)
            {
                if (visited[Camp].TryGetValue(move.Item2, out MoveTree tree))
                {
                    result.moves.TryAdd(move.Item1, tree);
                }
                else
                {
                    result.moves.TryAdd(move.Item1, new MoveTree(move.Item2, Camp.OppositeCamp(), !IsPlayer, depth - 1));
                }
            }
        }

        #endregion

        #region Accessors

        public static NextMove GetBestMove(List<IPawn> state, ECampType camp, int depth)
        {
            MoveTree moveTree = new MoveTree(new BoardState(state), camp, true, depth);

            Dictionary<ECampType, Dictionary<BoardState, MoveTree>> visited = new()
            {
                { ECampType.PLAYER_ONE, new() }, { ECampType.PLAYER_TWO, new() }
            };

            moveTree.ComputeDepth(visited, depth);

            if (moveTree.result.hasBestMove)
            {
                return moveTree.result.bestMove;
            }

            for (int i = depth - 1; i > 0; i--)
            {
                moveTree.ComputeDepth(visited, i);
            }

            moveTree.result.ComputeBestMove(camp);

            return moveTree.result.bestMove;
        }

        #endregion
    }
}

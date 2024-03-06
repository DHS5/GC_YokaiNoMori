// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Group15
{
    public enum AILevel
    {
        RANDOM = 0,
        DEBUTANT = 1,
        INTERMEDIATE = 2,
        MASTER = 3,
        INVINCIBLE = 4
    }

    public class AICore
    {
        #region Constructor
        public AICore(int playerIndex, AILevel level)
        {
            FirstPlayer = playerIndex == 1;
            Level = level;
        }
        #endregion

        #region Global Members

        private bool FirstPlayer { get; set; }
        private AILevel Level { get; set; }

        #endregion

        #region Core Behaviour

        public Move ComputeMove(int[,] board, int xSize, int ySize)
        {
            CurrentBoard = board;
            XSize = xSize;
            YSize = ySize;

            AnalyzeYokais();
            ComputeAllPotentialMoves();

            return GetMoveByLevel();
        }

        private Move GetMoveByLevel()
        {
            switch (Level)
            {
                case AILevel.RANDOM: return GetRandomMove();
                default: return null;
            }
        }

        #endregion

        #region Board Infos

        private int XSize { get; set; }
        private int YSize { get; set; }
        private int[,] CurrentBoard { get; set; }

        private List<Yokai> YokaiList { get; set; } = new();
        private List<Yokai> OwnYokais { get; set; } = new();
        private List<Yokai> EnemyYokais { get; set; } = new();

        #endregion

        #region Potential Moves

        private List<Move> PotentialMoves { get; set; } = new();

        private void AnalyzeYokais()
        {
            OwnYokais.Clear();
            EnemyYokais.Clear();

            int yokaiIndex;
            for (int column = 0; column < XSize; column++)
            {
                for (int line = 0; line < YSize; line++)
                {
                    yokaiIndex = CurrentBoard[column, line];
                    if (yokaiIndex != 0)
                    {
                        if (IsYokaiMine(yokaiIndex))
                        {
                            OwnYokais.Add(Board.GetYokaiByIndex(yokaiIndex));
                        }
                        else
                        {
                            EnemyYokais.Add(Board.GetYokaiByIndex(yokaiIndex));
                        }
                    }
                }
            }
        }
        private void ComputeAllPotentialMoves()
        {
            PotentialMoves.Clear();

            Vector2Int newPos;
            foreach (var yokai in OwnYokais)
            {
                foreach (var delta in yokai.ValidDeltas.ConvertAll(d => GetCorrectDelta(d)))
                {
                    newPos = yokai.CurrentPosition + delta;
                    if (CanMakeMove(newPos))
                    {
                        PotentialMoves.Add(new Move(yokai, newPos));
                    }
                }
            }
        }

        #endregion

        #region Random Level

        private Move GetRandomMove()
        {
            return PotentialMoves[UnityEngine.Random.Range(0, PotentialMoves.Count)];
        }

        #endregion


        #region Utility

        private bool IsYokaiMine(int yokaiIndex)
        {
            return FirstPlayer ? yokaiIndex % 2 == 1 : yokaiIndex % 2 == 0;
        }

        private bool IsPositionValid(int x, int y)
        {
            return x >= 0 && x < XSize && y >= 0 && y < YSize;
        }
        private bool IsPositionValid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < XSize && pos.y >= 0 && pos.y < YSize;
        }

        private bool IsOwnYokaiAtThisPosition(Vector2Int pos)
        {
            foreach (var yokai in OwnYokais)
            {
                if (yokai.CurrentPosition == pos) return true;
            }
            return false;
        }

        private bool CanMakeMove(Vector2Int pos)
        {
            return IsPositionValid(pos) && !IsOwnYokaiAtThisPosition(pos);
        }

        private Vector2Int GetCorrectDelta(Vector2Int delta)
        {
            return FirstPlayer ? delta : -delta;
        }

        #endregion
    }
}
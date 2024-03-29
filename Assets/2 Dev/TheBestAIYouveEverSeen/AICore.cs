﻿// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

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
        #region AI Move

        public class AIMove
        {
            #region Constructor

            public AIMove(IPawn _yokai, Vector2Int _newPosition)
            {
                yokai = _yokai;
                newPosition = _newPosition;
                actionType = yokai.GetCurrentBoardCase() != null ? EActionType.MOVE : EActionType.PARACHUTE;
            }

            #endregion

            #region Public Members

            public IPawn yokai;
            public Vector2Int newPosition;
            public EActionType actionType;

            #endregion
        }

        #endregion

        #region Constructor
        public AICore(int playerIndex, AILevel level, IGameManager gameManager)
        {
            FirstPlayer = playerIndex == 1;
            Camp = FirstPlayer ? ECampType.PLAYER_ONE : ECampType.PLAYER_TWO;
            Level = level;
            GameManager = gameManager;
            YokaiList = GameManager.GetAllPawn();
            BoardCases = GameManager.GetAllBoardCase();
        }
        public AICore(int playerIndex, IGameManager gameManager)
        {
            FirstPlayer = playerIndex == 1;
            Camp = FirstPlayer ? ECampType.PLAYER_ONE : ECampType.PLAYER_TWO;
            Level = AILevel.INVINCIBLE;
            GameManager = gameManager;
            YokaiList = GameManager.GetAllPawn();
            BoardCases = GameManager.GetAllBoardCase();
        }
        #endregion

        #region Global Members

        private bool FirstPlayer { get; set; }
        private AILevel Level { get; set; }
        private ECampType Camp { get; set; }
        private IGameManager GameManager { get; set; }

        #endregion

        #region Core Behaviour

        public void ComputeMove(int[,] board, int xSize, int ySize)
        {
            XSize = xSize;
            YSize = ySize;

            AnalyzeYokais();
            GetEmptyBoardCases();
            ComputeAllPotentialMoves();

            AIMove move = GetMoveByLevel();
            GameManager.DoAction(move.yokai, move.newPosition, move.actionType);
        }

        private AIMove GetMoveByLevel()
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

        private List<IBoardCase> BoardCases { get; set; } = new();
        private List<IBoardCase> EmptyBoardCases { get; set; } = new();

        private List<IPawn> YokaiList { get; set; } = new();
        private List<IPawn> OwnYokais { get; set; } = new();
        private List<IPawn> EnemyYokais { get; set; } = new();

        
        private void GetEmptyBoardCases()
        {
            EmptyBoardCases = new();

            foreach (var bCase in BoardCases)
            {
                if (bCase.GetPawnOnIt() == null)
                {
                    EmptyBoardCases.Add(bCase);
                }
            }
        }

        #endregion

        #region Potential Moves

        private List<AIMove> PotentialMoves { get; set; } = new();

        private void AnalyzeYokais()
        {
            OwnYokais.Clear();
            EnemyYokais.Clear();

            foreach (var yokai in YokaiList)
            {
                if (yokai.GetCurrentOwner().GetCamp() == Camp)
                {
                    OwnYokais.Add(yokai);
                }
                else
                {
                    EnemyYokais.Add(yokai);
                }
            }
        }
        private void ComputeAllPotentialMoves()
        {
            PotentialMoves.Clear();

            Vector2Int newPos;
            IBoardCase boardCase;
            foreach (var yokai in OwnYokais)
            {
                boardCase = yokai.GetCurrentBoardCase();

                if (boardCase != null)
                {
                    foreach (var delta in yokai.GetDirections().ConvertAll(d => GetCorrectDelta(d)))
                    {
                        newPos = boardCase.GetPosition() + delta;

                        if (CanMakeMove(newPos))
                        {
                            PotentialMoves.Add(new AIMove(yokai, newPos));
                        }
                    }
                }
                else
                {
                    foreach (var pos in EmptyBoardCases.ConvertAll(c => c.GetPosition()))
                    {
                        PotentialMoves.Add(new AIMove(yokai, pos));
                    }
                }
            }
        }

        #endregion

        #region Random Level

        private AIMove GetRandomMove()
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
            IBoardCase boardCase;
            foreach (var yokai in OwnYokais)
            {
                boardCase = yokai.GetCurrentBoardCase();
                if (boardCase != null && boardCase.GetPosition() == pos) return true;
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
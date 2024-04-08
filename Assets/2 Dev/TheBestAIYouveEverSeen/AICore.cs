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
        DEBUTANT = 0,
        INTERMEDIATE = 1,
        INVINCIBLE = 2
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
            public AIMove(IPawn _yokai, Vector2Int _newPosition, EActionType _actionType)
            {
                yokai = _yokai;
                newPosition = _newPosition;
                actionType = _actionType;
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
        public AICore(ECampType camp, AILevel level, IGameManager gameManager)
        {
            FirstPlayer = camp == ECampType.PLAYER_ONE;
            Camp = camp;
            Level = level;
            GameManager = gameManager;
            YokaiList = GameManager.GetAllPawn();
            BoardCases = GameManager.GetAllBoardCase();
        }
        public AICore(ECampType camp, IGameManager gameManager, AIMovesImporter _movesImporter)
        {
            FirstPlayer = camp == ECampType.PLAYER_ONE;
            Camp = camp;
            Level = AILevel.INVINCIBLE;
            GameManager = gameManager;
            YokaiList = GameManager.GetAllPawn();
            BoardCases = GameManager.GetAllBoardCase();
            movesImporter = _movesImporter;
            movesImporter.ParseNumbers();
        }
        #endregion

        #region Global Members

        private bool FirstPlayer { get; set; }
        private AILevel Level { get; set; }
        private ECampType Camp { get; set; }
        private IGameManager GameManager { get; set; }
        private int Round { get; set; } = 0;

        #endregion

        #region Behaviour

        public void GetDatas()
        {
            AnalyzeYokais();
        }

        public void ComputeMove()
        {
            Round++;

            GetDatas();// TO REMOVE

            AIMove move = GetMoveByLevel();
            GameManager.DoAction(move.yokai, move.newPosition, move.actionType);
        }
        public void ComputeTournamentMove()
        {
            Round++;
            HasDoneAction = false;

            AIMove move = GetMoveByLevel();

            HasDoneAction = move != null;
            GameManager.DoAction(move.yokai, move.newPosition, move.actionType);
        }

        public bool HasDoneAction { get; private set; }

        public void StopTurn()
        {
            if (HasDoneAction) return;

            GetRandomPotentialMove();
        }

        private AIMove GetMoveByLevel()
        {
            switch (Level)
            {
                case AILevel.DEBUTANT: return GetDebutantMove();
                case AILevel.INTERMEDIATE: return GetIntermediateMove();
                case AILevel.INVINCIBLE: return GetInvincibleMove();
                default: return null;
            }
        }

        #endregion



        #region Board Infos

        private int XSize => 3;
        private int YSize => 4;

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
            GetEmptyBoardCases();

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


        #region Random

        private AIMove GetRandomMove()
        {
            ComputeAllPotentialMoves();

            return PotentialMoves[UnityEngine.Random.Range(0, PotentialMoves.Count)];
        }

        #endregion

        #region Debutant Level

        private AIMove GetDebutantMove()
        {
            AIMove move = GetAIMoveFromNextMove(MoveTree.GetBestMove(YokaiList, Camp, 1, MoveTree.Strategy.OFFENSE));
            if (move != null) return move;

            Debug.LogError("Move is null --> Random");
            return GetRandomMove();
        }

        #endregion
        
        #region Intermediate Level

        private AIMove GetIntermediateMove()
        {
            AIMove move = GetAIMoveFromNextMove(MoveTree.GetBestMove(YokaiList, Camp, 6, MoveTree.Strategy.PROBA));
            if (move != null) return move;

            Debug.LogError("Move is null --> Random");
            return GetRandomPotentialMove();
        }
        private AIMove GetMasterMove()
        {
            AIMove move = GetAIMoveFromNextMove(MoveTree.GetBestMove(YokaiList, Camp, 8, MoveTree.Strategy.OFFENSE));
            if (move != null) return move;

            Debug.LogError("Move is null --> Random");
            return GetRandomMove();
        }

        #endregion

        #region Invincible Level

        private AIMovesImporter movesImporter;

        List<NextMove> potentialMoves;

        private AIMove GetInvincibleMove()
        {
            // Hardcoded moves
            if (Round == 1 && FirstPlayer)
            {
                return FirstPlayer ? GetInvincibleJ1Move1() : GetInvincibleJ2Move1();
            }

            // Get potential moves
            potentialMoves = MoveTree.GetPotentialMoves(YokaiList, Camp);
            if (potentialMoves == null || potentialMoves.Count == 0)
            {
                Debug.Log("No potential moves --> random");
                return GetRandomMove(); // If no moves do random
            }
            if (potentialMoves.Count == 1)
            {
                Debug.Log("Only one potential move !");
                return GetAIMoveFromNextMove(potentialMoves[0]);
            }

            // Search for perfect move
            AIMove move;
            if (movesImporter.TryGetNextMove(new BoardState(YokaiList), out NextMove nextMove))
            {
                Debug.Log("Found move in moves importer : " + new BoardState(YokaiList));
                if (potentialMoves.Contains(nextMove))
                {
                    Debug.Log("Contained in potential moves");
                    move = GetAIMoveFromNextMove(nextMove);
                    if (move != null)
                    {
                        Debug.Log("And it's not null");
                        return move;
                    }
                }
                else
                {
                    (Piece piece, Position oldPos, Position nextPos) nextMoveInfo = nextMove;
                    Debug.LogError("Move " + nextMoveInfo.piece + " from " + nextMoveInfo.oldPos + " to " + nextMoveInfo.nextPos 
                        + " is not contained in potential moves");
                }
            }
            else
            {
                Debug.Log("Didn't found move in importer : " + new BoardState(YokaiList));
            }
            Debug.Log("fallback on intermediate");
            return GetIntermediateMove();
        }

        private AIMove GetRandomPotentialMove()
        {
            return GetAIMoveFromNextMove(potentialMoves[UnityEngine.Random.Range(0, potentialMoves.Count)]);
        }

        private AIMove GetInvincibleJ1Move1()
        {
            return new AIMove(OwnYokais.Find(y => y.GetPawnType() == EPawnType.Kodama), new Vector2Int(1, 2));
        }
        private AIMove GetInvincibleJ2Move1()
        {
            return new AIMove(OwnYokais.Find(y => y.GetPawnType() == EPawnType.Tanuki), new Vector2Int(0, 2));
        }

        #endregion

        private AIMove GetAIMoveFromNextMove(NextMove nextMove)
        {
            if (nextMove == 0)
            {
                Debug.LogError("Next move is 0");
                return null;
            }

            // Get Next move infos
            (Piece piece, Position oldPos, Position nextPos) nextMoveInfo = nextMove;
            EPawnType pawnType = BoardState.GetPawnTypeFromPiece(nextMoveInfo.piece);
            Vector2Int oldPosition = nextMoveInfo.oldPos.ToVector();

            // Search for own yokai with those caracteristics
            IPawn yokai = OwnYokais.Find(y => y.GetPawnType() == pawnType && y.GetCurrentPosition() == oldPosition);
            if (yokai != null)
                return new(yokai, nextMoveInfo.nextPos.ToVector());

            Debug.LogError("Couldn't find own yokai of type " + pawnType + " at position " + oldPosition);

            // If yokai null, maybe a Chick is a Hen or something like that
            // so try to search only with position
            yokai = OwnYokais.Find(y => y.GetCurrentPosition() == oldPosition);
            if (yokai != null)
            {
                Debug.Log("found yokai " + yokai.GetPawnType() + " at position " + oldPosition);
                // then check if the move is valid
                ComputeAllPotentialMoves();
                AIMove move = new(yokai, nextMoveInfo.nextPos.ToVector());
                if (PotentialMoves.Contains(move)) return move;
                Debug.LogError("Couldn't find potential move for yokai " + yokai.GetPawnType() + " to position " + nextMoveInfo.nextPos.ToVector());
            }

            foreach (var yk in OwnYokais)
            {
                Debug.Log(yk.GetPawnType() + " " + yk.GetCurrentPosition());
            }
            return null;
        }


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
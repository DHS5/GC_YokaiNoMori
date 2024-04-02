// ReSharper disable once CheckNamespace

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
        public AICore(int playerIndex, AILevel level, IGameManager gameManager)
        {
            FirstPlayer = playerIndex == 1;
            Camp = FirstPlayer ? ECampType.PLAYER_ONE : ECampType.PLAYER_TWO;
            Level = level;
            GameManager = gameManager;
            YokaiList = GameManager.GetAllPawn();
            BoardCases = GameManager.GetAllBoardCase();
            if (level == AILevel.INVINCIBLE)
            {
                movesImporter = new AIMovesImporter(Camp);
            }
        }
        public AICore(int playerIndex, IGameManager gameManager)
        {
            FirstPlayer = playerIndex == 1;
            Camp = FirstPlayer ? ECampType.PLAYER_ONE : ECampType.PLAYER_TWO;
            Level = AILevel.INVINCIBLE;
            GameManager = gameManager;
            YokaiList = GameManager.GetAllPawn();
            BoardCases = GameManager.GetAllBoardCase();
            movesImporter = new AIMovesImporter(Camp);
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

        public void ComputeMove()
        {
            Round++;

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
            return PotentialMoves[UnityEngine.Random.Range(0, PotentialMoves.Count)];
        }

        #endregion

        #region Debutant Level

        private AIMove GetDebutantMove()
        {
            AIMove move = GetAIMoveFromNextMove(MoveTree.GetBestMove(YokaiList, Camp, 1));
            if (move != null) return move;

            Debug.LogError("Move is null --> Random");
            return GetRandomMove();
        }

        #endregion
        
        #region Intermediate Level

        private AIMove GetIntermediateMove()
        {
            AIMove move = GetAIMoveFromNextMove(MoveTree.GetBestMove(YokaiList, Camp, 6));
            if (move != null) return move;

            Debug.LogError("Move is null --> Random");
            return GetRandomMove();
        }

        #endregion

        #region Invincible Level

        private AIMovesImporter movesImporter;

        private AIMove GetInvincibleMove()
        {
            if (Round == 1 && FirstPlayer)
            {
                return FirstPlayer ? GetInvincibleJ1Move1() : GetInvincibleJ2Move1();
            }

            AIMove move;
            if (movesImporter.TryGetNextMove(new BoardState(YokaiList), out NextMove nextMove))
            {
                Debug.Log("Found move in moves importer");
                move = GetAIMoveFromNextMove(nextMove);
                if (move != null)
                {
                    Debug.Log("and it's not null");
                    return move;
                }
            }
            Debug.Log("fallback on intermediate");
            return GetIntermediateMove();
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

            (Piece piece, Position oldPos, Position nextPos) nextMoveInfo = nextMove;
            EPawnType pawnType = BoardState.GetPawnTypeFromPiece(nextMoveInfo.piece);
            Vector2Int oldPosition = nextMoveInfo.oldPos.ToVector();

            IPawn yokai = OwnYokais.Find(y => y.GetPawnType() == pawnType && y.GetCurrentPosition() == oldPosition);
            if (yokai != null)
                return new(yokai, nextMoveInfo.nextPos.ToVector());

            yokai = OwnYokais.Find(y => y.GetCurrentPosition() == oldPosition);
            if (yokai != null)
            {
                AIMove move = new(yokai, nextMoveInfo.nextPos.ToVector());
                if (PotentialMoves.Contains(move)) return move;
            }

            Debug.LogError("Couldn't find own yokai of type " + pawnType + " at position " + oldPosition);
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
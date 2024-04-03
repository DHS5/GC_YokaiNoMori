using Group15;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;
using YokaiNoMori.Interface;

namespace Group15
{
    public class AICompetitor : MonoBehaviour, ICompetitor
    {
        private AICore aiCore;
        private ECampType camp;

        [SerializeField] private AIMovesImporter j1MovesImporter;
        [SerializeField] private AIMovesImporter j2MovesImporter;

        public void Init(IGameManager igameManager, float timerForAI, ECampType currentCamp)
        {
            camp = currentCamp;
            aiCore = new AICore(currentCamp, igameManager, currentCamp == ECampType.PLAYER_ONE ? j1MovesImporter : j2MovesImporter);
        }


        public ECampType GetCamp()
        {
            return camp;
        }
        public string GetName()
        {
            return "Groupe 15 : DHAUSSY, PONAL";
        }

        

        public void StartTurn()
        {
            aiCore.ComputeMove();
        }
        public void StopTurn()
        {
            // No need
        }
        public void GetDatas()
        {
            aiCore.GetDatas();
        }
    }
}

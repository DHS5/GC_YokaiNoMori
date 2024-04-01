using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;

namespace Group15
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TextAsset textAsset;

        public void DebugBoardState()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var yokai in gameManager.GetAllPawn())
            {
                sb.AppendLine(yokai.GetPawnType() + " at " + yokai.GetCurrentPosition());
            }
            Debug.Log(sb.ToString());
        }


        private int step = 0;
        AIMovesImporter importer;

        public void ParseTextAsset()
        {
            switch (step)
            {
                case 0:
                    importer = new AIMovesImporter(YokaiNoMori.Enumeration.ECampType.PLAYER_TWO); break;
                case 1:
                    importer.GetFileLines(); break;
                default:
                    importer.ParseFile(5000000); break;
            }
            step++;
        }
    }
}

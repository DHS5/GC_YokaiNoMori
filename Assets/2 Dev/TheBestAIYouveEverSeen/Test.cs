using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Group15
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TextAsset textAsset;

        public void DebugBoardState()
        {
            BoardState boardState = new(gameManager.GetAllPawn());

            Debug.Log(boardState);
            Debug.Log(string.Format("0x{0:X}", 9760542094588389905));
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

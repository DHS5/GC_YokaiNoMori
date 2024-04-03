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
            sb.AppendLine(new BoardState(gameManager.GetAllPawn()).ToString());
            foreach (var yokai in gameManager.GetAllPawn())
            {
                sb.AppendLine(yokai.GetPawnType() + " at " + yokai.GetCurrentPosition());
            }
            Debug.Log(sb.ToString());
        }
    }
}

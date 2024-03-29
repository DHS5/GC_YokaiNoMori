using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Group15
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        public void DebugBoardState()
        {
            BoardState boardState = new(gameManager.GetAllPawn());

            Debug.Log(boardState);
            Debug.Log(string.Format("0x{0:X}", 9760542094588389905));
        }
    }
}

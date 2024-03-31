using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

namespace Group15
{
    public class AIMovesImporter
    {
        #region Global Members

        public const string player1MovesFileName = "output_w_d16";
        public const string player2MovesFileName = "output_b_d16";

        private string fileRawContent;
        private string[] fileLines;
        private int linesParsed = 0;

        private Dictionary<BoardState, NextMove> dico;

        #endregion

        #region Constructor

        public AIMovesImporter(ECampType eCampType)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(eCampType == ECampType.PLAYER_ONE ? player1MovesFileName : player2MovesFileName);

            if (textAsset == null)
            {
                Debug.LogError("Couldn't find file");
                return;
            }

            dico = new Dictionary<BoardState, NextMove>();
            fileRawContent = textAsset.text;
            Debug.Log("Loaded file");
        }

        #endregion

        #region Parsing

        public void GetFileLines()
        {
            fileLines = fileRawContent.Split('\n');
            Debug.Log("Split file in " + fileLines.Length + " lines");
        }

        public void ParseFile(int lineNumber)
        {
            string[] lineContent;
            int i;
            for (i = linesParsed; i < linesParsed + lineNumber; i++)
            {
                if (i >= fileLines.Length) break;

                lineContent = fileLines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (lineContent.Length >= 2)
                    dico.TryAdd(new BoardState(lineContent[0]), new NextMove(lineContent[1]));
            }
            Debug.Log("Parsed " + (i - linesParsed) + " new lines");

            linesParsed = i;
        }

        #endregion

        #region Accessors

        public bool TryGetNextMove(BoardState boardState, out NextMove nextMove) => dico.TryGetValue(boardState, out nextMove);

        #endregion
    }
}

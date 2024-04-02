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

        public const string player1MovesFileName = "w_16";
        public const string player2MovesFileName = "b_15";

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
            GetFileLines();
            //ParseFile(35000000);
            ParseFile(50000);
            Debug.Log("Dico length " + dico.Count);
        }

        #endregion

        #region Parsing

        public void GetFileLines()
        {
            fileLines = fileRawContent.Split('\n');
            Debug.Log("Split file in " + fileLines.Length + " lines");
            fileRawContent = null;
        }

        public void ParseFile(int lineNumber)
        {
            if (linesParsed >= fileLines.Length) return;

            string[] lineContent;
            int i;
            for (i = linesParsed; i < linesParsed + lineNumber; i++)
            {
                if (i >= fileLines.Length)
                {
                    fileLines = null;
                    break;
                }

                lineContent = fileLines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (lineContent.Length >= 2)
                {
                    if (!dico.TryAdd(new BoardState(lineContent[0]), new NextMove(lineContent[1])))
                    {
                        //Debug.Log("board " + lineContent[0] + " is already in the dico");
                        Debug.Log("duplicate");
                    }
                }
                else
                {
                    Debug.Log("line " + fileLines[i] + " could not be split correctly");
                }
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

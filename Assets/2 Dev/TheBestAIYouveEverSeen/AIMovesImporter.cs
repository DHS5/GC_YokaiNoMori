using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YokaiNoMori.Enumeration;

namespace Group15
{
    [CreateAssetMenu(fileName = "AI Moves Importer", menuName = "AI Moves Importer")]
    public class AIMovesImporter : ScriptableObject
    {
        #region Global Members

        [SerializeField] private TextAsset textAsset;

        private string fileRawContent;
        private string[] fileLines;

        [SerializeField] private List<ulong> ulongs = new();
        [SerializeField] private List<ushort> ushorts = new();

        #endregion

        #region Runtime Members

        private Dictionary<BoardState, NextMove> dico;

        #endregion

        #region Editor Parsing

        [ContextMenu("Parse File")]
        private void ParseFile()
        {
            GetFileLines();
            ParseLines();
        }

        [ContextMenu("Clear Lists")]
        private void ClearLists()
        {
            ulongs.Clear();
            ushorts.Clear();
        }


        private void GetFileLines()
        {
            if (textAsset == null)
            {
                Debug.LogError("Text asset is null");
                return;
            }

            fileRawContent = textAsset.text;
            fileLines = fileRawContent.Split('\n');
            Debug.Log("Split file in " + fileLines.Length + " lines");
            fileRawContent = null;
        }

        private void ParseLines()
        {
            ulongs.Clear();
            ushorts.Clear();

            string[] lineContent;
            for (int i = 0; i < fileLines.Length; i++)
            {
                lineContent = fileLines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (lineContent.Length >= 2)
                {
                    ulongs.Add(Convert.ToUInt64(lineContent[0], 16));
                    ushorts.Add(Convert.ToUInt16(lineContent[1], 16));
                }
            }

            Debug.Log("ULong : " + ulongs.Count + " / UShort : " + ushorts.Count);
        }

        #endregion

        #region Runtime Parsing

        public void ParseNumbers()
        {
            dico = new();

            for (int i = 0; i < ulongs.Count; i++)
            {
                if (!dico.TryAdd(new BoardState(ulongs[i]), ushorts[i]))
                {
                    Debug.Log("duplicate");
                }
            }
            Debug.Log("Dico contains " + dico.Count + " boards");
        }

        #endregion

        #region Accessors

        public bool TryGetNextMove(BoardState boardState, out NextMove nextMove) => dico.TryGetValue(boardState, out nextMove);

        #endregion
    }
}

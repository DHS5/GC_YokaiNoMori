using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class BoardRegister
{
    private static List<int[,]> _boards = new();

    public static void Init()
    {
        _boards.Clear();

        Register();
    }

    public static void Register()
    {
        int[,] board = Board.GetCurrentBoard();
        int lineNumber = board.GetLength(1);
        int columnNumber = board.GetLength(0);
        int[,] newBoard = new int[columnNumber, lineNumber];
        for (int line = 0; line < lineNumber; line++)
        {
            for (int column = 0; column < columnNumber; column++)
            {
                newBoard[column, line] = board[column, line];
            }
        }

        _boards.Add(newBoard);

        if (CheckForRepetion())
        {
            GameManager.Winner(0);
        }
    }

    public static bool CheckForRepetion()
    {
        int count = _boards.Count;
        if (count < 7) return false;

        while (_boards.Count > 7)
        {
            _boards.RemoveAt(0);
        }

        return AreEqual(6, 4) && AreEqual(4, 2) && AreEqual(2,0);
    }

    public static bool AreEqual(int index1, int index2)
    {
        for (int line = 0; line < _boards[index1].GetLength(1); line++)
        {
            for (int column = 0; column < _boards[index1].GetLength(0); column++)
            {
                if (_boards[index1][column, line] != _boards[index2][column, line]) return false;
            }
        }
        return true;
    }


    private static void DebugAtIndex(int index)
    {
        int[,] last = _boards[index];

        StringBuilder sb = new();
        sb.AppendLine("move " + index);
        for (int line = last.GetLength(1) - 1; line >= 0; line--)
        {
            for (int column = 0; column < last.GetLength(0); column++)
            {
                sb.Append(last[column, line]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }
}

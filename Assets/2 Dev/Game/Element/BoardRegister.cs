using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
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
        _boards.Add(Board.GetCurrentBoard());
        DebugLast();

        if (CheckForRepetion())
        {
            GameManager.Winner(0);
        }
    }

    public static bool CheckForRepetion()
    {
        int count = _boards.Count;
        if (count < 5) return false;

        while (_boards.Count > 5)
        {
            _boards.RemoveAt(0);
        }

        return _boards[4] == _boards[2] && _boards[2] == _boards[0];
    }

    private static void DebugLast()
    {
        int index = (_boards.Count - 1);
        Debug.Log(_boards.Count + " " + index);
        int[,] last = _boards[index];

        StringBuilder sb = new();
        for (int line = last.GetLength(0); line >= 0; line--)
        {
            for (int column = 0; column < last.GetLength(1); column++)
            {
                sb.Append(last[column,line]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
    }
}

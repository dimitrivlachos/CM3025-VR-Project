using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MLScriptableObjs;

namespace WFC
{
    public class WFC_I_Image : WFC_Interpreter
    {
        private Texture2D _result;
        private Mood_Data _songMood;
        public WFC_I_Image(Cell[,] grid, Mood_Data songMood) : base(grid)
        {
            _songMood = songMood;
            m_grid = grid;
            _result = new Texture2D(grid.GetLength(1) * 10, grid.GetLength(0) * 10);
        }

        public Texture2D CreateImage()
        {
            int col, row;
            Color[] pixels = _result.GetPixels(0);
            for (int i = 0; i < pixels.Length; i++)
            {
                row = i / _result.width / 10;
                col = i % _result.width / 10;

                int id = m_grid[row, col].FinalSolution;

                pixels[i] = _songMood.GetFragmentByID(id).ColorFragment;
            }
            _result.SetPixels(pixels);
            _result.Apply();
            return _result;
        }
    }
}
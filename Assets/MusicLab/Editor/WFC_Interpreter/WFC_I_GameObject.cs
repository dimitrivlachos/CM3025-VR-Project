using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public class WFC_I_GameObject : WFC_Interpreter
    {
        private GameObject[] m_primitives;

        public WFC_I_GameObject(Cell[,] grid, GameObject[] primitives) : base(grid)
        {
            m_grid = grid;
            m_primitives = primitives;
        }
        /// <summary>
        /// Instantiates game objects with the given distance
        /// </summary>
        public void Display_Game_Objects(float distance_between_objects)
        {
            for (int row = 0; row < m_grid.GetLength(0); row++)
                for (int col = 0; col < m_grid.GetLength(1); col++)
                {
                    {
                        int idx = m_grid[row, col].FinalSolution;
                        GameObject.Instantiate(m_primitives[idx], new Vector3(1 * col, -1 * row, 0) * distance_between_objects, Quaternion.identity);
                    }
                }
        }
    }
}
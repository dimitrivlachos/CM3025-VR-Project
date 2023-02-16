//20/JUL/2022
//Pablo Peñaloza
//A cell object is the unit the grid is composed of for the WFC algorithm. 

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    public class Cell
    {
        //Properties
        private bool m_is_Collapsed;
        private int m_final_solution;
        private int m_col;
        private int m_row;
        public List<int> m_possible_solutions;

        //Setters and Getters
        public bool IsCollapsed
        {
            get { return m_is_Collapsed; }
            set { m_is_Collapsed = value; }
        }
        public List<int> Possible_solutions
        {
            get { return m_possible_solutions; }
        }
        public int Column
        {
            get { return m_col; }
        }
        public int Row
        {
            get { return m_row; }
        }

        public int FinalSolution
        {
            get { return m_final_solution; }
        }
        //Constructor
        public Cell(int n_of_primitives, int row, int column)
        {
            m_possible_solutions = new List<int>();
            m_is_Collapsed = false;
            m_col = column;
            m_row = row;
            m_final_solution = -1;

            for (int i = 0; i < n_of_primitives; i++)
                m_possible_solutions.Add(i);
        }

        //Methods
        public void check_valid_options(Cell[,] grid, Dictionary<int, Primitive> primitives)
        {
            //Check UP
            if (m_row > 0)
            {
                Cell cell_up = grid[m_row - 1, m_col];
                if (cell_up.IsCollapsed)
                {
                    //Intersects possible options with available options
                    m_possible_solutions = m_possible_solutions.Intersect(primitives[cell_up.FinalSolution].Down).ToList();
                }
            }

            //Check Right
            if (m_col < grid.GetLength(1) - 1)
            {
                Cell cell_right = grid[m_row, m_col + 1];
                if (cell_right.IsCollapsed)
                {
                    //Intersects possible options with available options
                    m_possible_solutions = m_possible_solutions.Intersect(primitives[cell_right.FinalSolution].Left).ToList();
                }
            }

            //Check Down
            if (m_row < grid.GetLength(0) - 1)
            {
                Cell cell_down = grid[m_row + 1, m_col];
                if (cell_down.IsCollapsed)
                {
                    //Intersects possible options with available options
                    m_possible_solutions = m_possible_solutions.Intersect(primitives[cell_down.FinalSolution].Up).ToList();
                }
            }

            //Check Left
            if (m_col > 0)
            {
                Cell cell_left = grid[m_row, m_col - 1];
                if (cell_left.IsCollapsed)
                {
                    //Intersects possible options with available options
                    m_possible_solutions = m_possible_solutions.Intersect(primitives[cell_left.FinalSolution].Right).ToList();
                }
            }
        }

        public virtual void collapseCell()
        {
            IsCollapsed = true;

            int pick = Random.Range(0, Possible_solutions.Count);
            m_final_solution = Possible_solutions[pick];
            Possible_solutions.RemoveRange(0, Possible_solutions.Count);
            Possible_solutions.Add(FinalSolution);
        }
    }
}
//20/JUL/2022
//Pablo Peñaloza
//The WFC class implements the WFC algorithm to create procedular grid
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MLScriptableObjs;

namespace WFC
{
    public class Wave_Function_Collapse : I_GridOverride
    {
        private Dictionary<int, Primitive> m_primitives;   //Diccionario de primitivos con un int the key
        private Cell[,] m_grid;                     //[Rows, Columns]
        private bool m_stop_flag;

        private int m_current_min_entropy;
        private int m_n_cells_min_entropy;
        private Cell[] m_collapsable_cells;

        /// <summary>
        /// Initialize the WFC algorithm
        /// </summary>
        /// <param name="grid"></param>
        public Wave_Function_Collapse(int grid_width, int grid_height)
        {
            m_stop_flag = false;
            m_grid = new Cell[grid_height, grid_width];
            m_primitives = new Dictionary<int, Primitive>();
        }

        /// <summary>
        /// Gets a list of the cells with least enrrope
        /// </summary>
        /// <returns></returns>
        protected void Get_Cells_With_Least_Entropy()
        {
            Set_Least_Entropy();
            m_collapsable_cells = new Cell[m_n_cells_min_entropy];

            int idx = 0;
            foreach (Cell cell in m_grid)
            {
                if (cell.Possible_solutions.Count == m_current_min_entropy && !cell.IsCollapsed)
                {
                    m_collapsable_cells[idx] = cell;
                    idx++;
                }
            }
        }

        /// <summary>
        /// Chooses a cell from the collapsable cells.
        /// </summary>
        protected Cell Collapse_Cell()
        {
            if (m_collapsable_cells != null)
            {
                int random_idx = Random.Range(0, m_n_cells_min_entropy);
                Cell cell_Chosen = m_collapsable_cells[random_idx];

                return cell_Chosen;
            }
            else
            {
                Debug.Log("No collapsable cells");
                return null;
            }
        }

        /// <summary>
        /// Acces each cell and updates its entropy depending its enviroment
        /// </summary>
        protected void updateEntropy()
        {
            for (int row = 0; row < m_grid.GetLength(0); row++)
            {
                for (int col = 0; col < m_grid.GetLength(1); col++)
                    if (!m_grid[row, col].IsCollapsed)
                        m_grid[row, col].check_valid_options(m_grid, m_primitives);

            }
        }

        /// <summary>
        /// Gets the minimun entropy encuontered in the grid and how many cells have that entropy. 
        /// </summary>
        protected void Set_Least_Entropy()
        {
            m_current_min_entropy = int.MaxValue;
            m_n_cells_min_entropy = 0;
            foreach (Cell cell in m_grid)
            {
                if (!cell.IsCollapsed) //If cell is not collapsed
                {
                    if (m_current_min_entropy > cell.Possible_solutions.Count)
                    { //Get the minimun entropy in the system
                        m_current_min_entropy = cell.Possible_solutions.Count;
                        m_n_cells_min_entropy = 1;
                    }
                    else if (m_current_min_entropy == cell.Possible_solutions.Count)
                    {//Get the total of cells with the minimun entropy
                        m_n_cells_min_entropy++;
                    }
                }
            }
        }

        /// <summary>
        /// Prints the current state of the grid
        /// </summary>
        protected void Debug_Grid()
        {
            //Print Current map
            string cad = "\n";
            int colNumber = m_grid.GetLength(1);
            int rowNumber = m_grid.GetLength(0);
            for (int row = 0; row < rowNumber; row++)
            {
                for (int col = 0; col < colNumber; col++)
                {
                    cad += m_grid[row, col].FinalSolution + ", " /*+ "[" + row + "]" + "[" + col + "]"*/;
                }
                cad += "\n";
            }
            Debug.Log(cad);

        }


        /// <summary>
        /// Run the initialize the values of each primitive
        /// </summary>
        public void Load_Primitives(Mood_Data allRules)
        {
            int number_of_primitives = allRules.fragmentList.Count;


            for (int i = 0; i < number_of_primitives; i++)
            {
                int[] current_sockets = new int[allRules.fragmentList[i].Sockets.Length];
                for (int j = 0; j < current_sockets.Length; j++)
                {
                    current_sockets[j] = allRules.fragmentList[i].Sockets[j];
                }
                m_primitives.Add(allRules.fragmentList[i].id, new Primitive());
                m_primitives[allRules.fragmentList[i].id].InitializePrimitive(allRules.fragmentList[i].id, allRules.fragmentList[i].row_channel, current_sockets);

            }

        }

        /// <summary>
        /// Creates and asigns a value to the cells that comform the grid
        /// </summary>
        public void Create_Grid()
        {
            for (int row = 0; row < m_grid.GetLength(0); row++)
            {
                for (int col = 0; col < m_grid.GetLength(1); col++)
                    m_grid[row, col] = new Cell(m_primitives.Count, row, col);
            }
        }

        /// <summary>
        /// Creates the rules of adjacency of the cells
        /// </summary>
        public void Create_Rules()
        {
            foreach (KeyValuePair<int, Primitive> primitive in m_primitives)
            {
                m_primitives[primitive.Key].create_adjacent_rules(m_primitives);
            }
        }

        public void OverrideColumEntropy(int column, int[] newOptions)
        {
            for (int row = 0; row < m_grid.GetLength(0); row++)
            {
                m_grid[row, column].Possible_solutions.Clear();
                for (int option = 0; option < newOptions.Length; option++)
                {
                    m_grid[row, column].Possible_solutions.Add(newOptions[option]);
                }
            }
        }
        public void OverrideRowEntropy(int row, int[] newOptions)
        {
            for (int col = 0; col < m_grid.GetLength(1); col++)
            {
                m_grid[row, col].Possible_solutions.Clear();
                for (int option = 0; option < newOptions.Length; option++)
                {
                    m_grid[row, col].Possible_solutions.Add(newOptions[option]);
                }
            }
        }
        public void OverrideSingleCell(int row, int column, int[] new_options)
        {
            m_grid[row, column].Possible_solutions.Clear();

            for (int i = 0; i < new_options.Length; i++)
                m_grid[row, column].Possible_solutions.Add(new_options[i]);
        }


        //Music Overrides [Send to child after]

        ///
        /// <summary>
        /// Reads the primitives and 
        /// </summary>
        public void OverrideGridForMusic(int[] silences, int nMelodies, int nBasses)
        {
            //Overriding firts and last row
            List<int> FirtsRow = new List<int>();
            List<int> SecondRow = new List<int>();
            List<int> LastRow = new List<int>();
            List<int> Melodies = new List<int>();
            List<int> Basses = new List<int>();
            foreach (KeyValuePair<int, Primitive> currPrimitive in m_primitives)
            {
                if (currPrimitive.Value.RowParent == 0) FirtsRow.Add(currPrimitive.Value.ID);
                if (currPrimitive.Value.RowParent == 1) SecondRow.Add(currPrimitive.Value.ID);
                if (currPrimitive.Value.RowParent == 2) LastRow.Add(currPrimitive.Value.ID);
            }

            for (int i = 0; i < nMelodies; i++)
            {
                int melodie = Random.Range(0, FirtsRow.Count);
                Melodies.Add(FirtsRow[melodie]);
            }

            for (int i = 0; i < nBasses; i++)
            {
                int bass = Random.Range(0, FirtsRow.Count);
                Basses.Add(FirtsRow[bass]);
            }

            for (int i = 0; i < silences.Length; i++)
            {
                Melodies.Add(silences[i]);
            }
            OverrideRowEntropy(0, Melodies.ToArray());
            OverrideRowEntropy(1, SecondRow.ToArray());
            OverrideRowEntropy(2, LastRow.ToArray());

            //Overriding the middle row of chords
            //int[] progression = CreateProgression(cords);
            //OverrideGridWithProgression(progression);

            //OverrideSingleCell(1, m_grid.GetLength(1) - 1, new int []{ baseChord }); //Last cord has to be the base Chord 
        }

        /// <summary>
        /// Recieves the ids of the cords that make a progression
        /// </summary>
        /// <param name="cords"></param>
        protected int[] CreateProgression(int[] cords)
        {
            List<int> cordsList = cords.ToList();
            int[] progression = new int[cords.Length];
            int possibleCord;

            //All the cords
            for (int i = 0; i < cords.Length; i++)
            {
                possibleCord = Random.Range(0, cordsList.Count);
                progression[i] = cordsList[possibleCord];
                cordsList.RemoveAt(possibleCord);
            }
            return progression;
        }

        /// <summary>
        /// Overrides the middle column to make the progression appear
        /// </summary>
        /// <param name="progression"></param>
        protected void OverrideGridWithProgression(int[] progression)
        {
            for (int col = 0; col < m_grid.GetLength(1); col++)
            {
                OverrideSingleCell(1, col, new int[] { progression[col % progression.Length] });
            }
        }
        public Cell[,] Run(bool debug)
        {
            do
            {
                if (debug)
                    Debug_Grid();

                //[4.]
                Get_Cells_With_Least_Entropy();

                if (m_collapsable_cells.Length == 0)
                {
                    m_stop_flag = true;
                    continue;
                }

                //[5.]
                Cell cell_Chosen = Collapse_Cell();

                //[6.] -- Collapse cell
                if (cell_Chosen.Possible_solutions.Count != 0)
                    m_grid[cell_Chosen.Row, cell_Chosen.Column].collapseCell();
                else
                {
                    if (debug)
                        Debug.Log("RESTARTED");
                    Create_Grid();
                }
                //[7.]
                updateEntropy();


            } while (m_stop_flag == false);

            return m_grid;
        }
    }
}
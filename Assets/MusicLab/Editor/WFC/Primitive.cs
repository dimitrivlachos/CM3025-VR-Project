//20/JUL/2022
//Pablo Peñaloza
//The primitive class keeps the information used in each cell to create a grid with the WFC algortihm

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public class Primitive
    {
        private int[] m_sockets;

        private List<int> m_up;
        private List<int> m_right;
        private List<int> m_down;
        private List<int> m_left;

        private int m_id;

        private int m_row;


        //Getters and Setters
        public int ID
        {
            get { return m_id; }
        }
        public List<int> Up
        { get { return m_up; } }
        public List<int> Right
        { get { return m_right; } }
        public List<int> Down
        { get { return m_down; } }
        public List<int> Left
        { get { return m_left; } }
        public int RowParent { get { return m_row; } }

        /// <summary>
        /// Initializes the primitive
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="sockets has to be a 4 dimension array"></param>
        public virtual void InitializePrimitive(int tag, int[] sockets)
        {
            //Checks if the sockets of the primitive has only for values (UP, RIGHT, DOWN, LEFT)
            if (sockets.Length == 4)
                m_sockets = sockets;
            else
                m_sockets = new int[] { 0, 0, 0, 0 };

            m_up = new List<int>();
            m_right = new List<int>();
            m_down = new List<int>();
            m_left = new List<int>();

            m_id = tag;
        }

        public virtual void InitializePrimitive(int tag, int rowParent, int[] sockets)
        {
            //Checks if the sockets of the primitive has only for values (UP, RIGHT, DOWN, LEFT)
            if (sockets.Length == 4)
                m_sockets = sockets;
            else
                m_sockets = new int[] { 0, 0, 0, 0 };

            m_up = new List<int>();
            m_right = new List<int>();
            m_down = new List<int>();
            m_left = new List<int>();

            m_id = tag;
            m_row = rowParent;
        }

        /// <summary>
        /// Read the rules created and assigns them to each primitive
        /// </summary>
        public void create_adjacent_rules(Dictionary<int, Primitive> primitive_arr)
        {
            foreach (KeyValuePair<int, Primitive> primitive in primitive_arr)
            {
                //Create rules for primitives UP
                if (m_sockets[0] == primitive.Value.m_sockets[2])
                {
                    m_up.Add(primitive.Value.m_id);
                }
                //Create rules for primitives RIGHT

                if (m_sockets[1] == primitive.Value.m_sockets[3])
                {
                    m_right.Add(primitive.Value.m_id);
                }//Create rules for primitives DOWN

                if (m_sockets[2] == primitive.Value.m_sockets[0])
                {
                    m_down.Add(primitive.Value.m_id);
                }//Create rules for primitives LEFT

                if (m_sockets[3] == primitive.Value.m_sockets[1])
                {
                    m_left.Add(primitive.Value.m_id);
                }
            }
        }

    }
}
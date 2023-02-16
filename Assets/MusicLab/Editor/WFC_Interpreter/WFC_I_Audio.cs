using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MLScriptableObjs;
using SavWav;
using MusicForge;

namespace WFC
{
    public class WFC_I_Audio : WFC_Interpreter
    {
        private Dictionary<int, AudioClip> m_audioPrimitives;

        public WFC_I_Audio(Cell[,] grid, Mood_Data songMood) : base(grid)
        {
            m_audioPrimitives = new Dictionary<int, AudioClip>();
            LoadAudioPrimitives(songMood);
        }


        #region SongCreator
        /// <summary>
        /// Recieves a cell grid and an array of clips and returns an array form the grid
        /// </summary>
        /// <returns></returns>
        public AudioClip Create_Audio_Track()
        {
            AudioClip[] trackArray = new AudioClip[m_grid.GetLength(1)]; //Array of the row that is cocatenated
            AudioClip[] chanelArray = new AudioClip[m_grid.GetLength(0)]; //Array of the channel 
            AudioClip temp;

            //Cocatenate all the rows
            for (int row = 0; row < m_grid.GetLength(0); row++)
            {
                for (int col = 0; col < m_grid.GetLength(1); col++)
                {
                    m_audioPrimitives.TryGetValue(m_grid[row, col].FinalSolution, out temp);
                    trackArray[col] = temp;
                }


                chanelArray[row] = ClipCombiner.Cocatenate(trackArray, 428);

            }

            //_SaveSong("Assets/MusicLab/Editor/tmp", "Ch1", chanelArray[0]);
            //_SaveSong("Assets/MusicLab/Editor/tmp", "Ch2", chanelArray[1]);
            //_SaveSong("Assets/MusicLab/Editor/tmp", "Ch3", chanelArray[2]);


            chanelArray[0] = ClipCombiner.Mix(chanelArray[0], 0.4f, chanelArray[1], "temp");
            chanelArray[0] = ClipCombiner.Mix(chanelArray[0], 0.6f, chanelArray[2], "temp");

            return chanelArray[0];
        }

        #endregion

        #region AudioClipImport
        private void LoadAudioPrimitives(Mood_Data songMood)
        {

            for (int i = 0; i < songMood.fragmentList.Count; i++)
            {
                m_audioPrimitives.Add(songMood.fragmentList[i].id, songMood.fragmentList[i].clip_fragment);
            }
        }
        #endregion

        #region SaveChannel
        private static void _SaveSong(string path, string songName, AudioClip Clip)
        {
            if (Clip != null)
            {
                SavWav.SavWav.Save(path, songName, Clip);
                AssetDatabase.Refresh();
            }
            else Debug.Log("No song created");
        }
        #endregion
    }
}
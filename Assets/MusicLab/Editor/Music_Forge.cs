using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using MLScriptableObjs;
using SavWav;
using WindowClasses;
using WFC;

namespace MusicForge
{
    public class Music_Forge
    {
        private static AudioClip _clip;
        private static Texture2D _resImage;
        public static AudioClip Clip { get { return _clip; } }
        public static Texture2D ResImage { get { return _resImage; } }

        public static void CreateSong(MouseUpEvent evt)
        {
            int m_columns = 5;
            int m_rows = 3;

            WFC_I_Audio m_Audio_Interpreter;
            WFC_I_Image m_ImageInterpreter;
            Wave_Function_Collapse wave_function_collapse;

            Mood_Data m_sockets_data;

            Cell[,] m_grid;

            //WFC
            wave_function_collapse = new Wave_Function_Collapse(m_columns, m_rows);
            m_sockets_data = SongCreatorClass.CurrenMood;
            wave_function_collapse.Load_Primitives(m_sockets_data);
            wave_function_collapse.Create_Grid();
            wave_function_collapse.Create_Rules();

            wave_function_collapse.OverrideGridForMusic(SongCreatorClass.CurrenMood.FragmentSilences(), 2, 2);

            m_grid = wave_function_collapse.Run(false);

            //Audio Interpreter
            m_Audio_Interpreter = new WFC_I_Audio(m_grid, SongCreatorClass.CurrenMood);
            _clip = m_Audio_Interpreter.Create_Audio_Track();
            SaveSong("Assets/MusicLab/Editor/tmp", "tempSong");

            //Image Interpreter
            m_ImageInterpreter = new WFC_I_Image(m_grid, SongCreatorClass.CurrenMood);
            _resImage = m_ImageInterpreter.CreateImage();
        }

        private static void SaveSong(string path, string songName)
        {
            if (Clip != null)
            {
                SavWav.SavWav.Save(path, songName, Clip);
                AssetDatabase.Refresh();
            }
            else Debug.Log("No song created");
        }
    }
}
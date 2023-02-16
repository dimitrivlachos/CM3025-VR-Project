using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Windows;
using MLScriptableObjs;
using MusicForge;

namespace WFC
{
    public class RulesLoader
    {
        /// <summary>
        /// Loads rules form the resources folder
        /// </summary>
        /// <param name="location"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Mood_Data FromResources(string location, string name)
        {
            Mood_Data temp = Resources.Load<Mood_Data>(location + "/" + name);
            return temp;
        }

        public static Mood_Data FromEditorAssets(string moodDataname)
        {
            Mood_Data result = AssetDatabase.LoadAssetAtPath<Mood_Data>(AppConstants.RELATIVEASSETPATH + "/Editor/Rules/" + moodDataname + ".asset");
            return result;
        }

        public static Genre_Data GenreFromEditorAssets(string GenreDataname)
        {
            Genre_Data result = AssetDatabase.LoadAssetAtPath<Genre_Data>(AppConstants.RELATIVEASSETPATH + "/Editor/Rules/" + GenreDataname + ".asset");
            return result;
        }

        public static Music_Library LibraryFromEditorAssets(string LibraryDataname)
        {
            Music_Library result = AssetDatabase.LoadAssetAtPath<Music_Library>(AppConstants.RELATIVEASSETPATH + "/Editor/Rules/" + LibraryDataname + ".asset");
            return result;
        }
    }
}
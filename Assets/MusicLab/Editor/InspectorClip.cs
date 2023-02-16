using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using MusicForge;

namespace InspectorClip
{
    public static class InspectorClip
    {
        public static void PlayClip()
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AppConstants.RELATIVEASSETPATH + "/Editor/tmp/tempSong.wav");
            int startSample = 0;
            bool loop = false;

            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );

            //Debug.Log(method);
            method.Invoke(
                null,
                new object[] { clip, startSample, loop }
            );
        }
        public static void StopAllClips()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { },
                null
            );

            //Debug.Log(method);
            method.Invoke(
                null,
                new object[] { }
            );
        }

    }
}

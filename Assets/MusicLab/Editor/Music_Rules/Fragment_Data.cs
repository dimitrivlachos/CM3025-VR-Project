using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FragmentData
{
    [System.Serializable]
    public class Fragment_Data
    {
        public string tag;
        public int id;
        public AudioClip clip_fragment;
        public Color32 ColorFragment;
        public int row_channel;
        public bool isChord;
        public bool isSilence;
        public bool isBaseChord;
        public int[] Sockets = { 0, 0, 0, 0 };
    }
}
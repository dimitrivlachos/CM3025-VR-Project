using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLScriptableObjs
{
    [CreateAssetMenu(fileName = "Genre_Data", menuName = "ScriptableObjects/Genre data", order = 1)]
    [System.Serializable]
    public class Genre_Data : ScriptableObject
    {
        public List<Mood_Data> moodList;
    }
}
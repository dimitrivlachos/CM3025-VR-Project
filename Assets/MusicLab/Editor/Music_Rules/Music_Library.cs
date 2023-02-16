using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLScriptableObjs;

namespace MLScriptableObjs
{
    [CreateAssetMenu(fileName = "Library", menuName = "ScriptableObjects/LibraryData", order = 3)]
    [System.Serializable]
    public class Music_Library : ScriptableObject
    {
        public List<Genre_Data> GenreList;
    }
}
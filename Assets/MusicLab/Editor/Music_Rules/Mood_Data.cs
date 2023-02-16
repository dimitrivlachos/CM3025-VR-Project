using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FragmentData;

namespace MLScriptableObjs
{
    [CreateAssetMenu(fileName = "Mood_data", menuName = "ScriptableObjects/Mood data", order = 2)]
    [System.Serializable]
    public class Mood_Data : ScriptableObject
    {
        public List<Fragment_Data> fragmentList;

        public int[] FragmentChords()
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < fragmentList.Count; i++)
            {
                if (fragmentList[i].isChord)
                    temp.Add(fragmentList[i].id);
            }

            return temp.ToArray();
        }

        public int GetBaseChord()
        {
            int res = -1;
            for (int i = 0; i < fragmentList.Count; i++)
            {
                if (fragmentList[i].isBaseChord)
                {
                    res = fragmentList[i].id;
                    break;
                }


            }

            return res;
        }

        /// <summary>
        /// Get a fragment from the list by giving its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Fragment_Data GetFragmentByID(int id)
        {
            for (int i = 0; i < fragmentList.Count; i++)
            {
                if (id == fragmentList[i].id)
                    return fragmentList[i];
            }
            return null;
        }

        public int[] FragmentSilences()
        {
            List<int> temp = new List<int>();
            for (int i = 0; i < fragmentList.Count; i++)
            {
                if (fragmentList[i].isSilence)
                    temp.Add(fragmentList[i].id);
            }
            return temp.ToArray();
        }


        /// <summary>
        /// Reduces the fragment list to one with fewer fragments
        /// </summary>
        /// <param name="mood_Data"></param>
        /// <returns></returns>
        public static Mood_Data GetCuratedMoodData(ref Mood_Data mood_Data, int size, int rows)
        {
            return new Mood_Data();
        }
    }
}
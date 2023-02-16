using System.Collections.Generic;

namespace DynamicMeshCutter
{
    public enum Behaviour
    {
        Stone,
        Ragdoll,
        Animation
    }

    public enum RagdollPhysics
    {
        LeaveAsIs,
        Kinematic,
        NonKinematic
    }

    public enum GroupCondition
    {
        Exact,
        ContainsAll,
        ContainsAny
    }

    [System.Serializable]
    public class GroupBehaviours
    {
        public string Name;
        public Behaviour Behaviour;
        public GroupCondition Condition;
        public List<DynamicRagdollPart> Parts = new List<DynamicRagdollPart>();

        public List<int> Indices = new List<int>();

        public bool Passes(int[] indices)
        {
            switch (Condition)
            {
                case GroupCondition.Exact:
                    if (indices.Length == Parts.Count)
                    {
                        for (int i = 0; i < indices.Length; i++)
                        {
                            if (!Indices.Contains(indices[i]))
                                return false;
                        }
                        return true;
                    }
                    break;
                case GroupCondition.ContainsAll:
                    for (int i = 0; i < Indices.Count; i++)
                    {
                        bool contains = false;
                        for (int j = 0; j < indices.Length; j++)
                        {
                            if (indices[j] == Indices[i])
                            {
                                contains = true;
                                break;
                            }
                        }
                        if (!contains)
                            return false;
                    }
                    return true;
                case GroupCondition.ContainsAny:
                    for (int i = 0; i < Indices.Count; i++)
                    {
                        for (int j = 0; j < indices.Length; j++)
                        {
                            if (indices[j] == Indices[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }
    }

}

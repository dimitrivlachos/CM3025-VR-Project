using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DynamicMeshCutter
{
    public static class MeshTargetShephard
    {
        public static List<MeshTarget> Targets = new List<MeshTarget>();
        public static void RegisterMeshTarget(MeshTarget target)
        {
            if (!Targets.Contains(target))
                Targets.Add(target);
        }

        public static void UnRegisterMeshTarget(MeshTarget target)
        {
            if (Targets.Contains(target))
                Targets.Remove(target);
        }
    }
}

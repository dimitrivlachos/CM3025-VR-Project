using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicMeshCutter
{

    public class VertexInfo
    {
        public Vector3 Vertex;
        public List<int> Occasions_Vertex;
        public List<int> Occasions_Submesh;
        public List<Vector3> Neighbors;
        public VertexInfo(int occasion_Vertex, int occasion_Submesh)
        {
            Occasions_Vertex = new List<int>() { occasion_Vertex };
            Occasions_Submesh = new List<int>() { occasion_Submesh };
            Neighbors = new List<Vector3>();
        }
    }

    /// <summary>
    /// helper which holds all the necessary data to create a mesh
    /// </summary>
    public class VirtualMesh
    {
        public Mesh Mesh;

        public int[] Triangles;
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Vector2[] UVs;
        public BoneWeight[] BoneWeights;

        public DynamicRagdoll DynamicRagdoll; //the original ragdoll
        public int[] Assignments; //ragdoll assignments
        public Dictionary<int, Vector3[]> DynamicGroups;
        public Dictionary<int, Bounds> AdjustedBounds; //new bounds for cut off rigidbodies

        private int _subMeshCount;
        private int[][] _subMeshIndices;

        public int UniqueVerticesCount = -1;
        public int SubMeshCount
        {
            get { return _subMeshCount; }
        }

        private bool _hasBoneWeight = false;
        public bool HasBoneWeight
        {
            get { return _hasBoneWeight; }
        }
        public VirtualMesh() { }

        public VirtualMesh(Mesh mesh)
        {
            Mesh = mesh;

            Vertices = mesh.vertices;
            Normals = mesh.normals;
            UVs = mesh.uv;
            BoneWeights = mesh.boneWeights;

            if (mesh.boneWeights.Length > 0)
            {
                _hasBoneWeight = true;
            }

            _subMeshCount = mesh.subMeshCount;
            _subMeshIndices = new int[_subMeshCount][];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                _subMeshIndices[i] = mesh.GetIndices(i);
            }
        }
        public int[] GetIndices(int index)
        {
            return _subMeshIndices[index];
        }
        public void SetIndices(int[][] indices)
        {
            _subMeshIndices = indices;
            _subMeshCount = indices.Length;
        }
        public void FillUVs()
        {
            int length = Vertices.Length;
            UVs = new Vector2[length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                UVs[i] = Vector2.zero;
            }
        }
        public void AssignRagdoll(DynamicRagdoll dynamicRagdoll)
        {
            DynamicRagdoll = dynamicRagdoll;
            Assignments = dynamicRagdoll.Assignments;
        }

        public void SetupRagdoll()
        {
            //foreach(var key in ColliderGroups.Keys)
            //{
            //    if (ColliderGroups[key].Length == DynamicRagdoll.Parts[key].Size) //are all vertices of that group present?
            //        continue;

            //    //calc center
            //    Vector3[] vertices = ColliderGroups[key];
            //    Vector3 center = new Vector3();
            //    for (int i = 0; i < vertices.Length; i++)
            //    {
            //        center += vertices[i];
            //    }
            //    center /= vertices.Length;

            //    Bounds bounds = new Bounds(center, new Vector3(0, 0, 0));
            //    for(int i = 0; i < vertices.Length; i++)
            //    {
            //        bounds.Encapsulate(vertices[i]);
            //    }
            //}
        }
    }


}

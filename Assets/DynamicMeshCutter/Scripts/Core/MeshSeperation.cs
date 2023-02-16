using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicMeshCutter
{
    public static class MeshSeperation
    {
        /// <summary>
        /// flood fill algorithm which finds clusters of non adjancent vertices based on UniqueVertices data
        /// </summary>
        public static void FloodFillClusters(DynamicMesh mesh)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            int[][] allSubIndices = new int[mesh.SubIndices.Count][];
            for (int i = 0; i < allSubIndices.Length; i++)
            {
                allSubIndices[i] = mesh.SubIndices[i].ToArray();
            }
            Vector3[] vertices = mesh.Vertices.ToArray();

            Dictionary<Vector3, VertexInfo> uniqueVertices = new Dictionary<Vector3, VertexInfo>();

            //Debug.Log("FLOOD FILL START");

            for (int sub = 0; sub < allSubIndices.Length; sub++)
            {
                int[] subIndices = allSubIndices[sub];
                for (int i = 0; i < subIndices.Length; i++)
                {
                    int index = subIndices[i];
                    Vector3 vertex = vertices[index];

                    if (!uniqueVertices.ContainsKey(vertex))
                    {
                        uniqueVertices.Add(vertex, new VertexInfo(index,sub));
                    }
                    else
                    {
                        uniqueVertices[vertex].Occasions_Vertex.Add(index);
                        uniqueVertices[vertex].Occasions_Submesh.Add(sub);
                    }
                }
            }

            //Debug.Log($"After First Loop - {watch.ElapsedMilliseconds} ms");

            Vector3 b = new Vector3();
            Vector3 c = new Vector3();

            foreach (KeyValuePair<Vector3, VertexInfo> entry in uniqueVertices) //improve this loop
            {
                List<int> occassions = entry.Value.Occasions_Vertex;
                List<Vector3> neighbors = entry.Value.Neighbors;
                for (int j = 0; j < occassions.Count; j++)
                {
                    int verticeIndex = occassions[j];
                    int r = verticeIndex % 3;

                    switch (r)
                    {
                        case 0:
                            b = vertices[verticeIndex + 1];
                            c = vertices[verticeIndex + 2];
                            break;
                        case 1:
                            b = vertices[verticeIndex - 1];
                            c = vertices[verticeIndex + 1];
                            break;
                        case 2:
                            b = vertices[verticeIndex - 2];
                            c = vertices[verticeIndex - 1];
                            break;
                    }

                    if (!neighbors.Contains(b))
                        neighbors.Add(b);
                    if (!neighbors.Contains(c))
                        neighbors.Add(c);
                }
            }

            int[] cluster = new int[vertices.Length];
            for (int i = 0; i < cluster.Length; i++)
                cluster[i] = -1;

            int[] subMesh = new int[vertices.Length];
            for (int i = 0; i < subMesh.Length; i++)
                subMesh[i] = -1;

            //Debug.Log($"After Dic Loop - {watch.ElapsedMilliseconds} ms");

            List<Vector3> unvisited = uniqueVertices.Keys.ToList();
            List<Vector3> visited = new List<Vector3>();
            List<Vector3> gray = new List<Vector3>();

            void PopulateGrey(Vector3 vertex)
            {
                List<Vector3> adj = uniqueVertices[vertex].Neighbors;
                foreach (var n in adj)
                {
                    if (!visited.Contains(n) && !gray.Contains(n))
                        gray.Add(n);
                }
            }

            Vector3 pop;
            int group = 0;
            int popped = 0;
            while (unvisited.Count > 0)
            {
                gray.Add(unvisited[0]); //feed the first vertex

                while (gray.Count > 0)
                {
                    pop = gray[0];
                    popped++;
                    gray.RemoveAt(0);
                    unvisited.Remove(pop);
                    visited.Add(pop);
                    PopulateGrey(pop);
                    var info = uniqueVertices[pop];
                    var ocVertices = info.Occasions_Vertex;
                    var ocSubmeshes = info.Occasions_Submesh;

                    int ocLength = ocVertices.Count;
                    for(int i =0;i< ocLength; i++)
                    {
                        cluster[ocVertices[i]] = group; //assign the vertice a cluster group
                        subMesh[ocVertices[i]] = ocSubmeshes[i]; //what submesh does this vertex belong to ?
                    }
                }

                group++;

                if (unvisited.Count > 0)
                {
                    visited.Clear();
                    gray.Clear();

                }
            }

            watch.Stop();
            //Debug.Log($"After Flood Fill - {watch.ElapsedMilliseconds} ms");

            mesh.AmountOfClusters = group;
            mesh.AmountOfUniqueVertices = visited.Count;
            mesh.Cluster = cluster;
            mesh.SubMesh = subMesh;
        }

        public static VirtualMesh[] GetVirtualMeshes(DynamicMesh dynamicMesh, bool seperateByClusters)
        {
            if (!seperateByClusters)
            {
                return new VirtualMesh[] { ConstructVirtualMeshFromDynamic(dynamicMesh) };
            }
            else
            {
                FloodFillClusters(dynamicMesh); //runs a floodfill algorithm to find clusters. also finds the unique vertice amount.

                int numberClusters = dynamicMesh.AmountOfClusters;
                int numberSubMeshes = dynamicMesh.SubIndices.Count;

                DynamicMesh[] dynamicMeshes = new DynamicMesh[numberClusters];
                VirtualMesh[] vMeshes = new VirtualMesh[numberClusters];
                for (int i = 0; i < numberClusters; i++)
                {
                    dynamicMeshes[i] = new DynamicMesh();
                    for (int j = 0; j < numberSubMeshes; j++)
                    {
                        dynamicMeshes[i].SubIndices.Add(new List<int>());
                    }
                    if (dynamicMesh.DynamicRagdoll != null)
                        dynamicMeshes[i].DynamicRagdoll = dynamicMesh.DynamicRagdoll;
                }

                //construct new virtual meshes from triangles and cluster index
                List<int> triangles = dynamicMesh.Triangles;
                int[] triangle = new int[3] { 0, 0, 0 };
                for (int i = 0; i < triangles.Count; i += 3)
                {

                    triangle[0] = triangles[i];
                    triangle[1] = triangles[i + 1];
                    triangle[2] = triangles[i + 2];

                    int cluster = dynamicMesh.Cluster[triangle[0]];
                    int submesh = dynamicMesh.SubMesh[triangle[0]];

                    try
                    {
                        dynamicMeshes[cluster].AddTriangle(triangle, submesh, dynamicMesh);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        Debug.LogError(e);
                    }
                }

                for (int i = 0; i < numberClusters; i++)
                {
                    vMeshes[i] = ConstructVirtualMeshFromDynamic(dynamicMeshes[i]);
                }

                return vMeshes;
            }
        }

        private static VirtualMesh ConstructVirtualMeshFromDynamic(DynamicMesh dynamicMesh)
        {
            VirtualMesh virtualMesh = new VirtualMesh
            {
                Vertices = dynamicMesh.Vertices.ToArray(),
                Triangles = dynamicMesh.Triangles.ToArray(),
                Normals = dynamicMesh.Normals.ToArray(),
                UVs = dynamicMesh.UVs.ToArray(),
                BoneWeights = dynamicMesh.BoneWeights.ToArray(),
                UniqueVerticesCount = dynamicMesh.AmountOfUniqueVertices,
                
            };

            if (dynamicMesh.DynamicRagdoll != null)
            {
                virtualMesh.DynamicRagdoll = dynamicMesh.DynamicRagdoll;
                virtualMesh.Assignments = dynamicMesh.RD.ToArray();
                virtualMesh.DynamicGroups = dynamicMesh.ColliderGroups.ToDictionary(entry => entry.Key, entry => entry.Value.ToArray());

                //calc new bounds here?

            }

            int[][] subIndices = new int[dynamicMesh.SubIndices.Count][];
            for (int i = 0; i < dynamicMesh.SubIndices.Count; i++)
            {
                subIndices[i] = dynamicMesh.SubIndices[i].ToArray();
            }
            virtualMesh.SetIndices(subIndices);
            return virtualMesh;
        }
    }
}


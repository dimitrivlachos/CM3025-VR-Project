using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DynamicMeshCutter
{
    public class MeshCutting
    {
        class InternalData
        {
            public InternalData(VirtualMesh meshTarget, VirtualPlane plane)
            {
                MeshTarget = meshTarget;
                Plane = plane;

                for (int i = 0; i < 2; i++)
                {
                    Sides[i] = new DynamicMesh();
                    Sides[i].SetTargetMesh(meshTarget);
                    if (MeshTarget.DynamicRagdoll != null)
                    {
                        Sides[i].DynamicRagdoll = MeshTarget.DynamicRagdoll;
                    }
                }
            }

            public VirtualMesh MeshTarget;
            public VirtualPlane Plane;
            public DynamicMesh[] Sides = new DynamicMesh[2];

            public List<Vector3> AddedVertices = new List<Vector3>();
            public List<BoneWeight> AddedBoneweights = new List<BoneWeight>();
        }

        public VirtualMesh[] Cut(ref Info info)
        {
            var plane = info.Plane;
            bool doSeperateMeshes = info.MeshTarget.SeparateMeshes;
            VirtualMesh targetVirtualMesh = info.TargetVirtualMesh;

            //fill in uvs if missing
            if(targetVirtualMesh.UVs.Length == 0)
            {
                targetVirtualMesh.FillUVs();
            }

            InternalData data = new InternalData(targetVirtualMesh, plane);

            int subMeshCount = info.TargetVirtualMesh.SubMeshCount;
            int[] sides = new int[3]; //left == 0, right == 1
            int[] indicesOfSubmesh;
            int[] triangle = new int[3];

            for (int sub = 0; sub < subMeshCount; sub++)
            {
                //get indices of target submesh
                indicesOfSubmesh = targetVirtualMesh.GetIndices(sub);

                for (int i = 0; i < 2; i++)
                {
                    data.Sides[i].SubIndices.Add(new List<int>());
                }

                //determine side of vertices of triangle.
                for (int i = 0; i < indicesOfSubmesh.Length; i += 3)
                {
                    triangle[0] = indicesOfSubmesh[i];
                    triangle[1] = indicesOfSubmesh[i + 1];
                    triangle[2] = indicesOfSubmesh[i + 2];

                    sides[0] = plane.GetSide(data.MeshTarget.Vertices[triangle[0]]);
                    sides[1] = plane.GetSide(data.MeshTarget.Vertices[triangle[1]]);
                    sides[2] = plane.GetSide(data.MeshTarget.Vertices[triangle[2]]);

                    if (sides[0] == sides[1] && sides[0] == sides[2]) //all vertices are on the same side
                    {
                        data.Sides[sides[0]].AddTriangle(triangle, sub); //add triangle vertices to left (sides[0] == 0) or right (sides[0] == 1) side
                    }
                    else
                    {
                        CutTriangle(sub, sides, triangle, data);
                    }
                }
            }

            //if all vertices are on one side we can exit
            if (data.Sides[0].Vertices.Count == 0 || data.Sides[1].Vertices.Count == 0)
            {
                return null;
            }

            //+1 submesh per side for the NEW FACE that we create
            for (int i = 0; i < 2; i++)
            {
                data.Sides[i].SubIndices.Add(new List<int>());
            }

            CreateFaces(data,ref info);

            int[] size = new int[2];
            VirtualMesh[][] separatedMeshes = new VirtualMesh[2][];
            for (int i = 0; i < 2; i++)
            {
                separatedMeshes[i] = MeshSeperation.GetVirtualMeshes(data.Sides[i], doSeperateMeshes);
                size[i] = separatedMeshes.Length;
            }

            //combine meshes of both sides
            VirtualMesh[] meshes = separatedMeshes[0].Concat(separatedMeshes[1]).ToArray();
            //capture which side the meshes belong to
            //assign top or bottom to targets
            info.Sides = new int[meshes.Length];
            info.BT  = new int[meshes.Length];
            int topSide = info.Plane.WorldNormal.y < 0 ? 1 : 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                info.Sides[i] = (i < separatedMeshes[0].Length) ? 0 : 1;
                info.BT[i] = (info.Sides[i] == topSide) ? 1 : 0;
            }

            return meshes;
        }

        private void CutTriangle(int submesh, int[] sides, int[] triangle, InternalData data)
        {
            // first index is side, second index is first or second vertex of that side. one vertex of one side will be empty, sine the split of the triangle is 2-1 or 1-2
            Vector3[,] vertices = new Vector3[3, 2]; //left, right, new created
            Vector3[,] normals = new Vector3[3, 2];
            Vector2[,] uvs = new Vector2[3, 2];
            BoneWeight[,] boneweights = new BoneWeight[3, 2];
            int[,] rd = new int[3, 2];
            bool[] didset = new bool[2] { false, false };

            bool hasBoneWeights = data.MeshTarget.HasBoneWeight;
            bool doDynamicRagdoll = data.MeshTarget.Assignments != null;
            //fill the above data based on the triangle
            int index;
            for (int i = 0; i < 3; i++)
            {
                index = triangle[i];
                int side = sides[i];

                if (!didset[side])
                {
                    didset[side] = true;

                    for (int j = 0; j < 2; j++)
                    {
                        vertices[side, j] = data.MeshTarget.Vertices[index];
                        normals[side, j] = data.MeshTarget.Normals[index];
                        uvs[side, j] = data.MeshTarget.UVs[index];
                        if (hasBoneWeights)
                            boneweights[side, j] = data.MeshTarget.BoneWeights[index];
                        if (doDynamicRagdoll)
                            rd[side, j] = data.MeshTarget.Assignments[index];
                    }
                }
                else
                {
                    vertices[side, 1] = data.MeshTarget.Vertices[index];
                    normals[side, 1] = data.MeshTarget.Normals[index];
                        uvs[side, 1] = data.MeshTarget.UVs[index];
                    if (hasBoneWeights)
                        boneweights[side, 1] = data.MeshTarget.BoneWeights[index];
                    if (doDynamicRagdoll)
                        rd[side, 1] = data.MeshTarget.Assignments[index];
                }
            }

            float distance = 0f;
            float distanceNormalized = 0f;

            //did we cut a triangle of the same ragdoll part?
            int part = -1;
            if (doDynamicRagdoll)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (rd[0, i] == rd[1, i])
                    {
                        part = rd[0,i];
                        break;
                    }
                }
            }

            //create two new data entries of the cut between the plane and the to edges of the triangle 
            for (int i = 0; i < 2; i++)
            {
                //calculate  distance between left and right vertix
                data.Plane.Raycast(new Ray(vertices[0, i], (vertices[1, i] - vertices[0, i]).normalized), out distance);
                distanceNormalized = distance / (vertices[1, i] - vertices[0, i]).magnitude;

                //create new data based on lerp distance
                vertices[2, i] = Vector3.Lerp(vertices[0, i], vertices[1, i], distanceNormalized);
                uvs[2, i] = Vector2.Lerp(uvs[0, i], uvs[1, i], distanceNormalized);
                normals[2, i] = Vector3.Lerp(normals[0, i], normals[1, i], distanceNormalized);
                boneweights[2, i] = new BoneWeight();
                rd[2, i] = -1;

                if (hasBoneWeights)
                {
                    boneweights[2, i].boneIndex0 = boneweights[0, i].boneIndex0;
                    boneweights[2, i].boneIndex1 = boneweights[0, i].boneIndex1;
                    boneweights[2, i].boneIndex2 = boneweights[0, i].boneIndex2;
                    boneweights[2, i].boneIndex3 = boneweights[0, i].boneIndex3;
                    boneweights[2, i].weight0 = Mathf.Lerp(boneweights[0, i].weight0, boneweights[1, i].weight0, distanceNormalized);
                    boneweights[2, i].weight1 = Mathf.Lerp(boneweights[0, i].weight1, boneweights[1, i].weight1, distanceNormalized);
                    boneweights[2, i].weight2 = Mathf.Lerp(boneweights[0, i].weight2, boneweights[1, i].weight2, distanceNormalized);
                    boneweights[2, i].weight3 = Mathf.Lerp(boneweights[0, i].weight3, boneweights[1, i].weight3, distanceNormalized);

                    data.AddedBoneweights.Add(boneweights[2, i]);
                }

                if (doDynamicRagdoll)
                {
                    rd[2, i] = part;
                }

                data.AddedVertices.Add(vertices[2, i]);
            }


            //create new triangles given the new vertices
            //there will be exactly three new triangles we need to create. one for the side of the singular vertex, two for the other side
            int singularSide = 0; //on which side of the plane is the singular vertice? the other side will have two of the three triangle vertices
            if (vertices[1, 0] == vertices[1, 1])
                singularSide = 1;

            //just for debugging
            //if (singularSide == 0 && vertices[0, 0] != vertices[0, 1])
            //{
            //    Debug.LogError("error in determining singular vertice of cut triangle");
            //}

            for (int i = 0; i < 2; i++)
            {
                data.Sides[i].AddTriangle(
                        new Vector3[] { vertices[i, 0], vertices[2, 0], vertices[2, 1] },
                        new Vector3[] { normals[i, 0], normals[2, 0], normals[2, 1] },
                        new Vector2[] { uvs[i, 0], uvs[2, 0], uvs[2, 1] },
                        new BoneWeight[] { boneweights[i, 0], boneweights[2, 0], boneweights[2, 1] },
                        new int[] {rd[i,0],rd[2,0],rd[2,1]},
                        normals[2, 0],
                        submesh
                        );

                //add the second triangle for the side with two of the original vertices
                if (singularSide != i)
                {
                    data.Sides[i].AddTriangle(
                      new Vector3[] { vertices[i, 0], vertices[i, 1], vertices[2, 1] },
                      new Vector3[] { normals[i, 0], normals[i, 1], normals[2, 1] },
                      new Vector2[] { uvs[i, 0], uvs[i, 1], uvs[2, 1] },
                      new BoneWeight[] { boneweights[i, 0], boneweights[i, 1], boneweights[2, 1] },
                      new int[] { rd[i, 0], rd[i, 1], rd[2, 1] },
                      normals[2, 1],
                      submesh
                      );
                }
            }
        }

        private void CreateFaces(InternalData data, ref Info info)
        {
            bool hasBoneWeights = data.MeshTarget.HasBoneWeight;
            //bool doDynamicRagdoll = data.MeshTarget.RD != null;

            //keep a list of vertices we've already visited
            List<Vector3> visited = new List<Vector3>();
            int length = data.AddedVertices.Count;
            try
            {
                for (int i = 0; i < length; i++)
                {
                    //ignore any already visited vertices
                    if (visited.Contains(data.AddedVertices[i]))
                        continue;

                    //list of vectors that create a loop. we will create a face for these vertices
                    //remark: this ignores any holes or vertices inside the face

                    List<Vector3> fVertices = new List<Vector3>(); //vertices of the new face
                    List<BoneWeight> fBoneWeights = new List<BoneWeight>(); //boneweights of the new face
                    List<int> fRD = new List<int>(); //ragdoll part of the new face

                    //we always add pairs of vertices
                    for (int j = 0; j < 2; j++)
                    {
                        //do modulo incase of wrap
                        int index = (i + j) % length;
                        visited.Add(data.AddedVertices[index]);

                        fVertices.Add(data.AddedVertices[index]);
                        if (hasBoneWeights)
                            fBoneWeights.Add(data.AddedBoneweights[index]); 
                        
                    }

                    bool loopIsCompleted = false;
                    while (!loopIsCompleted)
                    {
                        loopIsCompleted = true;

                        for (int k = 0; k < data.AddedVertices.Count; k += 2)
                        {
                            if (data.AddedVertices[k] == fVertices[fVertices.Count - 1] && !visited.Contains(data.AddedVertices[k + 1]))
                            {
                                loopIsCompleted = false;
                                visited.Add(data.AddedVertices[k + 1]);
                                fVertices.Add(data.AddedVertices[k + 1]);

                                if (hasBoneWeights)
                                {
                                    fBoneWeights.Add(data.AddedBoneweights[k + 1]);
                                }
                            }
                            else if (data.AddedVertices[k + 1] == fVertices[fVertices.Count - 1] && !visited.Contains(data.AddedVertices[k]))
                            {
                                loopIsCompleted = false;
                                visited.Add(data.AddedVertices[k]);
                                fVertices.Add(data.AddedVertices[k]);

                                if (hasBoneWeights)
                                {
                                    fBoneWeights.Add(data.AddedBoneweights[k]);
                                }
                            }
                        }
                    }

                    FillFace(data, fVertices, fBoneWeights,ref info);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogError(e);
            }

        }
        private void FillFace(InternalData data, List<Vector3> fVertices, List<BoneWeight> fBoneWeights, ref Info info)
        {
            bool hasBoneWeight = data.MeshTarget.HasBoneWeight;
            Vector3 center = GetVerticeCenter(fVertices);
            BoneWeight centerWeight = hasBoneWeight ? GetBoneweightCenter(fBoneWeights) : new BoneWeight();

            info.LocalFaceCenters.Add(center);

            //create vector3 that faces "upwards" the plane, perpendicular to the normal of the plane
            Vector3 upward = Vector3.zero;
            upward.x = data.Plane.LocalNormal.y;
            upward.y = -data.Plane.LocalNormal.x;
            upward.z = data.Plane.LocalNormal.z;

            Vector3 left = Vector3.Cross(data.Plane.LocalNormal, upward);

            Vector3 delta; //delta between vertice and center
            Vector3[] uv = new Vector3[2]; //new uvs

            for (int n = 0; n < fVertices.Count; n++)
            {
                delta = fVertices[n] - center;
                int o = (n + 1) % fVertices.Count;

                for (int j = 0; j < 2; j++)
                {
                    uv[j] = Vector3.zero;
                    uv[j].x = 0.5f + Vector3.Dot(delta, left);
                    uv[j].y = 0.5f + Vector3.Dot(delta, upward);
                    uv[j].z = 0.5f + Vector3.Dot(delta, data.Plane.LocalNormal);

                    //do modulo to account for looping of last vertex
                    delta = fVertices[o] - center;
                }

                int sign = -1;
                for (int j = 0; j < 2; j++)
                {
                    if (j == 1)
                        sign = 1;

                    BoneWeight[] boneweights = new BoneWeight[3];
                    if (!hasBoneWeight)
                    {
                        for (int b = 0; b < 3; b++)
                            boneweights[b] = new BoneWeight();
                    }
                    else
                    {
                        boneweights[0] = fBoneWeights[n];
                        boneweights[1] = fBoneWeights[o];
                        boneweights[2] = centerWeight;
                    }

                    data.Sides[j].AddTriangle(
                            new Vector3[] { fVertices[n], fVertices[o], center },
                            new Vector3[] { sign * data.Plane.LocalNormal, sign * data.Plane.LocalNormal, sign * data.Plane.LocalNormal },
                            new Vector2[] { uv[0], uv[1], new Vector2(0.5f, 0.5f) },
                            boneweights,
                            new int[] {-1,-1,-1}, //for now we ignore collider part of the newly added face vertices
                            sign * data.Plane.LocalNormal,
                            data.Sides[j].SubIndices.Count - 1);
                }
            }
        }

        public Vector3 GetVerticeCenter(List<Vector3> fVertices)
        {
            Vector3 center = new Vector3();
            int length = fVertices.Count;
            for(int i =0;i<length;i++)
            {
                center += fVertices[i];
            }
            return center / length;
        }
        public BoneWeight GetBoneweightCenter(List<BoneWeight> fBoneWeights)
        {
            BoneWeight center = new BoneWeight();
            Dictionary<int, float> boneWeights = new Dictionary<int, float>(); //<index,weight>

            for (int i = 0; i < fBoneWeights.Count; i++)
            {
                BoneWeight fWeight = fBoneWeights[i];

                boneWeights[fWeight.boneIndex0] = (boneWeights.ContainsKey(fWeight.boneIndex0)) ? boneWeights[fWeight.boneIndex0] + fWeight.weight0 : fWeight.weight0;
                boneWeights[fWeight.boneIndex1] = (boneWeights.ContainsKey(fWeight.boneIndex1)) ? boneWeights[fWeight.boneIndex1] + fWeight.weight1 : fWeight.weight1;
                boneWeights[fWeight.boneIndex2] = (boneWeights.ContainsKey(fWeight.boneIndex2)) ? boneWeights[fWeight.boneIndex2] + fWeight.weight2 : fWeight.weight2;
                boneWeights[fWeight.boneIndex3] = (boneWeights.ContainsKey(fWeight.boneIndex3)) ? boneWeights[fWeight.boneIndex3] + fWeight.weight3 : fWeight.weight3;
            }

            //get the heaviest weights. convert to array for faster sort

            var sorted = boneWeights.OrderByDescending(b => b.Value).ToArray();
            float total = 0;
            int length = (sorted.Length >= 4) ? 4 : sorted.Length;
            for (int i = 0; i < length; i++)
                total += sorted[i].Value;

            if(length > 0)
            {
                center.boneIndex0 = sorted[0].Key;
                center.weight0 = sorted[0].Value/ total;
            }
            if (length > 1)
            {
                center.boneIndex1 = sorted[1].Key;
                center.weight1 = sorted[1].Value/ total;
            }
            if (length > 2)
            {
                center.boneIndex2 = sorted[2].Key;
                center.weight2 = sorted[2].Value/ total;
            }
            if (length > 3)
            {
                center.boneIndex3 = sorted[3].Key;
                center.weight3 = sorted[3].Value/ total;
            }

            return center;
        }
    }
}


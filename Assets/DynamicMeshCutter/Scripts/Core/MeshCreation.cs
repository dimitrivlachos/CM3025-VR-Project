using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicMeshCutter
{
    public class MeshCreationData
    {
        public GameObject[] CreatedObjects;
        public MeshTarget[] CreatedTargets;

        public MeshCreationData(int size)
        {
            CreatedObjects = new GameObject[size];
            CreatedTargets = new MeshTarget[size];
        }
    }
    public static class MeshCreation
    {
        static float _ragdoll_vertex_threshold = 0.75f;

     
        public static MeshCreationData CreateObjects(Info info, Material defaultMaterial, int vertexCreationThreshold)
        {
            if (info.MeshTarget == null)
                return null;

            VirtualMesh[] createdMeshes = info.CreatedMeshes;

            MeshCreationData cData = new MeshCreationData(createdMeshes.Length);

            MeshTarget target = info.MeshTarget as MeshTarget;
            Material[] materials = MeshCreation.GetMaterials(target.gameObject);
            Material[] materialsNew = new Material[materials.Length + 1];

            materials.CopyTo(materialsNew, 0);
            materialsNew[materialsNew.Length - 1] = (target.FaceMaterial != null) ? target.FaceMaterial : defaultMaterial;
            materials = materialsNew;

            for (int i = 0; i < createdMeshes.Length; i++)
            {
                if (createdMeshes[i].Vertices.Length < vertexCreationThreshold)
                    continue;

                int bt = info.BT[i]; //is this meshtarget bottom or top ?

                Transform parent = null;
                GameObject root = null;


                VirtualMesh vMesh = createdMeshes[i];
                Mesh mesh = new Mesh();
                mesh.vertices = vMesh.Vertices;
                mesh.triangles = vMesh.Triangles;
                mesh.normals = vMesh.Normals;
                mesh.uv = vMesh.UVs;
                mesh.subMeshCount = vMesh.SubMeshCount;
                for (int j = 0; j < vMesh.SubMeshCount; j++)
                {
                    mesh.SetIndices(vMesh.GetIndices(j), MeshTopology.Triangles, j);
                }

                Behaviour behaviour = target.DefaultBehaviour[bt];

                if (vMesh.DynamicGroups != null)
                {
                    int[] keys = new int[vMesh.DynamicGroups.Keys.Count];
                    int index = 0;
                    foreach(var key in vMesh.DynamicGroups.Keys)
                    {
                        keys[index++] = key;
                    }
                    for (int j = 0; j < target.GroupBehaviours.Count; j++)
                    {
                        if (target.GroupBehaviours[j].Passes(keys))
                        {
                            behaviour = target.GroupBehaviours[j].Behaviour;
                            break;
                        }
                    }
                }


                switch (behaviour)
                {
                    case Behaviour.Stone:
                        CreateMesh(ref root, ref parent, target, mesh, vMesh, materials, bt);
                        break;
                    case Behaviour.Ragdoll:
                        DynamicRagdoll tRagdoll = target.DynamicRagdoll;
                        if (tRagdoll != null && vMesh.DynamicGroups.Count > 1)
                        {
                            if (WillBeValidRagdoll(tRagdoll, vMesh))
                                CreateRagdoll(ref root, ref parent, info, target, mesh, vMesh, materials, bt, behaviour);
                            else
                                CreateMesh(ref root, ref parent, target, mesh, vMesh, materials, bt, true);
                        }
                        else
                        {
                            CreateMesh(ref root, ref parent, target, mesh, vMesh, materials, bt, true);
                        }
                        break;
                    case Behaviour.Animation:
                        if (target.Animator != null)
                        {
                            CreateAnimatedMesh(ref root, ref parent, info, target, mesh, vMesh, materials, bt, behaviour);
                        }
                        else
                        {
                            Debug.LogWarning("Beahviour is set to Animation, but there was no Animator found in parent!");
                            CreateMesh(ref root, ref parent, target, mesh, vMesh, materials, bt, true);
                        }
                        break;
                }

                string prefix = $"({i}/{createdMeshes.Length})";
                parent.name = prefix + parent.name;
                parent.name = parent.name.Replace("(Clone)", "");

                var nTarget = root.GetComponent<MeshTarget>();
                if (nTarget == null)
                    nTarget = root.AddComponent<MeshTarget>();
                nTarget.GameobjectRoot = parent.gameObject;
                nTarget.OverrideFaceMaterial = target.OverrideFaceMaterial;
                nTarget.SeparateMeshes = target.SeparateMeshes;
                nTarget.ApplyTranslation = target.ApplyTranslation;
                nTarget.GroupBehaviours = target.GroupBehaviours;

                //target scale
                nTarget.transform.localScale = target.transform.localScale;

                //if inherting, both upper and lower side behaviour will remain the same. otherwise, both sides will have the same effect
                if (target.Inherit[bt])
                {
                    for (int j = 0; j < 2; j++)
                    {
                        nTarget.DefaultBehaviour[j] = target.DefaultBehaviour[j];
                        nTarget.CreateRigidbody[j] = target.CreateRigidbody[j];
                        nTarget.CreateMeshCollider[j] = target.CreateMeshCollider[j];
                        nTarget.Physics[j] = target.Physics[j];
                        nTarget.Inherit[j] = target.Inherit[j];
                    }
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        nTarget.DefaultBehaviour[j] = target.DefaultBehaviour[bt];
                        nTarget.CreateRigidbody[j] = target.CreateRigidbody[bt];
                        nTarget.Physics[j] = target.Physics[bt];
                        nTarget.CreateMeshCollider[j] = target.CreateMeshCollider[bt];
                        nTarget.Inherit[j] = false;
                    }
                }

                cData.CreatedObjects[i] = parent.gameObject;
                cData.CreatedTargets[i] = nTarget;
            }

            return cData;
        }

        static void CreateMesh(ref GameObject root, ref Transform parent, MeshTarget target, Mesh mesh, VirtualMesh vMesh, Material[] materials, int bt, bool forcePhysics = false)
        {
            parent = new GameObject($"{target.GameobjectRoot.name}").transform;
            parent.transform.rotation = target.transform.rotation;
            parent.transform.position = target.transform.position;
            parent.gameObject.tag = target.GameobjectRoot.tag;

            root = new GameObject($"{target.gameObject.name}");
            root.transform.position = target.transform.position;
            root.transform.rotation = target.transform.rotation;
            root.gameObject.tag = target.transform.tag;

            var filter = root.AddComponent<MeshFilter>();
            var renderer = root.AddComponent<MeshRenderer>();

            filter.mesh = mesh;
            renderer.materials = materials;

            Vector3 worldCenter = renderer.bounds.center;
            parent.transform.position = worldCenter;

            root.transform.SetParent(parent, true);
            //root.transform.localScale = target.transform.localScale; //test this

            if (target.CreateRigidbody[bt])
            {
                var rb = parent.gameObject.AddComponent<Rigidbody>();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            if (target.CreateMeshCollider[bt])
            {
                //only create when more than or equal unique vertices. if we don't run floodfill algorithm, the uniquevertice amount will be unset and equals -1
                if (vMesh.UniqueVerticesCount < 0 || vMesh.UniqueVerticesCount > 3 && vMesh.Vertices.Length > 20)
                {
                    MeshCollider collider = root.AddComponent<MeshCollider>();
                    //remark: BE CAREFUL ABOUT CONVEX MESH COLLIDER CREATION. THIS WILL THROW PHYSICS.PHYSX ERRORS IF MESH IS TOO SMALL.
                    collider.convex = true;
                }
            }
        }

        /// <summary>
        /// will they ragdoll have any valid colliders left after considering the cutoff threshold?
        /// </summary>
        /// <param name="ragdoll"></param>
        /// <param name="vMesh"></param>
        /// <returns></returns>
        static bool WillBeValidRagdoll(DynamicRagdoll ragdoll, VirtualMesh vMesh)
        {
            foreach(int key in ragdoll.Parts.Keys)
            {
                if (vMesh.DynamicGroups.ContainsKey(key))
                {
                    DynamicRagdollPart part = ragdoll.Parts[key];
                    Vector3[] vertices = vMesh.DynamicGroups[key];
                    float percent = ((float)vertices.Length / (float)part.Vertices.Length);
                    if (part.Colliders.Length > 0 && percent > _ragdoll_vertex_threshold)
                        return true;
                }
            }
            return false;
        }
        static void TrimRagdoll(DynamicRagdoll ragdoll, MeshTarget target, VirtualMesh vMesh)
        {
            ragdoll.Assignments = vMesh.Assignments;

            int[] keys = new int[ragdoll.Parts.Keys.Count];
            int index = 0;
            foreach (var key in ragdoll.Parts.Keys)
            {
                keys[index++] = key;
            }

            for(int i =0;i<keys.Length;i++)
            {
                int key = keys[i];
                DynamicRagdollPart part = ragdoll.Parts[key];
                if (vMesh.DynamicGroups.ContainsKey(key))
                {
                    Vector3[] vertices = vMesh.DynamicGroups[key];
                    float percent = ((float)vertices.Length / (float)part.Vertices.Length);
                    if (part.Colliders.Length > 0 && percent > _ragdoll_vertex_threshold)
                    {

                    }
                    else
                    {
                        for (int k = 0; k < part.Colliders.Length; k++)
                        {
                            GameObject.DestroyImmediate(part.Colliders[k]);
                        }
                        part.Colliders = new Collider[0] { };
                    }

                    part.Vertices = vertices;
                }
                else
                {
                    if (part.Joint != null)
                        GameObject.DestroyImmediate(part.Joint);
                    if (part.Rigidbody != null)
                        GameObject.DestroyImmediate(part.Rigidbody);
                    if (part.Colliders != null)
                    {
                        for (int k = 0; k < part.Colliders.Length; k++)
                        {
                            GameObject.DestroyImmediate(part.Colliders[k]);
                        }
                    }
                    GameObject.DestroyImmediate(part);
                    ragdoll.Parts.Remove(key);
                }
            }
        }
        static void CreateRagdoll(ref GameObject root, ref Transform parent, Info info, MeshTarget target, Mesh mesh, VirtualMesh vMesh, Material[] materials, int bt, Behaviour behaviour)
        {
            Transform rootBone = CreateSkinnedMeshRenderer(ref root, ref parent, info, target, mesh, vMesh, materials, bt, behaviour);

            parent.transform.position = target.GameobjectRoot.transform.position;
            parent.transform.rotation = target.GameobjectRoot.transform.rotation;

            DynamicRagdoll ragdoll = parent.GetComponent<DynamicRagdoll>();
            List<DynamicRagdollPart> parts = ragdoll.Parts.Values.ToList();

            if (parts.Count == 0)
            {
                Debug.LogError("This shouldnt happen. (Bugreport: Parts of ragdoll is 0)");
            }

            //find outermost "root" parts
            List<DynamicRagdollPart> roots = new List<DynamicRagdollPart>();
            List<DynamicRagdollPart> remainingPartsToCheck = ragdoll.Parts.Values.ToList();
            while (remainingPartsToCheck.Count > 0)
            {
                DynamicRagdollPart part = remainingPartsToCheck[0];
                var toRemove = remainingPartsToCheck[0].GetComponentsInChildren<DynamicRagdollPart>();
                for (int j = 0; j < toRemove.Length; j++)
                {
                    if (parts.Contains(toRemove[j]))
                        remainingPartsToCheck.Remove(toRemove[j]);
                }

                var ancestor = part.GetComponentInParentIgnoreSelf<DynamicRagdollPart>();
                if (ancestor != null && parts.Contains(ancestor))
                {
                    remainingPartsToCheck.Remove(part);
                }
                else
                {
                    remainingPartsToCheck.Remove(part);
                    roots.Add(part);
                }
            }

            //move all roots to top, aka. make them direct children of the parent. 
            var allKids = rootBone.transform.GetComponentsInChildren<Transform>(true);
            List<Transform> childrenToMove = new List<Transform>();
            for(int i = 0; i < allKids.Length; i++)
            {
                childrenToMove.Add(allKids[i]);
            }

            foreach (var r in roots)
            {
                r.transform.SetParent(parent);
                Transform[] rootChildren = r.transform.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < rootChildren.Length; j++)
                {
                    childrenToMove.Remove(rootChildren[j]);
                }
            }

            //flat hierarchy, move to closest root
            for (int j = 0; j < childrenToMove.Count; j++)
            {
                DynamicRagdollPart closestRoot = roots[0];
                for (int i = 1; i < roots.Count; i++)
                {
                    if (Vector3.Distance(roots[i].transform.position, childrenToMove[j].position) < Vector3.Distance(closestRoot.transform.position, target.transform.position))
                    {
                        closestRoot = roots[i];
                    }
                }
                childrenToMove[j].SetParent(closestRoot.transform);
            }

            //connect outer roots together (they need to be part of "one" ragdoll)
            if (roots.Count > 1)
            {
                for (int j = 0; j < roots.Count - 1; j++)
                {
                    roots[j].Joint.connectedBody = roots[j + 1].Rigidbody;
                }
            }

            bool hasCollider = false;

            //ensure inner roots have connected rigibody
            for (int j = 0; j < parts.Count; j++)
            {
                if (!hasCollider && parts[j].Colliders.Length > 0)
                    hasCollider = true;

                if (parts[j].Joint == null)
                    continue;
                if (parts[j].Joint.connectedBody == null)
                {
                    var rb = parts[j].GetComponentInParentIgnoreSelf<Rigidbody>();

                    if (rb != null)
                    {
                        parts[j].Joint.connectedBody = rb;
                    }
                    else
                    {
                        if (!roots.Contains(parts[j]))
                        {
                            Debug.LogError("false");
                        }
                    }
                }
            }

            if (!hasCollider)
            {
                Debug.LogError("Dynamic Ragdoll has no more collider");
            }
            //active physics for the rigidbody
            switch (target.Physics[bt])
            {
                case RagdollPhysics.LeaveAsIs:
                    break;
                case RagdollPhysics.NonKinematic:
                    ragdoll.SetRagdollKinematic(false);
                    break;
                case RagdollPhysics.Kinematic:
                    ragdoll.SetRagdollKinematic(true);
                    break;
            }
        }

        static void CreateAnimatedMesh(ref GameObject root, ref Transform parent, Info info, MeshTarget target, Mesh mesh, VirtualMesh vMesh, Material[] materials, int bt, Behaviour behaviour)
        {
            Animator tAnimator = target.Animator;

            if (target.IsSkinned)
            {
                CreateSkinnedMeshRenderer(ref root, ref parent, info, target, mesh, vMesh, materials, bt, behaviour);
            }
            else
            {
                //animator transform needs to match that of the original animator
                //parent.transform.position = tAnimator.transform.position;
                //parent.transform.rotation = tAnimator.transform.rotation;

                parent = GameObject.Instantiate(target.Animator.gameObject).transform;
                //parent.name = target.Animator.gameObject.name.Replace("(Clone)", "");
                root = parent.GetComponentInChildren<MeshTarget>().gameObject;

                var filter = root.GetComponent<MeshFilter>();
                var renderer = root.GetComponent<MeshRenderer>();
                filter.mesh = mesh;
                renderer.materials = materials;
            }

            parent.transform.position = tAnimator.transform.position;
            parent.transform.rotation = tAnimator.transform.rotation;

            //copy animator data and play
            AnimatorStateInfo tAnimatorStateInfo = tAnimator.GetCurrentAnimatorStateInfo(0);
            Animator nAnimator = parent.gameObject.GetComponent<Animator>(); //animator will be added on parent, not the meshtargets gameobject
            nAnimator.runtimeAnimatorController = tAnimator.runtimeAnimatorController;
            nAnimator.avatar = tAnimator.avatar;
            nAnimator.applyRootMotion = tAnimator.applyRootMotion;
            nAnimator.updateMode = tAnimator.updateMode;
            nAnimator.cullingMode = tAnimator.cullingMode;

            nAnimator.Play(tAnimatorStateInfo.fullPathHash, 0, tAnimatorStateInfo.normalizedTime);
        }



        /// <summary>
        /// duplicates the armature and returns the rootbone
        /// </summary>
        public static Transform CreateSkinnedMeshRenderer(ref GameObject meshRoot, ref Transform parent, Info info, MeshTarget target, Mesh mesh, VirtualMesh vMesh, Material[] materials, int bt, Behaviour behaviour)
        {
            parent = GameObject.Instantiate(target.GameobjectRoot).transform;
            var nRenderer = parent.GetComponentInChildren<SkinnedMeshRenderer>();
            meshRoot = nRenderer.gameObject;
            Transform rootbone = nRenderer.rootBone;

            if (target.DynamicRagdoll != null)
            {
                DynamicRagdoll nRagdoll = parent.GetComponent<DynamicRagdoll>();
                //keep but modifiy dynamic ragdoll component
                TrimRagdoll(nRagdoll, target, vMesh);
            }

            if (target.Animator != null)
            {
                Animator nAnimator = parent.GetComponent<Animator>();
                if (behaviour == Behaviour.Animation)
                {
                    //keep animator component
                }
                else
                {
                    //remove because no longer neeeded
                    GameObject.DestroyImmediate(nAnimator);
                }
            }

            mesh.bindposes = info.Bindposes;
            mesh.boneWeights = vMesh.BoneWeights;
            nRenderer.sharedMesh = mesh;
            nRenderer.materials = materials;

            return rootbone;
        }


        /// <summary>
        /// createdObjects/targerts can be NULL if the created object's vertices are below vertexCreationThreshold
        /// </summary>
        public static void TranslateCreatedObjects(Info info, GameObject[] createdObjects, MeshTarget[] targets, float separation)
        {
            if (createdObjects == null)
                return;

            VirtualPlane plane = info.Plane;

            for (int i = 0; i < createdObjects.Length; i++)
            {
                if (createdObjects[i] == null || targets[i] == null)
                    continue;

                //int bt = info.BT[i];
                if (!targets[i].ApplyTranslation)
                    continue;

                GameObject createdObject = createdObjects[i];

                int sign = 1;
                if (info.Sides[i] == 1)
                    sign = -1;

                Vector3 translation = sign * plane.WorldNormal.normalized * separation;
                createdObject.transform.position += translation;
            }
        }

        public static Material[] GetMaterials(GameObject target)
        {
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
                return renderer.materials;// sharedMaterials;

            SkinnedMeshRenderer sRenderer = target.GetComponent<SkinnedMeshRenderer>();
            if (sRenderer != null)
                return sRenderer.materials;

            return null;
        }

        public static T GetComponentInParentIgnoreSelf<T>(this Component target, bool includeInactive = false) where T : Component
        {
            Component[] allComponents = target.GetComponentsInParent<T>(includeInactive);
            foreach (var c in allComponents)
            {
                if (c.transform.gameObject != target.transform.gameObject)
                {
                    return c as T;
                }
            }
            return null;
        }


        public static void GetMeshInfo(MeshTarget target, out Mesh outMesh, out Matrix4x4[] outBindposes)
        {
            MeshFilter filter = target.GetComponent<MeshFilter>();
            if (filter != null)
            {
                outMesh = filter.mesh;
                outBindposes = new Matrix4x4[0];
                return;
            }

            SkinnedMeshRenderer renderer = target.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                Mesh mesh = new Mesh();
                renderer.BakeMesh(mesh);
                mesh.boneWeights = renderer.sharedMesh.boneWeights;
                outMesh = mesh;

                Matrix4x4 scale = Matrix4x4.Scale(target.transform.localScale).inverse;
                outBindposes = new Matrix4x4[renderer.bones.Length];
                bool failed = false;
                for (int i = 0; i < renderer.bones.Length; i++)
                {
                    if (renderer.bones[i] == null)
                    {
                        failed = true;
                        break;
                    }

                    outBindposes[i] = renderer.bones[i].worldToLocalMatrix * target.transform.localToWorldMatrix * scale;
                }

                if (failed)
                {
                    outBindposes = new Matrix4x4[0];
                    return;
                }
                else
                {
                    return;
                }
            }

            outMesh = null;
            outBindposes = new Matrix4x4[0];
        }
    }
}